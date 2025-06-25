using CSETWebBlazor.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Service for analytics operations
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IApiClientService _apiClient;
        private readonly IConfigService _configService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AnalyticsService> _logger;
        private readonly IErrorHandlingService _errorHandling;

        private string _baseUrl = string.Empty;
        private string _analyticsUrl = string.Empty;
        private string _lastError = string.Empty;

        // Events
        public event Action<AnalyticsAggregation>? AnalyticsUpdated;
        public event Action<string>? AnalyticsError;

        public AnalyticsService(
            IApiClientService apiClient,
            IConfigService configService,
            IMemoryCache cache,
            ILogger<AnalyticsService> logger,
            IErrorHandlingService errorHandling)
        {
            _apiClient = apiClient;
            _configService = configService;
            _cache = cache;
            _logger = logger;
            _errorHandling = errorHandling;

            InitializeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Initialize the service by setting up URLs
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                _baseUrl = await _configService.GetConfigAsync().ContinueWith(t => t.Result?.Api?.Host ?? "localhost");
                _analyticsUrl = await GetAnalyticsUrlAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize AnalyticsService");
                await _errorHandling.HandleErrorAsync(ex, "Failed to initialize analytics service");
            }
        }

        #region Analytics Data

        public async Task<AnalyticsAggregation> GetAnalyticsAsync()
        {
            try
            {
                var cacheKey = "analytics_aggregation";
                if (_cache.TryGetValue(cacheKey, out AnalyticsAggregation? cachedAnalytics))
                {
                    return cachedAnalytics ?? new AnalyticsAggregation();
                }

                var response = await _apiClient.GetAsync<AnalyticsAggregation>("analytics/getAggregation");
                if (response != null)
                {
                    _cache.Set(cacheKey, response, TimeSpan.FromMinutes(10));
                    AnalyticsUpdated?.Invoke(response);
                    return response;
                }

                return new AnalyticsAggregation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get analytics");
                _lastError = ex.Message;
                AnalyticsError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve analytics data");
                return new AnalyticsAggregation();
            }
        }

        public async Task<MaturityAnalyticsResult> GetAnalyticResultsAsync(MaturityAnalyticsRequest request)
        {
            try
            {
                var queryParams = new List<string>();
                
                if (request.MaturityModelId.HasValue)
                {
                    queryParams.Add($"modelId={request.MaturityModelId.Value}");
                }
                
                if (request.SectorId.HasValue)
                {
                    queryParams.Add($"sectorId={request.SectorId.Value}");
                }
                
                if (request.StartDate.HasValue)
                {
                    queryParams.Add($"startDate={request.StartDate.Value:yyyy-MM-dd}");
                }
                
                if (request.EndDate.HasValue)
                {
                    queryParams.Add($"endDate={request.EndDate.Value:yyyy-MM-dd}");
                }
                
                if (request.IncludeInactive)
                {
                    queryParams.Add("includeInactive=true");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _apiClient.GetAsync<MaturityAnalyticsResult>($"analytics/maturity/bars{queryString}");
                
                return response ?? new MaturityAnalyticsResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get maturity analytics results");
                _lastError = ex.Message;
                AnalyticsError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve maturity analytics results");
                return new MaturityAnalyticsResult();
            }
        }

        public async Task<SectorAnalyticsResult> GetSectorAnalyticsAsync(SectorAnalyticsRequest request)
        {
            try
            {
                var queryParams = new List<string>();
                
                if (request.SectorId.HasValue)
                {
                    queryParams.Add($"sectorId={request.SectorId.Value}");
                }
                
                if (!string.IsNullOrEmpty(request.Workflow))
                {
                    queryParams.Add($"workflow={request.Workflow}");
                }
                
                if (request.StartDate.HasValue)
                {
                    queryParams.Add($"startDate={request.StartDate.Value:yyyy-MM-dd}");
                }
                
                if (request.EndDate.HasValue)
                {
                    queryParams.Add($"endDate={request.EndDate.Value:yyyy-MM-dd}");
                }
                
                if (request.IncludeInactive)
                {
                    queryParams.Add("includeInactive=true");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _apiClient.GetAsync<SectorAnalyticsResult>($"analytics/sector{queryString}");
                
                return response ?? new SectorAnalyticsResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get sector analytics");
                _lastError = ex.Message;
                AnalyticsError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve sector analytics");
                return new SectorAnalyticsResult();
            }
        }

        #endregion

        #region Analytics Authentication

        public async Task<AnalyticsTokenResponse> GetAnalyticsTokenAsync(AnalyticsTokenRequest request)
        {
            try
            {
                // Set timezone offset if not provided
                if (string.IsNullOrEmpty(request.TzOffset))
                {
                    request.TzOffset = DateTimeOffset.Now.Offset.TotalMinutes.ToString();
                }

                var headers = new Dictionary<string, string>
                {
                    ["noauth"] = "true"
                };

                var response = await _apiClient.PostAsync<AnalyticsTokenResponse>($"{_analyticsUrl}auth/login", request, headers);
                
                if (response != null)
                {
                    return response;
                }

                throw new InvalidOperationException("Failed to get analytics token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get analytics token");
                _lastError = ex.Message;
                AnalyticsError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, "Failed to get analytics token");
                throw;
            }
        }

        public async Task<bool> IsRemoteTokenValidAsync(string? tokenString)
        {
            try
            {
                // The TokenManager in the API will throw a 401 for an empty string
                if (string.IsNullOrEmpty(tokenString))
                {
                    tokenString = "abc";
                }

                var headers = new Dictionary<string, string>
                {
                    ["noauth"] = "true",
                    ["x-cset-noauth"] = "true"
                };

                var response = await _apiClient.PostAsync<bool>($"{_analyticsUrl}auth/istokenvalid", tokenString, headers);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if remote token is valid");
                _lastError = ex.Message;
                AnalyticsError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, "Failed to check if remote token is valid");
                return false;
            }
        }

        public async Task<PostAnalyticsResponse> PostAnalyticsAsync(PostAnalyticsRequest request)
        {
            try
            {
                var headers = new Dictionary<string, string>
                {
                    ["RemoteAuthorization"] = request.RemoteToken,
                    ["x-cset-noauth"] = "true"
                };

                var response = await _apiClient.GetAsync<PostAnalyticsResponse>("assessment/exportandsend", headers);
                
                if (response != null)
                {
                    return response;
                }

                return new PostAnalyticsResponse
                {
                    Success = false,
                    Message = "Failed to post analytics data"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to post analytics");
                _lastError = ex.Message;
                AnalyticsError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, "Failed to post analytics data");
                
                return new PostAnalyticsResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        #endregion

        #region Configuration

        public async Task<bool> IsCisaAssessorModeAsync()
        {
            try
            {
                var config = await _configService.GetConfigAsync();
                return config?.InstallationMode == "IOD";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if CISA assessor mode");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check CISA assessor mode");
                return false;
            }
        }

        public async Task<string> GetAnalyticsUrlAsync()
        {
            try
            {
                var config = await _configService.GetConfigAsync();
                return config?.AnalyticsUrl ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get analytics URL");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get analytics URL");
                return string.Empty;
            }
        }

        public async Task<string> GetBaseUrlAsync()
        {
            return _baseUrl;
        }

        #endregion

        #region Caching

        public async Task<AnalyticsAggregation?> GetCachedAnalyticsAsync()
        {
            try
            {
                var cacheKey = "analytics_aggregation";
                if (_cache.TryGetValue(cacheKey, out AnalyticsAggregation? cachedAnalytics))
                {
                    return cachedAnalytics;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cached analytics");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get cached analytics");
                return null;
            }
        }

        public async Task CacheAnalyticsAsync(AnalyticsAggregation analytics)
        {
            try
            {
                var cacheKey = "analytics_aggregation";
                _cache.Set(cacheKey, analytics, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache analytics");
                await _errorHandling.HandleErrorAsync(ex, "Failed to cache analytics");
            }
        }

        public async Task ClearAnalyticsCacheAsync()
        {
            try
            {
                _cache.Remove("analytics_aggregation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear analytics cache");
                await _errorHandling.HandleErrorAsync(ex, "Failed to clear analytics cache");
            }
        }

        #endregion

        #region Error Handling

        public async Task<bool> IsAnalyticsAvailableAsync()
        {
            try
            {
                // Try to get analytics to check if the service is available
                var analytics = await GetAnalyticsAsync();
                return analytics != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analytics service is not available");
                return false;
            }
        }

        public async Task<string> GetLastErrorAsync()
        {
            return _lastError;
        }

        public async Task ClearLastErrorAsync()
        {
            _lastError = string.Empty;
        }

        #endregion
    }
} 