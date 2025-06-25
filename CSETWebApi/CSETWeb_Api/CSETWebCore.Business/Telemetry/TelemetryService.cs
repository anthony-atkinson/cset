using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

namespace CSETWebCore.Business.Telemetry
{
    /// <summary>
    /// Implementation of telemetry service using Application Insights
    /// </summary>
    public class TelemetryService : ITelemetryService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<TelemetryService> _logger;

        public TelemetryService(TelemetryClient telemetryClient, ILogger<TelemetryService> logger)
        {
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void TrackAssessmentCreated(int assessmentId, int userId, string framework)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "AssessmentId", assessmentId.ToString() },
                    { "UserId", userId.ToString() },
                    { "Framework", framework },
                    { "EventType", "AssessmentCreated" }
                };

                var metrics = new Dictionary<string, double>
                {
                    { "AssessmentId", assessmentId }
                };

                _telemetryClient.TrackEvent("AssessmentCreated", properties, metrics);
                _logger.LogInformation("Tracked assessment creation: AssessmentId={AssessmentId}, UserId={UserId}, Framework={Framework}", 
                    assessmentId, userId, framework);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track assessment creation for AssessmentId={AssessmentId}", assessmentId);
            }
        }

        public void TrackAssessmentCompleted(int assessmentId, int userId, TimeSpan completionTime)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "AssessmentId", assessmentId.ToString() },
                    { "UserId", userId.ToString() },
                    { "EventType", "AssessmentCompleted" }
                };

                var metrics = new Dictionary<string, double>
                {
                    { "AssessmentId", assessmentId },
                    { "CompletionTimeMinutes", completionTime.TotalMinutes },
                    { "CompletionTimeSeconds", completionTime.TotalSeconds }
                };

                _telemetryClient.TrackEvent("AssessmentCompleted", properties, metrics);
                _logger.LogInformation("Tracked assessment completion: AssessmentId={AssessmentId}, UserId={UserId}, CompletionTime={CompletionTime}", 
                    assessmentId, userId, completionTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track assessment completion for AssessmentId={AssessmentId}", assessmentId);
            }
        }

        public void TrackReportGenerated(int assessmentId, string reportType, TimeSpan generationTime)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "AssessmentId", assessmentId.ToString() },
                    { "ReportType", reportType },
                    { "EventType", "ReportGenerated" }
                };

                var metrics = new Dictionary<string, double>
                {
                    { "AssessmentId", assessmentId },
                    { "GenerationTimeSeconds", generationTime.TotalSeconds },
                    { "GenerationTimeMilliseconds", generationTime.TotalMilliseconds }
                };

                _telemetryClient.TrackEvent("ReportGenerated", properties, metrics);
                _logger.LogInformation("Tracked report generation: AssessmentId={AssessmentId}, ReportType={ReportType}, GenerationTime={GenerationTime}", 
                    assessmentId, reportType, generationTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track report generation for AssessmentId={AssessmentId}, ReportType={ReportType}", 
                    assessmentId, reportType);
            }
        }

        public void TrackQuestionAnswered(int assessmentId, int questionId, string answer)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "AssessmentId", assessmentId.ToString() },
                    { "QuestionId", questionId.ToString() },
                    { "Answer", answer },
                    { "EventType", "QuestionAnswered" }
                };

                var metrics = new Dictionary<string, double>
                {
                    { "AssessmentId", assessmentId },
                    { "QuestionId", questionId }
                };

                _telemetryClient.TrackEvent("QuestionAnswered", properties, metrics);
                _logger.LogDebug("Tracked question answered: AssessmentId={AssessmentId}, QuestionId={QuestionId}, Answer={Answer}", 
                    assessmentId, questionId, answer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track question answered for AssessmentId={AssessmentId}, QuestionId={QuestionId}", 
                    assessmentId, questionId);
            }
        }

        public void TrackUserLogin(int userId, string loginMethod)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() },
                    { "LoginMethod", loginMethod },
                    { "EventType", "UserLogin" }
                };

                var metrics = new Dictionary<string, double>
                {
                    { "UserId", userId }
                };

                _telemetryClient.TrackEvent("UserLogin", properties, metrics);
                _logger.LogInformation("Tracked user login: UserId={UserId}, LoginMethod={LoginMethod}", userId, loginMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track user login for UserId={UserId}", userId);
            }
        }

        public void TrackDatabaseQuery(string queryName, TimeSpan duration, bool success)
        {
            try
            {
                var dependency = new DependencyTelemetry
                {
                    Type = "SQL",
                    Target = "Database",
                    Name = queryName,
                    Duration = duration,
                    Success = success,
                    Data = queryName
                };

                _telemetryClient.TrackDependency(dependency);
                _logger.LogDebug("Tracked database query: QueryName={QueryName}, Duration={Duration}, Success={Success}", 
                    queryName, duration, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track database query for QueryName={QueryName}", queryName);
            }
        }

        public void TrackApiEndpoint(string endpoint, string method, TimeSpan duration, int statusCode)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "Endpoint", endpoint },
                    { "Method", method },
                    { "StatusCode", statusCode.ToString() },
                    { "EventType", "ApiEndpoint" }
                };

                var metrics = new Dictionary<string, double>
                {
                    { "DurationMilliseconds", duration.TotalMilliseconds },
                    { "StatusCode", statusCode }
                };

                _telemetryClient.TrackEvent("ApiEndpoint", properties, metrics);
                _logger.LogDebug("Tracked API endpoint: Endpoint={Endpoint}, Method={Method}, Duration={Duration}, StatusCode={StatusCode}", 
                    endpoint, method, duration, statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track API endpoint for Endpoint={Endpoint}, Method={Method}", endpoint, method);
            }
        }

        public void TrackCustomEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            try
            {
                _telemetryClient.TrackEvent(eventName, properties, metrics);
                _logger.LogDebug("Tracked custom event: EventName={EventName}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track custom event: EventName={EventName}", eventName);
            }
        }

        public void TrackException(Exception exception, string context = null, int? userId = null)
        {
            try
            {
                var properties = new Dictionary<string, string>();
                
                if (!string.IsNullOrEmpty(context))
                {
                    properties.Add("Context", context);
                }
                
                if (userId.HasValue)
                {
                    properties.Add("UserId", userId.Value.ToString());
                }

                _telemetryClient.TrackException(exception, properties);
                _logger.LogError(exception, "Tracked exception: Context={Context}, UserId={UserId}", context, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track exception: Context={Context}, UserId={UserId}", context, userId);
            }
        }

        public void TrackDependency(string dependencyType, string target, string operation, TimeSpan duration, bool success)
        {
            try
            {
                var dependency = new DependencyTelemetry
                {
                    Type = dependencyType,
                    Target = target,
                    Name = operation,
                    Duration = duration,
                    Success = success,
                    Data = operation
                };

                _telemetryClient.TrackDependency(dependency);
                _logger.LogDebug("Tracked dependency: Type={Type}, Target={Target}, Operation={Operation}, Duration={Duration}, Success={Success}", 
                    dependencyType, target, operation, duration, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track dependency: Type={Type}, Target={Target}, Operation={Operation}", 
                    dependencyType, target, operation);
            }
        }
    }
} 