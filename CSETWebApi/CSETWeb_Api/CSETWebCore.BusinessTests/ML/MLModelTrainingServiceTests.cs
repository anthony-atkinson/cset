//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Threading.Tasks;
using CSETWebCore.Business.ML;
using CSETWebCore.Interfaces.ML;
using CSETWebCore.Model.ML;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CSETWebCore.BusinessTests.ML
{
    /// <summary>
    /// Tests for ML model training service
    /// </summary>
    public class MLModelTrainingServiceTests
    {
        private readonly Mock<IMLDataPipeline> _mockDataPipeline;
        private readonly Mock<IMLPredictionService> _mockPredictionService;
        private readonly Mock<ILogger<MLModelTrainingService>> _mockLogger;
        private readonly MLModelTrainingService _trainingService;

        public MLModelTrainingServiceTests()
        {
            _mockDataPipeline = new Mock<IMLDataPipeline>();
            _mockPredictionService = new Mock<IMLPredictionService>();
            _mockLogger = new Mock<ILogger<MLModelTrainingService>>();

            _trainingService = new MLModelTrainingService(
                _mockDataPipeline.Object,
                _mockPredictionService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task StartTrainingJobAsync_ValidRequest_ReturnsTrainingJob()
        {
            // Arrange
            var request = new ModelTrainingRequest
            {
                ModelName = "Test Model",
                ModelType = "Classification",
                Algorithm = "RandomForest",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                ValidateModel = true,
                DeployAfterTraining = false
            };

            // Act
            var result = await _trainingService.StartTrainingJobAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.TrainingJobId);
            Assert.NotEmpty(result.ModelId);
            Assert.Equal("Running", result.Status);
            Assert.True(result.StartedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task StartTrainingJobAsync_InvalidRequest_ReturnsFailedJob()
        {
            // Arrange
            var request = new ModelTrainingRequest
            {
                ModelName = "", // Invalid - empty name
                ModelType = "Classification",
                Algorithm = "RandomForest",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                ValidateModel = true,
                DeployAfterTraining = false
            };

            // Act
            var result = await _trainingService.StartTrainingJobAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Failed", result.Status);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("Model name is required", result.ErrorMessage);
        }

        [Fact]
        public async Task GetTrainingJobStatusAsync_ExistingJob_ReturnsJob()
        {
            // Arrange
            var request = new ModelTrainingRequest
            {
                ModelName = "Test Model",
                ModelType = "Classification",
                Algorithm = "RandomForest",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                ValidateModel = true,
                DeployAfterTraining = false
            };

            var trainingJob = await _trainingService.StartTrainingJobAsync(request);

            // Act
            var result = await _trainingService.GetTrainingJobStatusAsync(trainingJob.TrainingJobId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(trainingJob.TrainingJobId, result.TrainingJobId);
            Assert.Equal(trainingJob.ModelId, result.ModelId);
        }

        [Fact]
        public async Task GetTrainingJobStatusAsync_NonExistentJob_ReturnsNull()
        {
            // Arrange
            var nonExistentJobId = Guid.NewGuid().ToString();

            // Act
            var result = await _trainingService.GetTrainingJobStatusAsync(nonExistentJobId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTrainingJobsAsync_ReturnsJobs()
        {
            // Arrange
            var request = new ModelTrainingRequest
            {
                ModelName = "Test Model",
                ModelType = "Classification",
                Algorithm = "RandomForest",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                ValidateModel = true,
                DeployAfterTraining = false
            };

            await _trainingService.StartTrainingJobAsync(request);

            // Act
            var result = await _trainingService.GetTrainingJobsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public async Task CancelTrainingJobAsync_ExistingRunningJob_ReturnsTrue()
        {
            // Arrange
            var request = new ModelTrainingRequest
            {
                ModelName = "Test Model",
                ModelType = "Classification",
                Algorithm = "RandomForest",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                ValidateModel = true,
                DeployAfterTraining = false
            };

            var trainingJob = await _trainingService.StartTrainingJobAsync(request);

            // Act
            var result = await _trainingService.CancelTrainingJobAsync(trainingJob.TrainingJobId);

            // Assert
            Assert.True(result);

            // Verify job status was updated
            var jobStatus = await _trainingService.GetTrainingJobStatusAsync(trainingJob.TrainingJobId);
            Assert.Equal("Cancelled", jobStatus.Status);
        }

        [Fact]
        public async Task CancelTrainingJobAsync_NonExistentJob_ReturnsFalse()
        {
            // Arrange
            var nonExistentJobId = Guid.NewGuid().ToString();

            // Act
            var result = await _trainingService.CancelTrainingJobAsync(nonExistentJobId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAvailableAlgorithmsAsync_ReturnsAlgorithms()
        {
            // Act
            var result = await _trainingService.GetAvailableAlgorithmsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.Contains("RandomForest", result);
            Assert.Contains("GradientBoosting", result);
            Assert.Contains("LogisticRegression", result);
        }

        [Fact]
        public async Task ValidateTrainingRequestAsync_ValidRequest_ReturnsValid()
        {
            // Arrange
            var request = new ModelTrainingRequest
            {
                ModelName = "Test Model",
                ModelType = "Classification",
                Algorithm = "RandomForest",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                ValidateModel = true,
                DeployAfterTraining = false
            };

            // Act
            var result = await _trainingService.ValidateTrainingRequestAsync(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Issues);
        }

        [Fact]
        public async Task ValidateTrainingRequestAsync_InvalidRequest_ReturnsInvalid()
        {
            // Arrange
            var request = new ModelTrainingRequest
            {
                ModelName = "", // Invalid
                ModelType = "", // Invalid
                Algorithm = "InvalidAlgorithm", // Invalid
                StartDate = DateTime.UtcNow.AddDays(1), // Invalid - future date
                EndDate = DateTime.UtcNow.AddDays(-1), // Invalid - before start date
                ValidateModel = true,
                DeployAfterTraining = false
            };

            // Act
            var result = await _trainingService.ValidateTrainingRequestAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Issues.Count > 0);
        }

        [Fact]
        public async Task GetTrainingStatisticsAsync_ReturnsStatistics()
        {
            // Arrange
            var request = new ModelTrainingRequest
            {
                ModelName = "Test Model",
                ModelType = "Classification",
                Algorithm = "RandomForest",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                ValidateModel = true,
                DeployAfterTraining = false
            };

            await _trainingService.StartTrainingJobAsync(request);

            // Act
            var result = await _trainingService.GetTrainingStatisticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("TotalJobs"));
            Assert.True(result.ContainsKey("RunningJobs"));
            Assert.True(result.ContainsKey("CompletedJobs"));
            Assert.True(result.ContainsKey("FailedJobs"));
            Assert.True(result.ContainsKey("CancelledJobs"));
            Assert.True(result.ContainsKey("AverageTrainingTime"));
            Assert.True(result.ContainsKey("SuccessRate"));
        }
    }
} 