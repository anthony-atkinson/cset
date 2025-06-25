using CSETWebBlazor.Models;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Authentication service interface for Blazor Server
    /// Handles user authentication, token management, and session state
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Current user session information
        /// </summary>
        UserSession? CurrentUser { get; }

        /// <summary>
        /// Event fired when authentication state changes
        /// </summary>
        event Action<UserSession?> AuthenticationStateChanged;

        /// <summary>
        /// Authenticates user with email and password
        /// </summary>
        /// <param name="request">Login request containing credentials</param>
        /// <returns>Login response with user information and token</returns>
        Task<LoginResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Authenticates user with access key (anonymous login)
        /// </summary>
        /// <param name="request">Anonymous login request</param>
        /// <returns>Login response with user information and token</returns>
        Task<LoginResponse> LoginWithAccessKeyAsync(AnonymousLoginRequest request);

        /// <summary>
        /// Logs out the current user
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Refreshes the current JWT token
        /// </summary>
        /// <param name="request">Token refresh request</param>
        /// <returns>New token response</returns>
        Task<TokenResponse> RefreshTokenAsync(TokenRefreshRequest request);

        /// <summary>
        /// Validates if the current token is still valid
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>True if token is valid, false otherwise</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Gets an access key for anonymous access
        /// </summary>
        /// <returns>Generated access key</returns>
        Task<string> GetAccessKeyAsync();

        /// <summary>
        /// Changes the current user's password
        /// </summary>
        /// <param name="request">Password change request</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> ChangePasswordAsync(PasswordChangeRequest request);

        /// <summary>
        /// Updates the current user's profile
        /// </summary>
        /// <param name="request">Profile update request</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateProfileAsync(UserProfileUpdate request);

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        bool IsAuthenticated();

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="role">Role to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        bool HasRole(string role);

        /// <summary>
        /// Checks if the current user is a super user
        /// </summary>
        /// <returns>True if super user, false otherwise</returns>
        bool IsSuperUser();

        /// <summary>
        /// Gets the current user's assessment ID
        /// </summary>
        /// <returns>Current assessment ID or empty string</returns>
        string GetCurrentAssessmentId();

        /// <summary>
        /// Sets the current assessment ID for the user session
        /// </summary>
        /// <param name="assessmentId">Assessment ID to set</param>
        void SetCurrentAssessmentId(string assessmentId);

        /// <summary>
        /// Initializes the authentication service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Gets the current JWT token
        /// </summary>
        /// <returns>Current JWT token or empty string</returns>
        string GetCurrentToken();

        /// <summary>
        /// Checks if the current token is expired
        /// </summary>
        /// <returns>True if expired, false otherwise</returns>
        bool IsTokenExpired();

        /// <summary>
        /// Gets the timezone offset for the current user
        /// </summary>
        /// <returns>Timezone offset string</returns>
        string GetTimezoneOffset();

        /// <summary>
        /// Checks if this is a local installation
        /// </summary>
        /// <returns>True if local installation, false otherwise</returns>
        Task<bool> IsLocalInstallationAsync();
    }
} 