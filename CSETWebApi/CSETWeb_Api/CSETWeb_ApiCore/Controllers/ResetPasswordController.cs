//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Authorization;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Reports;
using CSETWebCore.Interfaces.User;
using CSETWebCore.Model.Authentication;
using CSETWebCore.Interfaces.Notification;
using CSETWebCore.Model.Password;
using CSETWebCore.Model.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSETWebCore.Model.Auth;
using CSETWebCore.Api.Models;
using NLog;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for password management and user registration functionality in CSET.
    /// This controller handles password resets, password changes, user registration, and security
    /// question management. Includes comprehensive password complexity validation and email
    /// verification. Supports both authenticated and unauthenticated operations for different
    /// password management scenarios.
    /// </summary>
    public class ResetPasswordController : ControllerBase
    {
        private Regex emailvalidator = new Regex(@"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$");

        private readonly IUserAuthentication _userAuthentication;
        private readonly ITokenManager _tokenManager;
        private readonly CSETContext _context;
        private readonly IAssessmentUtil _assessmentUtil;
        private readonly IAdminTabBusiness _adminTabBusiness;
        private readonly IReportsDataBusiness _reports;
        private readonly IUserBusiness _userBusiness;
        private readonly INotificationBusiness _notificationBusiness;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHost;

        /// <summary>
        /// Initializes a new instance of the ResetPasswordController.
        /// </summary>
        /// <param name="userAuthentication">User authentication service</param>
        /// <param name="tokenManager">Token manager for authentication and authorization</param>
        /// <param name="context">Database context for user operations</param>
        /// <param name="assessmentUtil">Assessment utility service</param>
        /// <param name="adminTabBusiness">Admin tab business logic service</param>
        /// <param name="reports">Reports data business service</param>
        /// <param name="userBusiness">User business logic service</param>
        /// <param name="notificationBusiness">Notification business service</param>
        /// <param name="configuration">Configuration service for application settings</param>
        /// <param name="webHost">Web hosting environment service</param>
        public ResetPasswordController(IUserAuthentication userAuthentication, ITokenManager tokenManager, CSETContext context,
             IAssessmentUtil assessmentUtil, IAdminTabBusiness adminTabBusiness, IReportsDataBusiness reports,
             IUserBusiness userBusiness, INotificationBusiness notificationBusiness, IConfiguration configuration, IWebHostEnvironment webHost)
        {
            _userAuthentication = userAuthentication;
            _tokenManager = tokenManager;
            _context = context;
            _assessmentUtil = assessmentUtil;
            _adminTabBusiness = adminTabBusiness;
            _reports = reports;
            _userBusiness = userBusiness;
            _notificationBusiness = notificationBusiness;
            _configuration = configuration;
            _webHost = webHost;
        }

        /// <summary>
        /// Checks if the current user's password requires a reset.
        /// </summary>
        /// <returns>
        /// 200 OK with boolean indicating if password reset is required
        /// 400 Bad Request if model state is invalid or user not found
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Determines if the current user needs to reset their password.
        /// Returns false if using an access key (no password expiration).
        /// Used to prompt users to change their password when required.
        /// Requires authentication via CsetAuthorize attribute.
        /// </remarks>
        [HttpGet]
        [Route("api/ResetPassword/ResetPasswordStatus")]
        [CsetAuthorize]
        public IActionResult GetResetPasswordStatus()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model State");
                }

                var userId = _tokenManager.GetUserId();
                var rval = _context.USERS.Where(x => x.UserId == userId).FirstOrDefault();
                if (rval != null)
                {
                    var resetRequired = rval.PasswordResetRequired;
                    return Ok(resetRequired);
                }

                // if an access key is used, there is no password to expire
                if (_tokenManager.GetAccessKey() != null)
                {
                    return Ok(false);
                }

                return BadRequest("Unknown error");
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }
        }

        /// <summary>
        /// Performs an actual password change for the authenticated user.
        /// </summary>
        /// <param name="changePass">ChangePassword object containing current and new password information</param>
        /// <returns>
        /// 200 OK with PasswordResponse indicating success or validation results
        /// 400 Bad Request if model state is invalid or email format is invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Changes the user's password after validating current password and complexity rules.
        /// Validates email format, current password, and new password complexity requirements.
        /// Checks against historical password reuse rules and complexity standards.
        /// Sets ResetRequired to false upon successful password change.
        /// Requires authentication via CsetAuthorize attribute.
        /// </remarks>
        [HttpPost]
        [Route("api/ResetPassword/ChangePassword")]
        [CsetAuthorize]
        public IActionResult PostChangePassword([FromBody] ChangePassword changePass)
        {
            UserAccountSecurityManager resetter = new UserAccountSecurityManager(_context, _userBusiness, _notificationBusiness, _configuration);
            var response = new PasswordResponse()
            {
                PasswordLengthMin = resetter.PasswordLengthMin,
                PasswordLengthMax = resetter.PasswordLengthMax,
                NumberOfHistoricalPasswords = resetter.NumberOfHistoricalPasswords,
                PasswordLengthMet = false,
                PasswordContainsNumbers = false,
                PasswordContainsLower = false,
                PasswordContainsUpper = false,
                PasswordContainsSpecial = false,
                PasswordNotReused = false
            };

            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsValid = false;
                    response.Message = "Invalid Model State";
                    return Ok(response);
                }
                if (!emailvalidator.IsMatch(changePass.PrimaryEmail.Trim()))
                {
                    response.IsValid = false;
                    response.Message = "Invalid Primary Email";
                    return Ok(response);
                }

                Login login = new Login()
                {
                    Email = changePass.PrimaryEmail,
                    Password = changePass.CurrentPassword
                };

                LoginResponse resp = _userAuthentication.Authenticate(login);
                if (resp == null)
                {
                    response.IsValid = false;
                    response.Message = "current invalid";
                    return Ok(response);
                }

                // does this new password follow the complexity rules?
                PasswordResponse respComplex = resetter.ComplexityRulesMet(changePass);
                if (!respComplex.PasswordContainsLower || !respComplex.PasswordContainsUpper || !respComplex.PasswordLengthMet ||
                    !respComplex.PasswordContainsSpecial || !respComplex.PasswordNotReused)
                {
                    respComplex.IsValid = false;
                    respComplex.Message = "rules not satisfied";
                    return Ok(respComplex);
                }

                bool rval = resetter.ChangePassword(changePass);
                if (rval)
                {
                    resp.ResetRequired = false;
                    _context.SaveChanges();

                    response.IsValid = true;
                    response.Message = "Created Successfully";
                    return Ok(response);
                }
                else
                {
                    response.IsValid = false;
                    response.Message = "Unknown error";
                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Validates password complexity rules without changing the password.
        /// </summary>
        /// <param name="changePass">ChangePassword object containing the new password to validate</param>
        /// <returns>
        /// 200 OK with PasswordResponse containing validation results
        /// </returns>
        /// <remarks>
        /// Checks if the provided password meets complexity requirements without performing
        /// the actual password change. Returns detailed validation results including
        /// length requirements, character type requirements, and historical password checks.
        /// Useful for real-time password validation in user interfaces.
        /// </remarks>
        [HttpPost]
        [Route("api/ResetPassword/CheckPassword")]
        public IActionResult CheckPassword([FromBody] ChangePassword changePass)
        {
            UserAccountSecurityManager resetter = new UserAccountSecurityManager(_context, _userBusiness, _notificationBusiness, _configuration);

            // does this new password follow the complexity rules?
            if (changePass.NewPassword == null)
            {
                return Ok(new PasswordResponse
                {
                    PasswordLengthMin = resetter.PasswordLengthMin,
                    PasswordLengthMax = resetter.PasswordLengthMax,
                    NumberOfHistoricalPasswords = resetter.NumberOfHistoricalPasswords,
                    PasswordLengthMet = false,
                    PasswordContainsNumbers = false,
                    PasswordContainsLower = false,
                    PasswordContainsUpper = false,
                    PasswordContainsSpecial = false,
                    PasswordNotReused = false
                });
            }

            PasswordResponse complexEnough = resetter.ComplexityRulesMet(changePass);
            return Ok(complexEnough);
        }

        /// <summary>
        /// Registers a new user account in the system.
        /// </summary>
        /// <param name="user">CreateUser object containing user registration information</param>
        /// <returns>
        /// 200 OK with status message if registration successful
        /// 400 Bad Request if validation fails or user already exists
        /// </returns>
        /// <remarks>
        /// Creates a new user account with comprehensive validation including email format,
        /// email confirmation matching, and email allowlist validation. Supports both beta
        /// mode (no email sent) and production mode (immediate email with temporary password).
        /// Validates against existing users to prevent duplicate accounts.
        /// Logs detailed error information for debugging purposes.
        /// </remarks>
        [HttpPost]
        [Route("api/ResetPassword/RegisterUser")]
        public IActionResult PostRegisterUser([FromBody] CreateUser user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LogManager.GetCurrentClassLogger().Error($"Invalid Model State: {JsonConvert.SerializeObject(ModelState)}");
                    return BadRequest("Invalid Model State");
                }

                if (String.IsNullOrWhiteSpace(user.PrimaryEmail))
                {
                    LogManager.GetCurrentClassLogger().Error("missing email");
                    return BadRequest("missing email");
                }

                if (!emailvalidator.IsMatch(user.PrimaryEmail))
                {
                    LogManager.GetCurrentClassLogger().Error($"invalid email format: ${JsonConvert.SerializeObject(user)}");
                    return BadRequest("invalid email format");
                }

                if (!emailvalidator.IsMatch(user.ConfirmEmail.Trim()))
                {
                    LogManager.GetCurrentClassLogger().Error($"invalid email format: ${JsonConvert.SerializeObject(user)}");
                    return BadRequest("invalid email format");
                }

                if (user.PrimaryEmail != user.ConfirmEmail)
                {
                    LogManager.GetCurrentClassLogger().Error($"emails do not match: ${JsonConvert.SerializeObject(user)}");
                    return BadRequest("emails do not match");
                }

                if (_userBusiness.GetUserDetail(user.PrimaryEmail) != null)
                {
                    LogManager.GetCurrentClassLogger().Error($"account already exists: ${JsonConvert.SerializeObject(user)}");
                    return BadRequest("account already exists");
                }

                // Validate the email against an allowlist (if defined by the host)
                var securityManager = new UserAccountSecurityManager(_context, _userBusiness, _notificationBusiness, _configuration);
                if (!securityManager.EmailIsAllowed(user.PrimaryEmail, _webHost))
                {
                    LogManager.GetCurrentClassLogger().Error($"email not allowed: ${JsonConvert.SerializeObject(user)}");
                    return BadRequest("email not allowed");
                }

                var resetter = new UserAccountSecurityManager(_context, _userBusiness, _notificationBusiness, _configuration);

                var gp = new CSETGlobalProperties(_context);
                var beta = gp.GetBoolProperty("IsCsetOnlineBeta") ?? false;

                if (beta)
                {
                    LogManager.GetCurrentClassLogger().Error("CreateUser - CSET is set to 'online beta' mode - no email sent to new user");

                    // create the user but DO NOT send the temp password email (test/beta)
                    var rval = resetter.CreateUser(user, false);
                    if (rval)
                    {
                        return Ok("waiting-for-approval");
                    }
                }
                else
                {
                    // create the user and send the temp password email immediately (production)
                    var rval = resetter.CreateUser(user, true);
                    if (rval)
                    {
                        return Ok("created-and-email-sent");
                    }
                }

                LogManager.GetCurrentClassLogger().Error($"Unknown error: {user}");
                return BadRequest("Unknown error");
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error($"... {e}");
                return BadRequest(e);
            }
        }

        /// <summary>
        /// Resets a user's password using security questions.
        /// </summary>
        /// <param name="answer">SecurityQuestionAnswer object containing question and answer</param>
        /// <returns>
        /// 200 OK if password reset successful
        /// 400 Bad Request if validation fails or user inactive
        /// 409 Conflict if security answer is incorrect
        /// </returns>
        /// <remarks>
        /// Resets a user's password after validating their security question answer.
        /// Validates email format and user account status before processing.
        /// Sends a temporary password email upon successful reset.
        /// Returns 409 Conflict instead of 401 Unauthorized to avoid JWT interceptor issues.
        /// </remarks>
        [HttpPost]
        [Route("api/ResetPassword")]
        public IActionResult ResetPassword([FromBody] SecurityQuestionAnswer answer)
        {
            try
            {
                answer.PrimaryEmail = answer.PrimaryEmail.Trim();

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!emailvalidator.IsMatch(answer.PrimaryEmail))
                {
                    LogManager.GetCurrentClassLogger().Error("reset password - emails don't match");
                    return BadRequest();
                }

                if (!_userBusiness.GetUserDetail(answer.PrimaryEmail).IsActive)
                {
                    LogManager.GetCurrentClassLogger().Error("reset password - user inactive");
                    return BadRequest("user inactive");
                }

                if (IsSecurityAnswerCorrect(answer))
                {
                    UserAccountSecurityManager resetter = new UserAccountSecurityManager(_context, _userBusiness, _notificationBusiness, _configuration);
                    bool rval = resetter.ResetPassword(answer.PrimaryEmail, "Password Reset", answer.AppName);

                    if (rval)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }

                // return Unauthorized();
                // returning a 401 (Unauthorized) gets caught by the JWT interceptor and dumps the user out, which we don't want.
                return Conflict();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of potential security questions for user registration.
        /// </summary>
        /// <param name="lang">Language code for question localization</param>
        /// <returns>
        /// 200 OK with list of security questions if successful
        /// 400 Bad Request if operation fails
        /// </returns>
        /// <remarks>
        /// Returns a list of available security questions for user registration.
        /// Supports localization through the lang parameter.
        /// Questions are used during user registration and password reset processes.
        /// </remarks>
        [HttpGet]
        [Route("api/ResetPassword/PotentialQuestions")]
        public IActionResult GetPotentialQuestions([FromQuery] string lang)
        {
            try
            {
                UserAccountSecurityManager resetter = new UserAccountSecurityManager(_context, _userBusiness, _notificationBusiness, _configuration);
                return Ok(resetter.GetSecurityQuestionList(lang));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Retrieves security questions for a specific user for password reset.
        /// </summary>
        /// <param name="email">Email address of the user</param>
        /// <param name="appName">Application name for the reset process</param>
        /// <returns>
        /// 200 OK with security questions if successful
        /// 400 Bad Request if user not found or inactive
        /// </returns>
        /// <remarks>
        /// Retrieves the security questions associated with a user's account.
        /// If no security questions are set, automatically triggers a password reset.
        /// Validates user existence and active status before returning questions.
        /// Used in the password reset workflow to challenge users with their security questions.
        /// </remarks>
        [HttpGet]
        [Route("api/ResetPassword/SecurityQuestions")]
        public IActionResult GetSecurityQuestions([FromQuery] string email, [FromQuery] string appName)
        {
            try
            {
                email = email.Trim();

                var user = _context.USERS.Where(x => String.Equals(x.PrimaryEmail, email)).FirstOrDefault();

                if (user == null)
                {
                    return BadRequest();
                }

                if (!user.IsActive)
                {
                    return BadRequest("user inactive");
                }

                var q = from b in _context.USER_SECURITY_QUESTIONS
                        join c in _context.USERS on b.UserId equals c.UserId
                        where c.PrimaryEmail == email
                        select new SecurityQuestions()
                        {
                            SecurityQuestion1 = b.SecurityQuestion1,
                            SecurityQuestion2 = b.SecurityQuestion2
                        };

                List<SecurityQuestions> questions = q.ToList();

                //note that you don't have to provide a security question
                //it will just reset if you don't 
                if (questions.Count == 0
                    || (questions[0].SecurityQuestion1 == null && questions[0].SecurityQuestion2 == null))
                {
                    UserAccountSecurityManager resetter = new UserAccountSecurityManager(_context, _userBusiness, _notificationBusiness, _configuration);
                    bool rval = resetter.ResetPassword(email, "Password Reset", appName);

                    return Ok(new List<SecurityQuestions>());
                }

                return Ok(questions);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Checks the user-supplied question and answer against the stored answer.
        /// </summary>
        /// <param name="answer">SecurityQuestionAnswer object containing question and answer to validate</param>
        /// <returns>True if the answer matches the stored answer, false otherwise</returns>
        /// <remarks>
        /// Private helper method that validates security question answers.
        /// Compares the provided question and answer against stored security questions.
        /// Supports both primary and secondary security questions.
        /// Used during password reset validation process.
        /// </remarks>
        private bool IsSecurityAnswerCorrect(SecurityQuestionAnswer answer)
        {
            var questions = from b in _context.USER_SECURITY_QUESTIONS
                            join c in _context.USERS on b.UserId equals c.UserId
                            where c.PrimaryEmail == answer.PrimaryEmail
                            && (
                                (b.SecurityQuestion1 == answer.QuestionText
                                    && b.SecurityAnswer1 == answer.AnswerText)
                                || (b.SecurityQuestion2 == answer.QuestionText
                                    && b.SecurityAnswer2 == answer.AnswerText)
                                )
                            select b;

            if ((questions != null) && questions.FirstOrDefault() != null)
                return true;
            return false;
        }
    }
}
