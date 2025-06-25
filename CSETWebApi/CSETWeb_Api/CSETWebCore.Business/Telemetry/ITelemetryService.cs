using System;
using System.Collections.Generic;

namespace CSETWebCore.Business.Telemetry
{
    /// <summary>
    /// Interface for custom telemetry and business metrics tracking
    /// </summary>
    public interface ITelemetryService
    {
        /// <summary>
        /// Track assessment creation
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="framework">Framework used</param>
        void TrackAssessmentCreated(int assessmentId, int userId, string framework);

        /// <summary>
        /// Track assessment completion
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="completionTime">Time taken to complete</param>
        void TrackAssessmentCompleted(int assessmentId, int userId, TimeSpan completionTime);

        /// <summary>
        /// Track report generation
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="reportType">Type of report generated</param>
        /// <param name="generationTime">Time taken to generate</param>
        void TrackReportGenerated(int assessmentId, string reportType, TimeSpan generationTime);

        /// <summary>
        /// Track question answered
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="questionId">Question ID</param>
        /// <param name="answer">Answer provided</param>
        void TrackQuestionAnswered(int assessmentId, int questionId, string answer);

        /// <summary>
        /// Track user login
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="loginMethod">Method used for login</param>
        void TrackUserLogin(int userId, string loginMethod);

        /// <summary>
        /// Track database query performance
        /// </summary>
        /// <param name="queryName">Name of the query</param>
        /// <param name="duration">Query duration</param>
        /// <param name="success">Whether query was successful</param>
        void TrackDatabaseQuery(string queryName, TimeSpan duration, bool success);

        /// <summary>
        /// Track API endpoint performance
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="method">HTTP method</param>
        /// <param name="duration">Request duration</param>
        /// <param name="statusCode">HTTP status code</param>
        void TrackApiEndpoint(string endpoint, string method, TimeSpan duration, int statusCode);

        /// <summary>
        /// Track custom business event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="properties">Custom properties</param>
        /// <param name="metrics">Custom metrics</param>
        void TrackCustomEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null);

        /// <summary>
        /// Track exception with context
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="context">Additional context</param>
        /// <param name="userId">User ID if available</param>
        void TrackException(Exception exception, string context = null, int? userId = null);

        /// <summary>
        /// Track dependency call
        /// </summary>
        /// <param name="dependencyType">Type of dependency</param>
        /// <param name="target">Target of the dependency</param>
        /// <param name="operation">Operation performed</param>
        /// <param name="duration">Duration of the call</param>
        /// <param name="success">Whether the call was successful</param>
        void TrackDependency(string dependencyType, string target, string operation, TimeSpan duration, bool success);
    }
} 