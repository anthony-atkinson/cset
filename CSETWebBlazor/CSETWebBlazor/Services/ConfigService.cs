using CSETWebBlazor.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Service for configuration management operations
    /// </summary>
    public class ConfigService : IConfigService
    {
        private readonly IApiClientService _apiClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ConfigService> _logger;
        private readonly IErrorHandlingService _errorHandling;

        private AppConfig? _config;
        private bool _initialized = false;
        private string _lastError = string.Empty;

        // Configuration Properties
        public string ApiUrl { get; private set; } = string.Empty;
        public string ServerUrl { get; private set; } = string.Empty;
        public string AppUrl { get; private set; } = string.Empty;
        public string DocUrl { get; private set; } = string.Empty;
        public string RefDocUrl { get; private set; } = string.Empty;
        public string LibraryUrl { get; private set; } = string.Empty;
        public string OnlineUrl { get; private set; } = string.Empty;
        public string AnalyticsUrl { get; private set; } = string.Empty;
        public string CsetGithubApiUrl { get; private set; } = string.Empty;
        public string HelpContactEmail { get; private set; } = string.Empty;
        public string HelpContactPhone { get; private set; } = string.Empty;
        public string DhsEmail { get; private set; } = string.Empty;
        public string InstallationMode { get; private set; } = string.Empty;
        public string GalleryLayout { get; private set; } = string.Empty;
        public string MobileEnvironment { get; private set; } = string.Empty;
        public bool UserIsCisaAssessor { get; private set; } = false;
        public bool IsRunningInElectron { get; private set; } = false;
        public bool IsRunningAnonymous { get; private set; } = false;
        public bool IsAPI_together_With_Web { get; private set; } = false;

        // Events
        public event Action<AppConfig>? ConfigLoaded;
        public event Action<AppConfig>? ConfigUpdated;
        public event Action<SystemStatus>? SystemStatusChanged;
        public event Action<string>? ConfigError;

        public ConfigService(
            IApiClientService apiClient,
            IMemoryCache cache,
            ILogger<ConfigService> logger,
            IErrorHandlingService errorHandling)
        {
            _apiClient = apiClient;
            _cache = cache;
            _logger = logger;
            _errorHandling = errorHandling;

            InitializeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Initialize the service by loading configuration
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                await LoadConfigAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize ConfigService");
                await _errorHandling.HandleErrorAsync(ex, "Failed to initialize config service");
            }
        }

        #region Configuration Management

        public async Task<AppConfig> LoadConfigAsync()
        {
            try
            {
                if (_initialized && _config != null)
                {
                    return _config;
                }

                var cacheKey = "app_config";
                if (_cache.TryGetValue(cacheKey, out AppConfig? cachedConfig))
                {
                    _config = cachedConfig;
                    SetConfigProperties();
                    _initialized = true;
                    ConfigLoaded?.Invoke(_config);
                    return _config;
                }

                // Load base configuration
                var config = await LoadBaseConfigAsync();
                if (config != null)
                {
                    // Load configuration chain
                    await LoadConfigChainAsync(config);

                    // Set configuration properties
                    _config = config;
                    SetConfigProperties();
                    _initialized = true;

                    // Cache the configuration
                    _cache.Set(cacheKey, config, TimeSpan.FromMinutes(30));
                    
                    ConfigLoaded?.Invoke(config);
                    return config;
                }

                throw new InvalidOperationException("Failed to load configuration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load configuration");
                _lastError = ex.Message;
                ConfigError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, "Failed to load configuration");
                throw;
            }
        }

        private async Task<AppConfig?> LoadBaseConfigAsync()
        {
            try
            {
                // Load the base configuration from assets/settings/config.json
                var configJson = await LoadConfigFileAsync("assets/settings/config.json");
                if (!string.IsNullOrEmpty(configJson))
                {
                    var config = JsonSerializer.Deserialize<AppConfig>(configJson);
                    return config;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load base configuration");
                await _errorHandling.HandleErrorAsync(ex, "Failed to load base configuration");
                return null;
            }
        }

        private async Task LoadConfigChainAsync(AppConfig config)
        {
            try
            {
                if (config.CurrentConfigChain == null || config.CurrentConfigChain.Count == 0)
                {
                    return;
                }

                foreach (var configProfile in config.CurrentConfigChain)
                {
                    var configPath = $"assets/settings/config.{configProfile}.json";
                    var subConfigJson = await LoadConfigFileAsync(configPath);
                    
                    if (!string.IsNullOrEmpty(subConfigJson))
                    {
                        var subConfig = JsonSerializer.Deserialize<AppConfig>(subConfigJson);
                        if (subConfig != null)
                        {
                            // Merge the sub-configuration into the main configuration
                            MergeConfig(config, subConfig);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load configuration chain");
                await _errorHandling.HandleErrorAsync(ex, "Failed to load configuration chain");
            }
        }

        private async Task<string> LoadConfigFileAsync(string path)
        {
            try
            {
                // This would typically load from the wwwroot folder in a Blazor application
                // For now, we'll use a placeholder implementation
                return await Task.FromResult("{}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load config file {Path}", path);
                return string.Empty;
            }
        }

        private void MergeConfig(AppConfig target, AppConfig source)
        {
            // Merge configuration properties
            if (!string.IsNullOrEmpty(source.InstallationMode))
                target.InstallationMode = source.InstallationMode;
            
            if (!string.IsNullOrEmpty(source.PublicDomainName))
                target.PublicDomainName = source.PublicDomainName;
            
            if (source.IsRunningAnonymous)
                target.IsRunningAnonymous = source.IsRunningAnonymous;
            
            if (!string.IsNullOrEmpty(source.HelpContactEmail))
                target.HelpContactEmail = source.HelpContactEmail;
            
            if (!string.IsNullOrEmpty(source.HelpContactPhone))
                target.HelpContactPhone = source.HelpContactPhone;
            
            if (!string.IsNullOrEmpty(source.CsetGithubApiUrl))
                target.CsetGithubApiUrl = source.CsetGithubApiUrl;
            
            if (!string.IsNullOrEmpty(source.GalleryLayout))
                target.GalleryLayout = source.GalleryLayout;
            
            if (!string.IsNullOrEmpty(source.MobileEnvironment))
                target.MobileEnvironment = source.MobileEnvironment;
            
            if (source.UserIsCisaAssessor)
                target.UserIsCisaAssessor = source.UserIsCisaAssessor;

            // Merge API configuration
            if (source.Api != null)
            {
                if (target.Api == null)
                    target.Api = new ApiConfig();
                
                if (!string.IsNullOrEmpty(source.Api.Protocol))
                    target.Api.Protocol = source.Api.Protocol;
                
                if (!string.IsNullOrEmpty(source.Api.Host))
                    target.Api.Host = source.Api.Host;
                
                if (!string.IsNullOrEmpty(source.Api.Port))
                    target.Api.Port = source.Api.Port;
                
                if (!string.IsNullOrEmpty(source.Api.ApiIdentifier))
                    target.Api.ApiIdentifier = source.Api.ApiIdentifier;
                
                if (!string.IsNullOrEmpty(source.Api.DocumentsIdentifier))
                    target.Api.DocumentsIdentifier = source.Api.DocumentsIdentifier;
                
                if (!string.IsNullOrEmpty(source.Api.LibraryIdentifier))
                    target.Api.LibraryIdentifier = source.Api.LibraryIdentifier;
            }

            // Merge App configuration
            if (source.App != null)
            {
                if (target.App == null)
                    target.App = new AppConfig();
                
                if (!string.IsNullOrEmpty(source.App.Protocol))
                    target.App.Protocol = source.App.Protocol;
                
                if (!string.IsNullOrEmpty(source.App.Host))
                    target.App.Host = source.App.Host;
                
                if (!string.IsNullOrEmpty(source.App.Port))
                    target.App.Port = source.App.Port;
            }

            // Merge behaviors
            if (source.Behaviors != null)
            {
                if (target.Behaviors == null)
                    target.Behaviors = new Dictionary<string, object>();
                
                foreach (var behavior in source.Behaviors)
                {
                    target.Behaviors[behavior.Key] = behavior.Value;
                }
            }

            // Merge SAL labels
            if (source.SalLabels != null)
            {
                if (target.SalLabels == null)
                    target.SalLabels = new Dictionary<string, string>();
                
                foreach (var label in source.SalLabels)
                {
                    target.SalLabels[label.Key] = label.Value;
                }
            }
        }

        private void SetConfigProperties()
        {
            if (_config == null)
                return;

            IsRunningInElectron = false; // This would be set based on the environment
            IsRunningAnonymous = _config.IsRunningAnonymous;
            InstallationMode = _config.InstallationMode;
            GalleryLayout = _config.GalleryLayout;
            MobileEnvironment = _config.MobileEnvironment;
            UserIsCisaAssessor = _config.UserIsCisaAssessor;

            // Build URLs
            BuildUrls();
        }

        private void BuildUrls()
        {
            if (_config?.Api == null)
                return;

            var apiPort = !string.IsNullOrEmpty(_config.Api.Port) ? $":{_config.Api.Port}" : "";
            var appPort = !string.IsNullOrEmpty(_config.App?.Port) ? $":{_config.App.Port}" : "";
            var apiProtocol = $"{_config.Api.Protocol}://";
            var appProtocol = $"{_config.App?.Protocol}://";

            // Check for localStorage overrides (this would be handled differently in Blazor)
            var localStorageApiUrl = ""; // This would be retrieved from browser storage
            var localStorageLibraryUrl = ""; // This would be retrieved from browser storage

            if (!string.IsNullOrEmpty(localStorageApiUrl))
            {
                ApiUrl = $"{localStorageApiUrl}/{_config.Api.ApiIdentifier}/";
                ServerUrl = $"{localStorageApiUrl}/";
                DocUrl = $"{localStorageApiUrl}/{_config.Api.DocumentsIdentifier}/";
            }
            else
            {
                var apiUrl = BuildUrl(_config.Api);
                ApiUrl = $"{apiUrl}/{_config.Api.ApiIdentifier}/";
                ServerUrl = $"{apiUrl}/";
                DocUrl = $"{apiUrl}/{_config.Api.DocumentsIdentifier}/";
            }

            if (_config.App != null)
            {
                AppUrl = $"{appProtocol}{_config.App.Host}{appPort}";
            }

            HelpContactEmail = _config.HelpContactEmail;
            HelpContactPhone = _config.HelpContactPhone;
            CsetGithubApiUrl = _config.CsetGithubApiUrl;

            // Set other URLs
            RefDocUrl = $"{ServerUrl}api/library/doc";
            LibraryUrl = $"{ServerUrl}api/library";
            OnlineUrl = ServerUrl;
            AnalyticsUrl = _config.AnalyticsUrl ?? string.Empty;
        }

        private string BuildUrl(ApiConfig config)
        {
            var port = !string.IsNullOrEmpty(config.Port) ? $":{config.Port}" : "";
            return $"{config.Protocol}://{config.Host}{port}";
        }

        public async Task<ConfigResponse> UpdateConfigAsync(ConfigUpdateRequest request)
        {
            try
            {
                var response = await _apiClient.PutAsync<ConfigResponse>("config", request);
                
                if (response?.Success == true)
                {
                    // Reload configuration
                    await LoadConfigAsync();
                    ConfigUpdated?.Invoke(_config!);
                }
                
                return response ?? new ConfigResponse { Success = false, Message = "Failed to update configuration" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update configuration");
                _lastError = ex.Message;
                ConfigError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, "Failed to update configuration");
                
                return new ConfigResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<AppConfig> GetConfigAsync()
        {
            if (_config != null)
            {
                return _config;
            }

            return await LoadConfigAsync();
        }

        public async Task<bool> SaveConfigAsync(AppConfig config)
        {
            try
            {
                var response = await _apiClient.PostAsync<bool>("config", config);
                
                if (response)
                {
                    _config = config;
                    SetConfigProperties();
                    
                    // Update cache
                    _cache.Set("app_config", config, TimeSpan.FromMinutes(30));
                    
                    ConfigUpdated?.Invoke(config);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save configuration");
                await _errorHandling.HandleErrorAsync(ex, "Failed to save configuration");
                return false;
            }
        }

        public async Task<ConfigValidationResult> ValidateConfigAsync(AppConfig config)
        {
            try
            {
                var response = await _apiClient.PostAsync<ConfigValidationResult>("config/validate", config);
                return response ?? new ConfigValidationResult { IsValid = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate configuration");
                await _errorHandling.HandleErrorAsync(ex, "Failed to validate configuration");
                
                return new ConfigValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion

        #region System Status

        public async Task<SystemStatus> GetSystemStatusAsync()
        {
            try
            {
                var status = new SystemStatus
                {
                    ApiUrl = ApiUrl,
                    ServerUrl = ServerUrl,
                    DocUrl = DocUrl,
                    LibraryUrl = LibraryUrl,
                    RefDocUrl = RefDocUrl,
                    CheckedAt = DateTime.UtcNow
                };

                // Check API status
                try
                {
                    var apiResponse = await _apiClient.GetAsync<bool>("health");
                    status.ApiOnline = apiResponse;
                }
                catch
                {
                    status.ApiOnline = false;
                    status.Errors.Add("API is not responding");
                }

                // Check database status
                try
                {
                    var dbResponse = await _apiClient.GetAsync<bool>("health/database");
                    status.DatabaseOnline = dbResponse;
                }
                catch
                {
                    status.DatabaseOnline = false;
                    status.Errors.Add("Database is not accessible");
                }

                // Check local documents
                try
                {
                    var localDocsResponse = await CheckLocalDocStatusAsync();
                    status.LocalDocumentsAvailable = localDocsResponse;
                }
                catch
                {
                    status.LocalDocumentsAvailable = false;
                    status.Warnings.Add("Local documents not available");
                }

                // Check online documents
                try
                {
                    var onlineDocsResponse = await CheckOnlineDocStatusAsync();
                    status.OnlineDocumentsAvailable = onlineDocsResponse;
                }
                catch
                {
                    status.OnlineDocumentsAvailable = false;
                    status.Warnings.Add("Online documents not available");
                }

                SystemStatusChanged?.Invoke(status);
                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system status");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get system status");
                
                return new SystemStatus
                {
                    CheckedAt = DateTime.UtcNow,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<bool> CheckLocalDocStatusAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("HasLocalDocuments");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check local document status");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check local document status");
                return false;
            }
        }

        public async Task<bool> CheckOnlineDocStatusAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("HasOnlineDocuments");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check online document status");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check online document status");
                return false;
            }
        }

        public async Task<bool> CheckOnlineStatusFromConfigAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("config/online");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check online status from config");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check online status from config");
                return false;
            }
        }

        #endregion

        #region Version Information

        public async Task<VersionInfo> GetCsetVersionAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<VersionInfo>("version");
                return response ?? new VersionInfo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get CSET version");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get CSET version");
                return new VersionInfo();
            }
        }

        public async Task<string> GetBuildTimeAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<string>("version/buildtime");
                return response ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get build time");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get build time");
                return string.Empty;
            }
        }

        #endregion

        #region Feature Configuration

        public async Task<FeatureConfig> GetFeatureConfigAsync()
        {
            try
            {
                var config = await GetConfigAsync();
                return new FeatureConfig
                {
                    ShowImportButton = true, // Default values
                    ShowExportAllButton = true,
                    ShowQuestionAndRequirementIDs = false,
                    ShowAssessmentUpgrade = true,
                    ShowBuildTime = false,
                    EnableCisaAssessorWorkflow = config.UserIsCisaAssessor,
                    EnableMobileFeatures = !string.IsNullOrEmpty(config.MobileEnvironment),
                    EnableOfflineMode = false,
                    EnableCollaboration = true,
                    EnableAnalytics = true,
                    EnableReports = true,
                    EnableDiagrams = true,
                    EnableDocuments = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get feature configuration");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get feature configuration");
                return new FeatureConfig();
            }
        }

        public async Task<bool> ShowImportButtonAsync()
        {
            try
            {
                var features = await GetFeatureConfigAsync();
                return features.ShowImportButton;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if import button should be shown");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check import button visibility");
                return true; // Default to showing
            }
        }

        public async Task<bool> ShowExportAllButtonAsync()
        {
            try
            {
                var features = await GetFeatureConfigAsync();
                return features.ShowExportAllButton;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if export all button should be shown");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check export all button visibility");
                return true; // Default to showing
            }
        }

        public async Task<bool> ShowQuestionAndRequirementIDsAsync()
        {
            try
            {
                var features = await GetFeatureConfigAsync();
                return features.ShowQuestionAndRequirementIDs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if question and requirement IDs should be shown");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check question and requirement ID visibility");
                return false; // Default to hiding
            }
        }

        public async Task<bool> ShowAssessmentUpgradeAsync()
        {
            try
            {
                var features = await GetFeatureConfigAsync();
                return features.ShowAssessmentUpgrade;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if assessment upgrade should be shown");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check assessment upgrade visibility");
                return true; // Default to showing
            }
        }

        public async Task<bool> ShowBuildTimeAsync()
        {
            try
            {
                var features = await GetFeatureConfigAsync();
                return features.ShowBuildTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if build time should be shown");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check build time visibility");
                return false; // Default to hiding
            }
        }

        #endregion

        #region CISA Assessor Workflow

        public async Task<bool> GetCisaAssessorWorkflowAsync()
        {
            return UserIsCisaAssessor;
        }

        public async Task<bool> SetCisaAssessorWorkflowAsync(bool enabled)
        {
            try
            {
                UserIsCisaAssessor = enabled;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set CISA assessor workflow");
                await _errorHandling.HandleErrorAsync(ex, "Failed to set CISA assessor workflow");
                return false;
            }
        }

        public async Task<bool> EnableCisaAssessorWorkflowAsync()
        {
            try
            {
                var iodConfig = await LoadConfigFileAsync("assets/settings/config.IOD.json");
                if (!string.IsNullOrEmpty(iodConfig))
                {
                    var config = JsonSerializer.Deserialize<AppConfig>(iodConfig);
                    if (config != null)
                    {
                        MergeConfig(_config!, config);
                        SetConfigProperties();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enable CISA assessor workflow");
                await _errorHandling.HandleErrorAsync(ex, "Failed to enable CISA assessor workflow");
                return false;
            }
        }

        #endregion

        #region Module Behavior

        public async Task<ModuleBehavior> GetModuleBehaviorAsync(int id)
        {
            try
            {
                var config = await GetConfigAsync();
                if (config.Behaviors != null && config.Behaviors.ContainsKey(id.ToString()))
                {
                    var behaviorData = config.Behaviors[id.ToString()];
                    // Convert behavior data to ModuleBehavior object
                    return new ModuleBehavior
                    {
                        Id = id,
                        Name = id.ToString(),
                        Description = "Module behavior",
                        IsActive = true,
                        Settings = new Dictionary<string, object>()
                    };
                }

                return new ModuleBehavior
                {
                    Id = id,
                    Name = id.ToString(),
                    Description = "Default module behavior",
                    IsActive = true,
                    Settings = new Dictionary<string, object>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get module behavior for ID {Id}", id);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to get module behavior for ID {id}");
                
                return new ModuleBehavior
                {
                    Id = id,
                    Name = id.ToString(),
                    Description = "Error loading module behavior",
                    IsActive = false,
                    Settings = new Dictionary<string, object>()
                };
            }
        }

        public async Task<bool> SwitchConfigsForModeAsync(string installationMode)
        {
            try
            {
                InstallationMode = installationMode;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to switch configs for mode {Mode}", installationMode);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to switch configs for mode {installationMode}");
                return false;
            }
        }

        #endregion

        #region Mobile Features

        public async Task<bool> IsMobileAsync()
        {
            return !string.IsNullOrEmpty(MobileEnvironment);
        }

        public async Task<bool> ShowMobileFeaturesAsync()
        {
            try
            {
                var features = await GetFeatureConfigAsync();
                return features.EnableMobileFeatures;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if mobile features should be shown");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check mobile features visibility");
                return false;
            }
        }

        #endregion

        #region Configuration Chain

        public async Task<List<string>> GetCurrentConfigChainAsync()
        {
            try
            {
                var config = await GetConfigAsync();
                return config.CurrentConfigChain ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current config chain");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get current config chain");
                return new List<string>();
            }
        }

        public async Task<bool> LoadConfigChainAsync(List<string> configChain)
        {
            try
            {
                if (_config != null)
                {
                    _config.CurrentConfigChain = configChain;
                    await LoadConfigChainAsync(_config);
                    SetConfigProperties();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load config chain");
                await _errorHandling.HandleErrorAsync(ex, "Failed to load config chain");
                return false;
            }
        }

        #endregion

        #region Label Values

        public async Task<Dictionary<string, string>> GetSalLabelsAsync()
        {
            try
            {
                var config = await GetConfigAsync();
                return config.SalLabels ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get SAL labels");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get SAL labels");
                return new Dictionary<string, string>();
            }
        }

        public async Task<bool> PopulateLabelValuesAsync()
        {
            try
            {
                // This would populate label values from the configuration
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to populate label values");
                await _errorHandling.HandleErrorAsync(ex, "Failed to populate label values");
                return false;
            }
        }

        #endregion

        #region Behaviors

        public async Task<Dictionary<string, object>> GetBehaviorsAsync()
        {
            try
            {
                var config = await GetConfigAsync();
                return config.Behaviors ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get behaviors");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get behaviors");
                return new Dictionary<string, object>();
            }
        }

        public async Task<object?> GetBehaviorAsync(string key)
        {
            try
            {
                var behaviors = await GetBehaviorsAsync();
                return behaviors.ContainsKey(key) ? behaviors[key] : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get behavior for key {Key}", key);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to get behavior for key {key}");
                return null;
            }
        }

        public async Task<bool> SetBehaviorAsync(string key, object value)
        {
            try
            {
                if (_config != null)
                {
                    if (_config.Behaviors == null)
                        _config.Behaviors = new Dictionary<string, object>();
                    
                    _config.Behaviors[key] = value;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set behavior for key {Key}", key);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to set behavior for key {key}");
                return false;
            }
        }

        #endregion

        #region Caching

        public async Task<AppConfig?> GetCachedConfigAsync()
        {
            try
            {
                var cacheKey = "app_config";
                if (_cache.TryGetValue(cacheKey, out AppConfig? cachedConfig))
                {
                    return cachedConfig;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cached config");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get cached config");
                return null;
            }
        }

        public async Task CacheConfigAsync(AppConfig config)
        {
            try
            {
                var cacheKey = "app_config";
                _cache.Set(cacheKey, config, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache config");
                await _errorHandling.HandleErrorAsync(ex, "Failed to cache config");
            }
        }

        public async Task ClearConfigCacheAsync()
        {
            try
            {
                _cache.Remove("app_config");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear config cache");
                await _errorHandling.HandleErrorAsync(ex, "Failed to clear config cache");
            }
        }

        #endregion

        #region Error Handling

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