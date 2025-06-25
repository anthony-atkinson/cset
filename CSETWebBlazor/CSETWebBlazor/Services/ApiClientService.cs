using System.Text;
using System.Text.Json;
using CSETWebBlazor.Models;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// API client service implementation for communicating with the CSET API
    /// Handles HTTP requests, authentication headers, and response processing
    /// </summary>
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<ApiClientService> _logger;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        private Dictionary<string, string> _customHeaders = new();

        public ApiClientService(
            HttpClient httpClient,
            IAuthenticationService authenticationService,
            ILogger<ApiClientService> logger,
            IErrorHandlingService errorHandlingService,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _authenticationService = authenticationService;
            _logger = logger;
            _errorHandlingService = errorHandlingService;
            _configuration = configuration;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Configure base address from configuration
            var apiBaseUrl = _configuration["CSET:ApiBaseUrl"] ?? "https://localhost:5001/";
            _httpClient.BaseAddress = new Uri(apiBaseUrl);
            _httpClient.Timeout = TimeSpan.FromMinutes(5); // 5 minute timeout
        }

        public async Task<T?> GetAsync<T>(string endpoint, bool requiresAuth = true)
        {
            try
            {
                var response = await GetRawAsync(endpoint, requiresAuth);
                return await ProcessResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, $"GET request failed for endpoint: {endpoint}");
                throw;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object? data = null, bool requiresAuth = true)
        {
            try
            {
                var response = await PostRawAsync(endpoint, data, requiresAuth);
                return await ProcessResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, $"POST request failed for endpoint: {endpoint}");
                throw;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object? data = null, bool requiresAuth = true)
        {
            try
            {
                var request = CreateRequestMessage(HttpMethod.Put, endpoint, data, requiresAuth);
                var response = await _httpClient.SendAsync(request);
                return await ProcessResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, $"PUT request failed for endpoint: {endpoint}");
                throw;
            }
        }

        public async Task<T?> DeleteAsync<T>(string endpoint, bool requiresAuth = true)
        {
            try
            {
                var request = CreateRequestMessage(HttpMethod.Delete, endpoint, null, requiresAuth);
                var response = await _httpClient.SendAsync(request);
                return await ProcessResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, $"DELETE request failed for endpoint: {endpoint}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> GetRawAsync(string endpoint, bool requiresAuth = true)
        {
            var request = CreateRequestMessage(HttpMethod.Get, endpoint, null, requiresAuth);
            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PostRawAsync(string endpoint, object? data = null, bool requiresAuth = true)
        {
            var request = CreateRequestMessage(HttpMethod.Post, endpoint, data, requiresAuth);
            return await _httpClient.SendAsync(request);
        }

        public async Task<T?> UploadFileAsync<T>(string endpoint, Stream fileStream, string fileName, string contentType, bool requiresAuth = true)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                content.Add(fileContent, "file", fileName);

                var request = CreateRequestMessage(HttpMethod.Post, endpoint, content, requiresAuth);
                var response = await _httpClient.SendAsync(request);
                return await ProcessResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, $"File upload failed for endpoint: {endpoint}");
                throw;
            }
        }

        public async Task<byte[]> DownloadFileAsync(string endpoint, bool requiresAuth = true)
        {
            try
            {
                var response = await GetRawAsync(endpoint, requiresAuth);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("File download failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"File download failed: {response.StatusCode}");
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, $"File download failed for endpoint: {endpoint}");
                throw;
            }
        }

        public void SetCustomHeaders(Dictionary<string, string> headers)
        {
            _customHeaders = new Dictionary<string, string>(headers);
        }

        public void ClearCustomHeaders()
        {
            _customHeaders.Clear();
        }

        public string GetBaseUrl()
        {
            return _httpClient.BaseAddress?.ToString() ?? string.Empty;
        }

        public async Task<bool> IsApiAccessibleAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "API accessibility check failed");
                return false;
            }
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string endpoint, object? data, bool requiresAuth)
        {
            var request = new HttpRequestMessage(method, endpoint);

            // Add authentication header if required
            if (requiresAuth)
            {
                var token = _authenticationService.GetCurrentToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

            // Add custom headers
            foreach (var header in _customHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            // Add content for POST/PUT requests
            if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put))
            {
                if (data is HttpContent content)
                {
                    request.Content = content;
                }
                else
                {
                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }

            return request;
        }

        private async Task<T?> ProcessResponseAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API request failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);

                // Handle specific error cases
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.Unauthorized:
                        // Token might be expired, try to refresh
                        await HandleUnauthorizedAsync();
                        break;
                    case System.Net.HttpStatusCode.Forbidden:
                        throw new UnauthorizedAccessException("Access denied");
                    case System.Net.HttpStatusCode.NotFound:
                        throw new InvalidOperationException("Resource not found");
                    case System.Net.HttpStatusCode.BadRequest:
                        throw new ArgumentException($"Bad request: {errorContent}");
                    default:
                        throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
                }
            }

            // Handle empty responses
            if (response.Content.Headers.ContentLength == 0)
            {
                return default(T);
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // Handle primitive types
            if (typeof(T) == typeof(string))
            {
                return (T)(object)content;
            }
            if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(content);
            }
            if (typeof(T) == typeof(bool))
            {
                return (T)(object)bool.Parse(content);
            }

            // Handle complex types
            try
            {
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (JsonException ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Failed to deserialize API response");
                throw new InvalidOperationException($"Failed to deserialize response: {ex.Message}");
            }
        }

        private async Task HandleUnauthorizedAsync()
        {
            try
            {
                // Try to refresh the token
                var refreshRequest = new TokenRefreshRequest { IsRefresh = true };
                await _authenticationService.RefreshTokenAsync(refreshRequest);
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex, "Token refresh failed during unauthorized handling");
                // If refresh fails, logout the user
                await _authenticationService.LogoutAsync();
            }
        }
    }
} 