using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using CSETWebCore.Business.Telemetry;

namespace CSETWebCore.Business.Telemetry
{
    /// <summary>
    /// Entity Framework interceptor for tracking database query performance
    /// </summary>
    public class DatabasePerformanceInterceptor : IObserver<DiagnosticListener>
    {
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<DatabasePerformanceInterceptor> _logger;

        public DatabasePerformanceInterceptor(ITelemetryService telemetryService, ILogger<DatabasePerformanceInterceptor> logger)
        {
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnCompleted()
        {
            // No action needed
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error, "Error in database performance interceptor");
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == "Microsoft.EntityFrameworkCore.Database.Command")
            {
                value.Subscribe(new DatabaseCommandObserver(_telemetryService, _logger));
            }
        }
    }

    /// <summary>
    /// Observer for database command events
    /// </summary>
    public class DatabaseCommandObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<DatabaseCommandObserver> _logger;

        public DatabaseCommandObserver(ITelemetryService telemetryService, ILogger<DatabaseCommandObserver> logger)
        {
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnCompleted()
        {
            // No action needed
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error, "Error in database command observer");
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            try
            {
                switch (value.Key)
                {
                    case "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting":
                        HandleCommandExecuting(value.Value);
                        break;
                    case "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted":
                        HandleCommandExecuted(value.Value);
                        break;
                    case "Microsoft.EntityFrameworkCore.Database.Command.CommandError":
                        HandleCommandError(value.Value);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling database command event: {EventName}", value.Key);
            }
        }

        private void HandleCommandExecuting(object value)
        {
            if (value is CommandEventData commandData)
            {
                var queryName = GetQueryName(commandData.Command.CommandText);
                var stopwatch = Stopwatch.StartNew();
                
                // Store stopwatch in command data for later use
                commandData.Command.Connection.DataSource = stopwatch.GetHashCode().ToString();
            }
        }

        private void HandleCommandExecuted(object value)
        {
            if (value is CommandEventData commandData)
            {
                var queryName = GetQueryName(commandData.Command.CommandText);
                var duration = TimeSpan.FromMilliseconds(commandData.Duration);
                var success = true;

                _telemetryService.TrackDatabaseQuery(queryName, duration, success);

                // Log slow queries
                if (duration.TotalMilliseconds > 100) // Log queries taking more than 100ms
                {
                    _logger.LogWarning("Slow database query detected: {QueryName} took {Duration}ms",
                        queryName, duration.TotalMilliseconds);
                }
            }
        }

        private void HandleCommandError(object value)
        {
            if (value is CommandErrorEventData errorData)
            {
                var queryName = GetQueryName(errorData.Command.CommandText);
                var duration = TimeSpan.FromMilliseconds(errorData.Duration);
                var success = false;

                _telemetryService.TrackDatabaseQuery(queryName, duration, success);
                _telemetryService.TrackException(errorData.Exception, $"Database Query: {queryName}");

                _logger.LogError(errorData.Exception, "Database query failed: {QueryName} took {Duration}ms",
                    queryName, duration.TotalMilliseconds);
            }
        }

        private string GetQueryName(string commandText)
        {
            if (string.IsNullOrEmpty(commandText))
                return "Unknown";

            // Extract the first word (usually the SQL command type)
            var firstWord = commandText.Trim().Split(' ').FirstOrDefault();
            if (string.IsNullOrEmpty(firstWord))
                return "Unknown";

            // Limit the query name length
            var queryName = firstWord.ToUpperInvariant();
            if (commandText.Length > 50)
            {
                queryName += "_" + commandText.Substring(0, 50).Replace(" ", "_").Replace("\n", "_").Replace("\r", "_");
            }

            return queryName;
        }
    }
} 