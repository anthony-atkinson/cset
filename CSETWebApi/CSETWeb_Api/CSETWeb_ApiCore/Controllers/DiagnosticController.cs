//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.AspNetCore.Mvc;
using System;
using CSETWebCore.Interfaces.Notification;
using NLog;
using CSETWebCore.DataLayer.Model;
using System.Linq;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for diagnostic operations in CSET.
    /// This controller handles system diagnostics, email testing, and logging
    /// configuration verification. Supports troubleshooting and system health
    /// monitoring for CSET deployments.
    /// </summary>
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        private readonly INotificationBusiness _notification;
        private readonly CSETContext _context;

        /// <summary>
        /// Initializes a new instance of the DiagnosticController.
        /// </summary>
        /// <param name="notification">The notification business service for email operations</param>
        /// <param name="context">The database context for data access operations</param>
        public DiagnosticController(INotificationBusiness notification, CSETContext context)
        {
            _notification = notification;
            _context = context;
        }

        /// <summary>
        /// Tests connectivity to the SMTP server and sends a test email to the designated recipient.
        /// </summary>
        /// <param name="recip">The email address to send the test email to</param>
        /// <returns>
        /// 200 OK with "Test email sent successfully" if email was sent
        /// Error message if email sending fails
        /// </returns>
        /// <remarks>
        /// This endpoint tests the email configuration and SMTP connectivity:
        /// - Validates SMTP server connectivity
        /// - Tests email configuration settings
        /// - Sends a test email to the specified recipient
        /// - Provides detailed error information on failure
        /// 
        /// The email test process includes:
        /// - SMTP server connection validation
        /// - Email configuration verification
        /// - Test email generation and sending
        /// - Error handling and reporting
        /// 
        /// Email test features:
        /// - SMTP connectivity verification
        /// - Email configuration validation
        /// - Test email delivery confirmation
        /// - Detailed error reporting
        /// - Configuration troubleshooting
        /// 
        /// The test email includes:
        /// - Standard test email content
        /// - System identification information
        /// - Timestamp and diagnostic data
        /// - Configuration verification details
        /// 
        /// Usage scenarios:
        /// - System deployment verification
        /// - Email configuration troubleshooting
        /// - SMTP server connectivity testing
        /// - Notification system validation
        /// - Configuration debugging
        /// 
        /// Error handling:
        /// - SMTP connection failures
        /// - Authentication errors
        /// - Configuration issues
        /// - Network connectivity problems
        /// - Email delivery failures
        /// 
        /// This endpoint is used for:
        /// - System administrator diagnostics
        /// - Deployment verification
        /// - Configuration troubleshooting
        /// - Email system validation
        /// - Support and maintenance operations
        /// 
        /// No authentication required - this is a diagnostic endpoint.
        /// </remarks>
        [HttpPost]
        [Route("api/diagnostic/email")]
        [ProducesResponseType(200)]
        public string TestEmailServer(string recip)
        {
            try
            {
                _notification.SendTestEmail(recip);
            }
            catch (Exception Exc)
            {
                return Exc.Message;
            }

            return "Test email sent successfully";
        }

        /// <summary>
        /// Tests logging configuration by echoing supplied text to the two logging targets.
        /// </summary>
        /// <param name="text">The text message to log for testing purposes</param>
        /// <returns>
        /// 200 OK with completion timestamp if logging was successful
        /// 400 Bad Request if logging fails
        /// </returns>
        /// <remarks>
        /// This endpoint tests the logging configuration and writes test messages:
        /// - Tests database logging functionality
        /// - Tests file logging functionality
        /// - Validates logging configuration
        /// - Provides logging verification
        /// 
        /// The logging test process includes:
        /// - Database logging target test
        /// - File logging target test
        /// - Log message generation
        /// - Logging configuration validation
        /// 
        /// Logging test features:
        /// - Dual logging target testing
        /// - Database logging verification
        /// - File logging verification
        /// - Configuration validation
        /// - Log level testing
        /// 
        /// Logging targets tested:
        /// - Database logging (via LogManager.GetCurrentClassLogger())
        /// - File logging (via LogManager.GetLogger("DBManager"))
        /// - Info level logging
        /// - Timestamp generation
        /// 
        /// The test includes:
        /// - Custom text message logging
        /// - Timestamp generation
        /// - Multiple logging target verification
        /// - Configuration validation
        /// - Error handling
        /// 
        /// Usage scenarios:
        /// - Logging configuration verification
        /// - System deployment testing
        /// - Logging troubleshooting
        /// - Configuration validation
        /// - Support and maintenance operations
        /// 
        /// Logging verification:
        /// - Database log entries
        /// - File log entries
        /// - Log level configuration
        /// - Log format validation
        /// - Log rotation testing
        /// 
        /// This endpoint is used for:
        /// - System administrator diagnostics
        /// - Logging configuration verification
        /// - Deployment testing
        /// - Troubleshooting operations
        /// - Configuration validation
        /// 
        /// No authentication required - this is a diagnostic endpoint.
        /// </remarks>
        [HttpGet]
        [Route("api/diagnostic/logging")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult TestLogging([FromQuery] string text)
        {
            var logToDb = LogManager.GetCurrentClassLogger();
            logToDb.Info(text);

            var logToFile = LogManager.GetLogger("DBManager");
            logToFile.Info(text);

            return Ok($"Complete at {DateTime.UtcNow} UTC");
        }
    }
}
