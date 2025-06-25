using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using CSETWebBlazor.Services;

namespace CSETWebBlazor.Authorization
{
    /// <summary>
    /// Custom authorization attribute for Blazor components
    /// Mirrors the functionality of the API's CsetAuthorize attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CsetAuthorizeAttribute : Attribute
    {
        public string[] Roles { get; set; } = new string[] { "User" };

        public CsetAuthorizeAttribute(params string[] roles)
        {
            if (roles.Length > 0)
            {
                Roles = roles;
            }
        }
    }

    /// <summary>
    /// Authorization handler for CsetAuthorize attribute
    /// </summary>
    public class CsetAuthorizeHandler : IAuthorizeData
    {
        private readonly IAuthenticationService _authenticationService;

        public CsetAuthorizeHandler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public string? Policy { get; set; }
        public string? Roles { get; set; }
        public string? AuthenticationSchemes { get; set; }

        public bool IsAuthorized()
        {
            // Check if user is authenticated
            if (!_authenticationService.IsAuthenticated())
            {
                return false;
            }

            // Check roles if specified
            if (!string.IsNullOrEmpty(Roles))
            {
                var requiredRoles = Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
                return requiredRoles.Any(role => _authenticationService.HasRole(role.Trim()));
            }

            return true;
        }
    }

    /// <summary>
    /// Authorization service for Blazor components
    /// </summary>
    public interface ICsetAuthorizationService
    {
        /// <summary>
        /// Checks if the current user is authorized based on the specified roles
        /// </summary>
        /// <param name="roles">Required roles</param>
        /// <returns>True if authorized, false otherwise</returns>
        bool IsAuthorized(params string[] roles);

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
        /// Checks if the current user is authenticated
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        bool IsAuthenticated();
    }

    /// <summary>
    /// Authorization service implementation for Blazor components
    /// </summary>
    public class CsetAuthorizationService : ICsetAuthorizationService
    {
        private readonly IAuthenticationService _authenticationService;

        public CsetAuthorizationService(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public bool IsAuthorized(params string[] roles)
        {
            if (!_authenticationService.IsAuthenticated())
            {
                return false;
            }

            if (roles.Length == 0)
            {
                return true;
            }

            return roles.Any(role => _authenticationService.HasRole(role));
        }

        public bool HasRole(string role)
        {
            return _authenticationService.HasRole(role);
        }

        public bool IsSuperUser()
        {
            return _authenticationService.IsSuperUser();
        }

        public bool IsAuthenticated()
        {
            return _authenticationService.IsAuthenticated();
        }
    }

    /// <summary>
    /// Authorization state provider for Blazor authentication
    /// </summary>
    public class CsetAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthenticationService _authenticationService;

        public CsetAuthenticationStateProvider(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            _authenticationService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = _authenticationService.CurrentUser;
            
            if (user?.IsAuthenticated == true && !_authenticationService.IsTokenExpired())
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("IsSuperUser", user.IsSuperUser.ToString()),
                    new Claim("CurrentAssessmentId", user.CurrentAssessmentId)
                };

                // Add roles as claims
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, "CSET");
                var principal = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(principal));
            }

            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        private void OnAuthenticationStateChanged(UserSession? user)
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
} 