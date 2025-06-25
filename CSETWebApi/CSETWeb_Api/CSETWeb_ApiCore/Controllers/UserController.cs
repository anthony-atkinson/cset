using System;
using CSETWebCore.Api.Models;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.Notification;
using CSETWebCore.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;
using System.Collections.Generic;
using System.Linq;
using CSETWebCore.Business.Authorization;
using CSETWebCore.Interfaces.Helpers;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for user management functionality in CSET.
    /// This controller handles user administration operations including user listing,
    /// activation/deactivation, role management, and user account operations.
    /// Supports both API key authentication for external integrations and token-based
    /// authentication for internal operations. Requires authentication and authorization
    /// via CsetAuthorize attribute for most operations.
    /// </summary>
    [ApiController]
    [CsetAuthorize]
    public class UserController : ControllerBase
    {
        private readonly CSETContext _context;
        private readonly IUserBusiness _userBusiness;
        private readonly INotificationBusiness _notificationBusiness;
        private readonly IConfiguration _configuration;
        private readonly ITokenManager _tokenManager;

        /// <summary>
        /// Initializes a new instance of the UserController.
        /// </summary>
        /// <param name="context">Database context for user operations</param>
        /// <param name="userBusiness">User business logic service</param>
        /// <param name="notificationBusiness">Notification business logic service</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="tokenManager">Token manager for authentication and authorization</param>
        public UserController(CSETContext context,
            IUserBusiness userBusiness,
            INotificationBusiness notificationBusiness,
            IConfiguration configuration, ITokenManager tokenManager)
        {
            _context = context;
            _userBusiness = userBusiness;
            _notificationBusiness = notificationBusiness;
            _configuration = configuration;
            _tokenManager = tokenManager;
            
        }


        /// <summary>
        /// Retrieves a collection of users in the system with optional filtering.
        /// </summary>
        /// <param name="onlyInactive">Optional filter to return only inactive users</param>
        /// <param name="apiKey">API key for authentication (required for external access)</param>
        /// <returns>
        /// 200 OK with list of UserAdmin objects if successful
        /// 401 Unauthorized if API key is invalid
        /// </returns>
        /// <remarks>
        /// Returns a list of users in the system with basic user information.
        /// Supports filtering to show only inactive users when onlyInactive is true.
        /// Requires valid API key authentication for external access.
        /// Returns user data including ID, name, email, and active status.
        /// </remarks>
        [HttpGet]
        [Route("api/users")]
        public IActionResult GetUsers([FromQuery] bool? onlyInactive, [FromQuery] string apiKey)
        {           
            if (!IsApiKeyValid(apiKey))
            {
                return Unauthorized();
            }

            var resp = new List<UserAdmin>();


            var query = _context.USERS.AsQueryable();

            // the consumer can limit the response to inactive users only
            if (onlyInactive ?? false)
            {
                query =  _context.USERS.Where(x => !x.IsActive);
            }

            query.ToList().ForEach(u => 
            {
                var user = new UserAdmin() {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PrimaryEmail = u.PrimaryEmail,
                    IsActive = u.IsActive
                };

                resp.Add(user);
            });

            return Ok(resp);
        }


        /// <summary>
        /// Activates or deactivates a user account and sends temporary password if activating.
        /// </summary>
        /// <param name="userId">ID of the user to modify</param>
        /// <param name="isActive">New active status for the user</param>
        /// <param name="apiKey">API key for authentication (required for external access)</param>
        /// <returns>
        /// 200 OK if user activation status changed successfully
        /// 400 Bad Request if user not found or no change needed
        /// 401 Unauthorized if API key is invalid
        /// </returns>
        /// <remarks>
        /// Changes the active status of a user account. When activating a user (isActive = true),
        /// automatically sends a temporary password email to the user's primary email address.
        /// Requires valid API key authentication for external access.
        /// Logs activation changes and password reset operations for audit purposes.
        /// Returns immediately if no status change is needed.
        /// </remarks>
        [HttpGet]
        [Route("api/user/activate")]
        public IActionResult ChangeUserActivation(
            [FromQuery] int userId, [FromQuery] bool isActive, [FromQuery] string apiKey)
        {
            LogManager.GetCurrentClassLogger().Info($"ChangeUserActivation:  changing isActive property to {isActive}");

            if (!IsApiKeyValid(apiKey))
            {
                return Unauthorized();
            }

            var user = _context.USERS.FirstOrDefault(x => x.UserId == userId);
            if (user == null)
            {
                return BadRequest();
            }


            // return if no change
            if (isActive == user.IsActive)
            {
                return Ok();
            }


            user.IsActive = isActive;
            _context.SaveChanges();


            // if the user is being activated, send them a new temp password
            if (isActive)
            {
                LogManager.GetCurrentClassLogger().Info($"ChangeUserActivation:  sending temporary password email to {user.PrimaryEmail}");

                var resetter = new UserAccountSecurityManager(_context, _userBusiness, _notificationBusiness, _configuration);
                resetter.ResetPassword(user.PrimaryEmail, "Temporary Password", "CSET");
            }


            // TODO:  What sort of response should we send?
            return Ok();
        }


        /// <summary>
        /// Validates the specified API key against the secret stored in the database.
        /// </summary>
        /// <param name="apiKey">API key to validate</param>
        /// <returns>True if the API key is valid, false otherwise</returns>
        /// <remarks>
        /// Internal method used to validate API keys for external integrations.
        /// Compares the provided API key against the UserApprovalApiKey stored in global properties.
        /// Returns false for null or empty API keys.
        /// </remarks>
        private bool IsApiKeyValid(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            var gp = new CSETGlobalProperties(_context);
            var secret = gp.GetProperty("UserApprovalApiKey");
            return (apiKey == secret);
        }
        
        /// <summary>
        /// Retrieves the current user's role information.
        /// </summary>
        /// <returns>
        /// 200 OK with role information if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns the role of the currently authenticated user.
        /// Uses the token manager to identify the current user and retrieve their role.
        /// Returns role information in JSON format with Role property.
        /// Logs errors for debugging purposes if role retrieval fails.
        /// </remarks>
        [HttpGet]
        [Route("api/getRole")]
        public IActionResult GetRole()
        {
            try
            {
                var userId = _tokenManager.GetCurrentUserId();
                var role = _userBusiness.GetRole(userId);
                return Ok(new { Role = role });
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
            }

            return Ok(); 
        }
        
        /// <summary>
        /// Retrieves all users from the database with full user information.
        /// </summary>
        /// <returns>
        /// 200 OK with list of all users if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns comprehensive user information for all users in the system.
        /// Uses the user business service to retrieve complete user data.
        /// Includes both active and inactive users in the response.
        /// Logs errors for debugging purposes if user retrieval fails.
        /// </remarks>
        [HttpGet]
        [Route("api/getusers")]
        public IActionResult GetAllUsers()
        {           
            try
            {
                var users = _userBusiness.GetUsers();
                return Ok(users);
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
            }

            return Ok();
        }
        
        /// <summary>
        /// Updates the role of a specific user.
        /// </summary>
        /// <param name="user">UserRole object containing user ID and new role ID</param>
        /// <returns>
        /// 200 OK if role updated successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Updates the role assignment for the specified user.
        /// Uses the user business service to perform the role update operation.
        /// Logs errors for debugging purposes if role update fails.
        /// Requires proper authorization to modify user roles.
        /// </remarks>
        [HttpPost]
        [Route("api/updateuser")]
        public IActionResult UpdateUserRole([FromBody] UserRole user)
        {
            try
            {
                _userBusiness.UpdateRole(user.RoleId, user.UserId);
                return Ok();
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
            }

            return Ok(); 
        }
        
        /// <summary>
        /// Retrieves all available roles from the database.
        /// </summary>
        /// <returns>
        /// 200 OK with list of available roles if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns all roles that can be assigned to users in the system.
        /// Uses the user business service to retrieve role information.
        /// Provides role data for user management interfaces.
        /// Logs errors for debugging purposes if role retrieval fails.
        /// </remarks>
        [HttpGet]
        [Route("api/getavailableroles")]
        public IActionResult GetAvailableRoles()
        {
            try
            {
                return Ok(_userBusiness.GetAvailableRoles());
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
            }

            return Ok(); 
        }
    }
}
