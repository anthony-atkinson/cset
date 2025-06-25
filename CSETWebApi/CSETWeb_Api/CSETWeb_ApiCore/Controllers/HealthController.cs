using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CSETWebCore.Business.Telemetry;
using CSETWebCore.DataLayer.Model;
using Microsoft.EntityFrameworkCore;

namespace CSETWeb_ApiCore.Controllers
{
    /// <summary>
    /// Controller for health monitoring and performance metrics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly CSETContext _context;

        public HealthController(
            ILogger<HealthController> logger,
            ITelemetryService telemetryService,
            CSETContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = GetType().Assembly.GetName().Version?.ToString(),
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                };

                stopwatch.Stop();
                _telemetryService.TrackApiEndpoint("/api/health", "GET", stopwatch.Elapsed, 200);

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _telemetryService.TrackException(ex, "Health Check");
                _logger.LogError(ex, "Health check failed");
                
                return StatusCode(500, new { Status = "Unhealthy", Error = ex.Message });
            }
        }

        /// <summary>
        /// Detailed health check including database connectivity
        /// </summary>
        /// <returns>Detailed health status</returns>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedHealth()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = GetType().Assembly.GetName().Version?.ToString(),
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    Database = await CheckDatabaseHealth(),
                    System = GetSystemInfo()
                };

                stopwatch.Stop();
                _telemetryService.TrackApiEndpoint("/api/health/detailed", "GET", stopwatch.Elapsed, 200);

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _telemetryService.TrackException(ex, "Detailed Health Check");
                _logger.LogError(ex, "Detailed health check failed");
                
                return StatusCode(500, new { Status = "Unhealthy", Error = ex.Message });
            }
        }

        /// <summary>
        /// Performance metrics endpoint
        /// </summary>
        /// <returns>Performance metrics</returns>
        [HttpGet("metrics")]
        public IActionResult GetMetrics()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var metrics = new
                {
                    Timestamp = DateTime.UtcNow,
                    Memory = GetMemoryMetrics(),
                    Process = GetProcessMetrics(),
                    System = GetSystemMetrics()
                };

                stopwatch.Stop();
                _telemetryService.TrackApiEndpoint("/api/health/metrics", "GET", stopwatch.Elapsed, 200);

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _telemetryService.TrackException(ex, "Metrics Collection");
                _logger.LogError(ex, "Metrics collection failed");
                
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        private async Task<object> CheckDatabaseHealth()
        {
            try
            {
                var dbStopwatch = Stopwatch.StartNew();
                
                // Test database connectivity
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                
                // Test a simple query
                var assessmentCount = await _context.ASSESSMENTS.CountAsync();
                
                dbStopwatch.Stop();
                
                _telemetryService.TrackDatabaseQuery("HealthCheck_AssessmentCount", dbStopwatch.Elapsed, true);

                return new
                {
                    Status = "Connected",
                    ResponseTime = dbStopwatch.Elapsed.TotalMilliseconds,
                    AssessmentCount = assessmentCount
                };
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex, "Database Health Check");
                return new
                {
                    Status = "Disconnected",
                    Error = ex.Message
                };
            }
        }

        private object GetSystemInfo()
        {
            return new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet,
                Is64BitProcess = Environment.Is64BitProcess,
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem
            };
        }

        private object GetMemoryMetrics()
        {
            var process = Process.GetCurrentProcess();
            return new
            {
                WorkingSet = process.WorkingSet64,
                WorkingSetMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                PrivateMemorySize = process.PrivateMemorySize64,
                PrivateMemorySizeMB = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2),
                VirtualMemorySize = process.VirtualMemorySize64,
                VirtualMemorySizeMB = Math.Round(process.VirtualMemorySize64 / 1024.0 / 1024.0, 2)
            };
        }

        private object GetProcessMetrics()
        {
            var process = Process.GetCurrentProcess();
            return new
            {
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                StartTime = process.StartTime,
                TotalProcessorTime = process.TotalProcessorTime.TotalSeconds,
                UserProcessorTime = process.UserProcessorTime.TotalSeconds,
                Threads = process.Threads.Count,
                Handles = process.HandleCount
            };
        }

        private object GetSystemMetrics()
        {
            return new
            {
                Uptime = Environment.TickCount / 1000.0 / 60.0, // Minutes
                GCTotalMemory = GC.GetTotalMemory(false),
                GCTotalMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2),
                GCCollectionCount = new
                {
                    Gen0 = GC.CollectionCount(0),
                    Gen1 = GC.CollectionCount(1),
                    Gen2 = GC.CollectionCount(2)
                }
            };
        }
    }
} 