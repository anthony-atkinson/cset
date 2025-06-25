using System.ComponentModel.DataAnnotations;

namespace CSETWebBlazor.Models
{
    /// <summary>
    /// Login request model for Blazor authentication
    /// </summary>
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string TzOffset { get; set; } = string.Empty;

        /// <summary>
        /// The application scope (CSET, IOD, etc.)
        /// </summary>
        public string Scope { get; set; } = "CSET";
    }

    /// <summary>
    /// Anonymous login request model
    /// </summary>
    public class AnonymousLoginRequest
    {
        [Required]
        public string AccessKey { get; set; } = string.Empty;

        public string TzOffset { get; set; } = string.Empty;

        public string Scope { get; set; } = "CSET";
    }

    /// <summary>
    /// Login response model for Blazor authentication
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserFirstName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public bool ResetRequired { get; set; }
        public bool IsPasswordExpired { get; set; }
        public bool IsSuperUser { get; set; }
        public string Lang { get; set; } = "en";
        public string ExportExtension { get; set; } = string.Empty;
        public string ImportExtensions { get; set; } = string.Empty;
        public string LinkerTime { get; set; } = string.Empty;
        public bool IsFirstLogin { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// User session information for Blazor
    /// </summary>
    public class UserSession
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsSuperUser { get; set; }
        public string Lang { get; set; } = "en";
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime LoginTime { get; set; }
        public DateTime TokenExpiration { get; set; }
        public bool IsAuthenticated { get; set; }
        public string CurrentAssessmentId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Token refresh request model
    /// </summary>
    public class TokenRefreshRequest
    {
        public int? AssessmentId { get; set; }
        public int? AggregationId { get; set; }
        public int? ExpSeconds { get; set; }
        public bool IsRefresh { get; set; }
    }

    /// <summary>
    /// Token response model
    /// </summary>
    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }

    /// <summary>
    /// Password change request model
    /// </summary>
    public class PasswordChangeRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// User profile update model
    /// </summary>
    public class UserProfileUpdate
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Lang { get; set; } = "en";
    }
} 