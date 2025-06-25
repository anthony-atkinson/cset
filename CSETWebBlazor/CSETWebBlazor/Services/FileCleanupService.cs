using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSETWebBlazor.Services
{
    public class FileCleanupService : BackgroundService
    {
        private readonly ILogger<FileCleanupService> _logger;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _cleanupInterval;

        public FileCleanupService(
            ILogger<FileCleanupService> logger,
            IFileService fileService,
            IConfiguration configuration)
        {
            _logger = logger;
            _fileService = fileService;
            _configuration = configuration;
            
            // Run cleanup every 24 hours by default
            _cleanupInterval = TimeSpan.FromHours(
                _configuration.GetValue<int>("CSET:FileCleanupIntervalHours", 24));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("File cleanup service started. Cleanup interval: {Interval}", _cleanupInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformCleanup();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during file cleanup");
                }

                // Wait for the next cleanup interval
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }

        private async Task PerformCleanup()
        {
            _logger.LogInformation("Starting scheduled file cleanup");
            
            try
            {
                await _fileService.CleanupOldFilesAsync();
                _logger.LogInformation("File cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file cleanup");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("File cleanup service stopped");
            await base.StopAsync(cancellationToken);
        }
    }
} 