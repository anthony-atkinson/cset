//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Authorization;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Auth;
using CSETWebCore.Model.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides authentication and authorization endpoints for the CSET application.
    /// Supports both enterprise (multi-user) and standalone (single-user) deployment models.
    /// </summary>
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthentication _userAuthentication;
        private readonly ILocalInstallationHelper _localInstallationHelper;
        private readonly ITokenManager _tokenManager;
        private static readonly object _locker = new object();
        static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Initializes a new instance of the AuthController.
        /// </summary>
        /// <param name="userAuthentication">Service for user authentication operations</param>
        /// <param name="tokenManager">Service for JWT token management</param>
        /// <param name="localInstallationHelper">Helper for determining local installation status</param>
        public AuthController(IUserAuthentication userAuthentication, ITokenManager tokenManager, ILocalInstallationHelper localInstallationHelper)
        {
            _userAuthentication = userAuthentication;
            _localInstallationHelper = localInstallationHelper;
            _tokenManager = tokenManager;
        }


        /// <summary>
        /// Authenticates user credentials for enterprise deployments.
        /// </summary>
        /// <param name="login">Login credentials including username and password</param>
        /// <returns>
        /// 200 OK with LoginResponse if authentication successful
        /// 400 Bad Request if credentials are invalid or password is expired
        /// </returns>
        /// <remarks>
        /// This endpoint is used for enterprise deployments where users have registered accounts.
        /// The response includes authentication token and user information.
        /// 
        /// Sample request:
        ///     POST /api/auth/login
        ///     {
        ///         "Email": "user@example.com",
        ///         "Password": "SecurePassword123!"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/auth/login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(LoginResponse), 400)]
        public IActionResult Login([FromBody] Login login)
        {
            LoginResponse resp = _userAuthentication.Authenticate(login);

            if (resp == null)
            {
                return BadRequest(new LoginResponse());
            }

            if (resp.IsPasswordExpired)
            {
                return BadRequest(resp);
            }

            return Ok(resp);
        }


        /// <summary>
        /// Authenticates user for standalone (local) deployments.
        /// </summary>
        /// <param name="login">Login credentials</param>
        /// <returns>
        /// 200 OK with LoginResponse if authentication successful
        /// 500 Internal Server Error if authentication fails
        /// </returns>
        /// <remarks>
        /// This endpoint is specifically for standalone deployments where the application
        /// runs locally on a single machine. Authentication is simplified for local use.
        /// 
        /// Sample request:
        ///     POST /api/auth/login/standalone
        ///     {
        ///         "Email": "localuser@example.com",
        ///         "Password": "LocalPassword123!"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/auth/login/standalone")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(500)]
        public IActionResult LoginStandalone([FromBody] Login login)
        {
            try
            {
                _tokenManager.GenerateSecret();
                lock (_locker)
                {
                    LoginResponse resp = _userAuthentication.AuthenticateStandalone(login, _tokenManager);
                    if (resp != null)
                    {
                        return Ok(resp);
                    }

                    resp = new LoginResponse()
                    {
                        LinkerTime = new Helpers.BuildNumberHelper().GetLinkerTime()
                    };
                    return Ok(resp);
                }
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");

                _logger.Error(exc.Message);
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Determines if the current installation is a local (standalone) installation.
        /// </summary>
        /// <returns>
        /// 200 OK with boolean indicating if this is a local installation
        /// </returns>
        /// <remarks>
        /// This endpoint helps the client determine the deployment model and adjust
        /// authentication behavior accordingly. Local installations typically don't
        /// require user registration or complex authentication.
        /// </remarks>
        [HttpGet]
        [Route("api/auth/islocal")]
        [ProducesResponseType(typeof(bool), 200)]
        public IActionResult IsLocalInstallation()
        {
            return Ok(_localInstallationHelper.IsLocalInstallation());
        }


        /// <summary>
        /// Issues a new JWT token with optional assessment and aggregation context.
        /// </summary>
        /// <returns>
        /// 200 OK with new token if successful
        /// 401 Unauthorized if current token is invalid
        /// </returns>
        /// <remarks>
        /// This endpoint refreshes the current JWT token and can optionally include
        /// assessment or aggregation context in the new token.
        /// 
        /// Headers:
        /// - assessmentid: Optional assessment ID to include in token
        /// - expSeconds: Optional token expiration time in seconds
        /// - refresh: Set to any value to perform a pure refresh
        /// - aggregationid: Optional aggregation ID to include in token
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [CsetAuthorize]
        [HttpGet]
        [Route("api/auth/token")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        [ProducesResponseType(401)]
        public IActionResult IssueToken()
        {
            // get operating parameters from request header
            int assessmentId = StringToInt(Request.Headers["assessmentid"], -1);
            int expSeconds = StringToInt(Request.Headers["expSeconds"], -1);
            string refresh = Request.Headers["refresh"];
            if (string.IsNullOrEmpty(refresh))
            {
                refresh = "*default*";
            }
            int aggregationId = StringToInt(Request.Headers["aggregationid"], -1);



            int? currentUserId = _tokenManager.PayloadInt(Constants.Constants.Token_UserId);
            string accessKey = _tokenManager.Payload(Constants.Constants.Token_AccessKey);
            int? currentAssessmentId = _tokenManager.PayloadInt(Constants.Constants.Token_AssessmentId);
            int? currentAggregationId = _tokenManager.PayloadInt(Constants.Constants.Token_AggregationId);
            string scope = _tokenManager.Payload(Constants.Constants.Token_Scope);


            // If the 'refresh' parm was sent, this is a pure refresh
            if (refresh != "*default*")
            {
                // If the token has an assess ID, validate the user/assessment
                if (currentAssessmentId != null)
                {
                    _tokenManager.AssessmentForUser(currentUserId, accessKey, (int)currentAssessmentId);
                }
            }
            else
            {
                // If an assessmentId was sent, use that in the new token after validating user/assessment
                if (assessmentId > 0)
                {
                    _tokenManager.AssessmentForUser(currentUserId, accessKey, assessmentId);
                    currentAssessmentId = assessmentId;
                }

                if (aggregationId > 0)
                {
                    currentAggregationId = aggregationId;
                }
            }

            // If we make it this far, we can issue the new token with what we know to be current and valid
            string token = _tokenManager.GenerateToken(
                currentUserId,
                accessKey,
                _tokenManager.Payload(Constants.Constants.Token_TimezoneOffsetKey),
                expSeconds,
                currentAssessmentId,
                currentAggregationId,
                scope);

            TokenResponse resp = new TokenResponse
            {
                Token = token
            };

            return Ok(resp);
        }


        /// <summary>
        /// Converts a string value to an integer with a default fallback.
        /// </summary>
        /// <param name="value">String value to convert</param>
        /// <param name="defaultInt">Default value if conversion fails</param>
        /// <returns>Converted integer or default value</returns>
        private int StringToInt(string value, int defaultInt = -1)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultInt;
            }

            if (int.TryParse(value, out int result))
            {
                return result;
            }

            return defaultInt;
        }


        /// <summary>
        /// Validates if a JWT token is still valid.
        /// </summary>
        /// <param name="value">JWT token string to validate</param>
        /// <returns>
        /// 200 OK with boolean indicating if token is valid
        /// </returns>
        /// <remarks>
        /// This endpoint is used by the export-to-enterprise feature to determine
        /// whether the current token is still valid before attempting operations
        /// that require authentication.
        /// 
        /// Sample request:
        ///     POST /api/auth/istokenvalid
        ///     "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        /// </remarks>
        [AllowAnonymous]
        [HttpPost]
        [Route("api/auth/istokenvalid")]
        [ProducesResponseType(typeof(bool), 200)]
        public IActionResult IsTokenValid([FromBody] string value)
        {
            _logger.Info("api/auth/istokenvalid");
            return Ok(_tokenManager.IsTokenValid(value));
        }


        /// <summary>
        /// Generates an access key for anonymous user access.
        /// </summary>
        /// <returns>
        /// 200 OK with generated access key
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates a temporary access key that can be used
        /// for anonymous access to certain features. Requires valid JWT token.
        /// 
        /// The access key is typically used for temporary access scenarios
        /// where full user registration is not required.
        /// </remarks>
        [CsetAuthorize]
        [HttpGet]
        [Route("api/auth/accesskey")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetAccessKey()
        {
            var x = _userAuthentication.GenerateAccessKey();
            return Ok(x);
        }


        /// <summary>
        /// Authenticates a user using an access key instead of username/password.
        /// </summary>
        /// <param name="login">Anonymous login containing access key</param>
        /// <returns>
        /// 200 OK with LoginResponse if authentication successful
        /// 400 Bad Request if access key is invalid
        /// </returns>
        /// <remarks>
        /// This endpoint allows authentication using a pre-generated access key,
        /// which is useful for temporary or anonymous access scenarios.
        /// 
        /// Sample request:
        ///     POST /api/auth/login/accesskey
        ///     {
        ///         "AccessKey": "temp-access-key-12345"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/auth/login/accesskey")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(LoginResponse), 400)]
        public IActionResult LoginWithAccessKey([FromBody] AnonymousLogin login)
        {
            LoginResponse resp = _userAuthentication.AuthenticateAccessKey(login);

            if (resp == null)
            {
                return BadRequest(new LoginResponse());
            }

            return Ok(resp);
        }


        /// <summary>
        /// Simple health check endpoint to verify the API is running.
        /// </summary>
        /// <returns>
        /// 200 OK if the API is running
        /// </returns>
        /// <remarks>
        /// This endpoint provides a simple way to check if the CSET API
        /// is running and responding to requests. No authentication required.
        /// 
        /// Useful for:
        /// - Health monitoring
        /// - Load balancer health checks
        /// - Basic connectivity testing
        /// </remarks>
        [HttpGet]
        [Route("api/IsRunning")]
        [ProducesResponseType(200)]
        public IActionResult IsRunning()
        {
            return Ok();
        }
    }
}
