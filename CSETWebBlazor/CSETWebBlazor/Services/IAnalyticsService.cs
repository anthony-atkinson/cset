using CSETWebBlazor.Models;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Interface for analytics services
    /// </summary>
    public interface IAnalyticsService
    {
        // Analytics Data
        Task<AnalyticsAggregation> GetAnalyticsAsync();
        Task<MaturityAnalyticsResult> GetAnalyticResultsAsync(MaturityAnalyticsRequest request);
        Task<SectorAnalyticsResult> GetSectorAnalyticsAsync(SectorAnalyticsRequest request);

        // Analytics Authentication
        Task<AnalyticsTokenResponse> GetAnalyticsTokenAsync(AnalyticsTokenRequest request);
        Task<bool> IsRemoteTokenValidAsync(string? tokenString);
        Task<PostAnalyticsResponse> PostAnalyticsAsync(PostAnalyticsRequest request);

        // Configuration
        Task<bool> IsCisaAssessorModeAsync();
        Task<string> GetAnalyticsUrlAsync();
        Task<string> GetBaseUrlAsync();

        // Caching
        Task<AnalyticsAggregation?> GetCachedAnalyticsAsync();
        Task CacheAnalyticsAsync(AnalyticsAggregation analytics);
        Task ClearAnalyticsCacheAsync();

        // Error Handling
        Task<bool> IsAnalyticsAvailableAsync();
        Task<string> GetLastErrorAsync();
        Task ClearLastErrorAsync();

        // Events
        event Action<AnalyticsAggregation> AnalyticsUpdated;
        event Action<string> AnalyticsError;
    }
} 