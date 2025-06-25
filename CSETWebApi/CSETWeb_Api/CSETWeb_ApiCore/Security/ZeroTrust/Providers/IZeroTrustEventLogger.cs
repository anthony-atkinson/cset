using System;
using System.Threading.Tasks;
using CSETWebCore.Model.Security;

namespace CSETWebCore.ApiCore.Security.ZeroTrust.Providers
{
    /// <summary>
    /// Interface for zero trust event logger
    /// Handles logging and monitoring of zero trust events
    /// </summary>
    public interface IZeroTrustEventLogger
    {
        /// <summary>
        /// Logs a zero trust event
        /// </summary>
        /// <param name="event">Event to log</param>
        /// <returns>Logging success status</returns>
        Task<bool> LogEventAsync(ZeroTrustEvent @event);

        /// <summary>
        /// Gets events for a specific time range
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="eventType">Optional event type filter</param>
        /// <returns>List of events</returns>
        Task<List<ZeroTrustEvent>> GetEventsAsync(DateTime startTime, DateTime endTime, string eventType = null);

        /// <summary>
        /// Gets events for a specific user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="limit">Maximum number of events to return</param>
        /// <returns>List of user events</returns>
        Task<List<ZeroTrustEvent>> GetUserEventsAsync(string userId, int limit = 100);

        /// <summary>
        /// Gets security incidents
        /// </summary>
        /// <param name="severity">Minimum severity level</param>
        /// <param name="limit">Maximum number of incidents to return</param>
        /// <returns>List of security incidents</returns>
        Task<List<SecurityIncident>> GetSecurityIncidentsAsync(RiskLevel severity = RiskLevel.Medium, int limit = 50);

        /// <summary>
        /// Archives old events
        /// </summary>
        /// <param name="olderThan">Archive events older than this date</param>
        /// <returns>Number of archived events</returns>
        Task<int> ArchiveEventsAsync(DateTime olderThan);
    }
} 