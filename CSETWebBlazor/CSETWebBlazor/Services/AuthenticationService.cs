using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CSETWebBlazor.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Authentication service implementation for Blazor Server
    /// Handles user authentication, token management, and session state
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IValidationService _validationService;

        private UserSession? _currentUser;
        private string _currentToken = string.Empty;
        private Timer? _tokenRefreshTimer;

        public event Action<UserSession?>? AuthenticationStateChanged;

        public UserSession? CurrentUser => _currentUser;

        public AuthenticationService(
            HttpClient httpClient,
            ILogger<AuthenticationService> logger,
            IConfiguration configuration,
            ProtectedLocalStorage protectedLocalStorage,
            IErrorHandlingService errorHandlingService,
            IValidationService validationService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _protectedLocalStorage = protectedLocalStorage;
            _errorHandlingService = errorHandlingService;
            _validationService = validationService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Validate request
                var validation = await _validationService.ValidateObjectAsync(request);
                if (!validation.IsValid)
                {
                    throw new ArgumentException($"Invalid login request: {string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))}");
                }

                // Set timezone offset if not provided
                if (string.IsNullOrEmpty(request.TzOffset))
                {
                    request.TzOffset = GetTimezoneOffset();
                }

                // Prepare request for API
                var apiRequest = new
                {
                    Email = request.Email,
                    Password = request.Password,
                    TzOffset = request.TzOffset,
                    Scope = request.Scope
                };

                var json = JsonSerializer.Serialize(apiRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Make API call
                var response = await _httpClient.PostAsync("api/auth/login", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Login failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Login failed: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse == null)
                {
                    throw new InvalidOperationException("Failed to deserialize login response");
                }

                // Store user session
                await StoreUserSessionAsync(loginResponse);

                // Start token refresh timer
                StartTokenRefreshTimer();

                return loginResponse;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Login operation failed");
                throw;
            }
        }

        public async Task<LoginResponse> LoginWithAccessKeyAsync(AnonymousLoginRequest request)
        {
            try
            {
                // Validate request
                var validation = await _validationService.ValidateObjectAsync(request);
                if (!validation.IsValid)
                {
                    throw new ArgumentException($"Invalid anonymous login request: {string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))}");
                }

                // Set timezone offset if not provided
                if (string.IsNullOrEmpty(request.TzOffset))
                {
                    request.TzOffset = GetTimezoneOffset();
                }

                // Prepare request for API
                var apiRequest = new
                {
                    AccessKey = request.AccessKey,
                    TzOffset = request.TzOffset,
                    Scope = request.Scope
                };

                var json = JsonSerializer.Serialize(apiRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Make API call
                var response = await _httpClient.PostAsync("api/auth/login/accesskey", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Anonymous login failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Anonymous login failed: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse == null)
                {
                    throw new InvalidOperationException("Failed to deserialize login response");
                }

                // Store user session
                await StoreUserSessionAsync(loginResponse);

                // Start token refresh timer
                StartTokenRefreshTimer();

                return loginResponse;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Anonymous login operation failed");
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                // Stop token refresh timer
                StopTokenRefreshTimer();

                // Clear current session
                _currentUser = null;
                _currentToken = string.Empty;

                // Clear stored session data
                await _protectedLocalStorage.DeleteAsync("userSession");
                await _protectedLocalStorage.DeleteAsync("authToken");

                // Notify state change
                AuthenticationStateChanged?.Invoke(null);

                _logger.LogInformation("User logged out successfully");
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Logout operation failed");
                throw;
            }
        }

        public async Task<TokenResponse> RefreshTokenAsync(TokenRefreshRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentToken))
                {
                    throw new InvalidOperationException("No current token to refresh");
                }

                // Prepare headers
                var headers = new Dictionary<string, string>();
                if (request.AssessmentId.HasValue)
                {
                    headers["assessmentid"] = request.AssessmentId.Value.ToString();
                }
                if (request.AggregationId.HasValue)
                {
                    headers["aggregationid"] = request.AggregationId.Value.ToString();
                }
                if (request.ExpSeconds.HasValue)
                {
                    headers["expSeconds"] = request.ExpSeconds.Value.ToString();
                }
                if (request.IsRefresh)
                {
                    headers["refresh"] = "true";
                }

                // Add authorization header
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/auth/token");
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _currentToken);

                // Add custom headers
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }

                // Make API call
                var response = await _httpClient.SendAsync(requestMessage);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Token refresh failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Token refresh failed: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse == null)
                {
                    throw new InvalidOperationException("Failed to deserialize token response");
                }

                // Update current token
                _currentToken = tokenResponse.Token;
                await _protectedLocalStorage.SetAsync("authToken", _currentToken);

                // Update user session expiration
                if (_currentUser != null)
                {
                    _currentUser.TokenExpiration = tokenResponse.Expiration;
                    await _protectedLocalStorage.SetAsync("userSession", _currentUser);
                }

                return tokenResponse;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Token refresh operation failed");
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                var content = new StringContent($"\"{token}\"", Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/auth/istokenvalid", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(responseContent);
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Token validation failed");
                return false;
            }
        }

        public async Task<string> GetAccessKeyAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentToken))
                {
                    throw new InvalidOperationException("No current token available");
                }

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/auth/accesskey");
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _currentToken);

                var response = await _httpClient.SendAsync(requestMessage);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Get access key failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Get access key failed: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<string>(responseContent) ?? string.Empty;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Get access key operation failed");
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(PasswordChangeRequest request)
        {
            try
            {
                // Validate request
                var validation = await _validationService.ValidateObjectAsync(request);
                if (!validation.IsValid)
                {
                    return false;
                }

                // Validate password strength
                var passwordValidation = await _validationService.ValidatePasswordAsync(request.NewPassword);
                if (!passwordValidation.IsValid)
                {
                    return false;
                }

                // TODO: Implement password change API call
                // This would typically call a user management endpoint
                _logger.LogInformation("Password change requested for user {UserId}", _currentUser?.UserId);
                
                return true;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Password change operation failed");
                return false;
            }
        }

        public async Task<bool> UpdateProfileAsync(UserProfileUpdate request)
        {
            try
            {
                // Validate request
                var validation = await _validationService.ValidateObjectAsync(request);
                if (!validation.IsValid)
                {
                    return false;
                }

                // TODO: Implement profile update API call
                // This would typically call a user management endpoint
                _logger.LogInformation("Profile update requested for user {UserId}", _currentUser?.UserId);
                
                return true;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Profile update operation failed");
                return false;
            }
        }

        public bool IsAuthenticated()
        {
            return _currentUser?.IsAuthenticated == true && !IsTokenExpired();
        }

        public bool HasRole(string role)
        {
            return _currentUser?.Roles.Contains(role) == true;
        }

        public bool IsSuperUser()
        {
            return _currentUser?.IsSuperUser == true;
        }

        public string GetCurrentAssessmentId()
        {
            return _currentUser?.CurrentAssessmentId ?? string.Empty;
        }

        public void SetCurrentAssessmentId(string assessmentId)
        {
            if (_currentUser != null)
            {
                _currentUser.CurrentAssessmentId = assessmentId;
                _ = Task.Run(async () => await _protectedLocalStorage.SetAsync("userSession", _currentUser));
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Try to restore session from protected storage
                var sessionResult = await _protectedLocalStorage.GetAsync<UserSession>("userSession");
                var tokenResult = await _protectedLocalStorage.GetAsync<string>("authToken");

                if (sessionResult.Success && tokenResult.Success)
                {
                    _currentUser = sessionResult.Value;
                    _currentToken = tokenResult.Value;

                    // Validate token
                    if (await ValidateTokenAsync(_currentToken))
                    {
                        _currentUser.IsAuthenticated = true;
                        StartTokenRefreshTimer();
                        AuthenticationStateChanged?.Invoke(_currentUser);
                    }
                    else
                    {
                        // Token is invalid, clear session
                        await LogoutAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Authentication service initialization failed");
                await LogoutAsync();
            }
        }

        public string GetCurrentToken()
        {
            return _currentToken;
        }

        public bool IsTokenExpired()
        {
            return _currentUser?.TokenExpiration <= DateTime.UtcNow;
        }

        public string GetTimezoneOffset()
        {
            return TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes.ToString();
        }

        public async Task<bool> IsLocalInstallationAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/auth/islocal");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<bool>(content);
                }
                return false;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Failed to check local installation status");
                return false;
            }
        }

        private async Task StoreUserSessionAsync(LoginResponse loginResponse)
        {
            _currentToken = loginResponse.Token;
            _currentUser = new UserSession
            {
                UserId = loginResponse.UserId,
                Email = loginResponse.Email,
                FirstName = loginResponse.UserFirstName,
                LastName = loginResponse.UserLastName,
                IsSuperUser = loginResponse.IsSuperUser,
                Lang = loginResponse.Lang,
                Roles = loginResponse.Roles ?? new List<string>(),
                LoginTime = DateTime.UtcNow,
                TokenExpiration = DateTime.UtcNow.AddHours(1), // Default 1 hour, should be parsed from token
                IsAuthenticated = true,
                CurrentAssessmentId = string.Empty
            };

            // Store in protected storage
            await _protectedLocalStorage.SetAsync("userSession", _currentUser);
            await _protectedLocalStorage.SetAsync("authToken", _currentToken);

            // Notify state change
            AuthenticationStateChanged?.Invoke(_currentUser);
        }

        private void StartTokenRefreshTimer()
        {
            StopTokenRefreshTimer();

            // Refresh token 5 minutes before expiration
            var refreshInterval = TimeSpan.FromMinutes(55); // Assuming 1-hour token lifetime
            _tokenRefreshTimer = new Timer(async _ => await RefreshTokenBeforeExpiration(), null, refreshInterval, refreshInterval);
        }

        private void StopTokenRefreshTimer()
        {
            _tokenRefreshTimer?.Dispose();
            _tokenRefreshTimer = null;
        }

        private async Task RefreshTokenBeforeExpiration()
        {
            try
            {
                if (IsTokenExpired())
                {
                    await LogoutAsync();
                    return;
                }

                var request = new TokenRefreshRequest
                {
                    IsRefresh = true
                };

                await RefreshTokenAsync(request);
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Automatic token refresh failed");
                await LogoutAsync();
            }
        }
    }
} 