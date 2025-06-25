using CSETWebBlazor.Models;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Interface for configuration management services
    /// </summary>
    public interface IConfigService
    {
        // Configuration Properties
        string ApiUrl { get; }
        string ServerUrl { get; }
        string AppUrl { get; }
        string DocUrl { get; }
        string RefDocUrl { get; }
        string LibraryUrl { get; }
        string OnlineUrl { get; }
        string AnalyticsUrl { get; }
        string CsetGithubApiUrl { get; }
        string HelpContactEmail { get; }
        string HelpContactPhone { get; }
        string DhsEmail { get; }
        string InstallationMode { get; }
        string GalleryLayout { get; }
        string MobileEnvironment { get; }
        bool UserIsCisaAssessor { get; }
        bool IsRunningInElectron { get; }
        bool IsRunningAnonymous { get; }
        bool IsAPI_together_With_Web { get; }

        // Configuration Management
        Task<AppConfig> LoadConfigAsync();
        Task<ConfigResponse> UpdateConfigAsync(ConfigUpdateRequest request);
        Task<AppConfig> GetConfigAsync();
        Task<bool> SaveConfigAsync(AppConfig config);
        Task<ConfigValidationResult> ValidateConfigAsync(AppConfig config);

        // System Status
        Task<SystemStatus> GetSystemStatusAsync();
        Task<bool> CheckLocalDocStatusAsync();
        Task<bool> CheckOnlineDocStatusAsync();
        Task<bool> CheckOnlineStatusFromConfigAsync();

        // Version Information
        Task<VersionInfo> GetCsetVersionAsync();
        Task<string> GetBuildTimeAsync();

        // Feature Configuration
        Task<FeatureConfig> GetFeatureConfigAsync();
        Task<bool> ShowImportButtonAsync();
        Task<bool> ShowExportAllButtonAsync();
        Task<bool> ShowQuestionAndRequirementIDsAsync();
        Task<bool> ShowAssessmentUpgradeAsync();
        Task<bool> ShowBuildTimeAsync();

        // CISA Assessor Workflow
        Task<bool> GetCisaAssessorWorkflowAsync();
        Task<bool> SetCisaAssessorWorkflowAsync(bool enabled);
        Task<bool> EnableCisaAssessorWorkflowAsync();

        // Module Behavior
        Task<ModuleBehavior> GetModuleBehaviorAsync(int id);
        Task<bool> SwitchConfigsForModeAsync(string installationMode);

        // Mobile Features
        Task<bool> IsMobileAsync();
        Task<bool> ShowMobileFeaturesAsync();

        // Configuration Chain
        Task<List<string>> GetCurrentConfigChainAsync();
        Task<bool> LoadConfigChainAsync(List<string> configChain);

        // Label Values
        Task<Dictionary<string, string>> GetSalLabelsAsync();
        Task<bool> PopulateLabelValuesAsync();

        // Behaviors
        Task<Dictionary<string, object>> GetBehaviorsAsync();
        Task<object?> GetBehaviorAsync(string key);
        Task<bool> SetBehaviorAsync(string key, object value);

        // Caching
        Task<AppConfig?> GetCachedConfigAsync();
        Task CacheConfigAsync(AppConfig config);
        Task ClearConfigCacheAsync();

        // Error Handling
        Task<string> GetLastErrorAsync();
        Task ClearLastErrorAsync();

        // Events
        event Action<AppConfig> ConfigLoaded;
        event Action<AppConfig> ConfigUpdated;
        event Action<SystemStatus> SystemStatusChanged;
        event Action<string> ConfigError;
    }
} 