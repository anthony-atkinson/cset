using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for email configuration management in CSET.
    /// This controller handles DHS email address retrieval and email configuration
    /// management. Supports email system configuration and contact information
    /// for CSET support and feedback operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the EmailController.
        /// </summary>
        /// <param name="configuration">The configuration service for email settings access</param>
        public EmailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves the DHS email address from configuration settings.
        /// </summary>
        /// <returns>
        /// 200 OK with DHS email address if configured
        /// 200 OK with "test" if DHS email is not configured
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves the DHS email address from configuration:
        /// - Reads Email:DHSEmail from configuration settings
        /// - Returns the configured DHS contact email
        /// - Provides fallback value if not configured
        /// - Supports CSET support and feedback operations
        /// 
        /// The DHS email configuration includes:
        /// - Primary DHS contact email address
        /// - CSET support contact information
        /// - Feedback submission address
        /// - System administration contact
        /// 
        /// Email configuration features:
        /// - Configuration-based email management
        /// - DHS contact information retrieval
        /// - Support email address access
        /// - Feedback submission support
        /// - System administration contact
        /// 
        /// The response includes:
        /// - Configured DHS email address
        /// - Fallback test value if not configured
        /// - Contact information for support
        /// - Feedback submission address
        /// 
        /// Usage scenarios:
        /// - CSET support contact information
        /// - Feedback submission workflows
        /// - System administration contact
        /// - Email configuration verification
        /// - Contact information display
        /// 
        /// The DHS email supports:
        /// - CSET support inquiries
        /// - Feedback submission
        /// - System administration
        /// - Technical support
        /// - User assistance
        /// 
        /// Configuration settings:
        /// - Email:DHSEmail in appsettings.json
        /// - Default value handling
        /// - Configuration validation
        /// - Fallback behavior
        /// 
        /// No authentication required - this is a public configuration endpoint.
        /// </remarks>
        [HttpGet("dhsemail")]
        [ProducesResponseType(200)]
        public IActionResult GetDHSEmail()
        {
            var dhsEmail = _configuration.GetValue<string>("Email:DHSEmail");
            if (string.IsNullOrEmpty(dhsEmail))
            {
                return Ok("test"); // 204 No Content
            }
            return Ok(dhsEmail);
        }
    }
}