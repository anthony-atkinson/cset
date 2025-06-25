//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.Interfaces.ML;
using CSETWebCore.Model.ML;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace CSETWebCore.Business.ML
{
    /// <summary>
    /// Implementation of ML model training service
    /// </summary>
    public class MLModelTrainingService : IMLModelTrainingService
    {
        private readonly IMLDataPipeline _dataPipeline;
        private readonly IMLPredictionService _predictionService;
        private readonly ILogger<MLModelTrainingService> _logger;
        private readonly Dictionary<string, ModelTrainingResult> _trainingJobs;
        private readonly Dictionary<string, List<string>> _trainingLogs;
        private readonly object _lockObject = new object();

        public MLModelTrainingService(
            IMLDataPipeline dataPipeline,
            IMLPredictionService predictionService,
            ILogger<MLModelTrainingService> logger)
        {
            _dataPipeline = dataPipeline;
            _predictionService = predictionService;
            _logger = logger;
            _trainingJobs = new Dictionary<string, ModelTrainingResult>();
            _trainingLogs = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Starts a new model training job
        /// </summary>
        public async Task<ModelTrainingResult> StartTrainingJobAsync(ModelTrainingRequest request)
        {
            try
            {
                _logger.LogInformation("Starting training job for model {ModelName}", request.ModelName);

                // Validate the training request
                var validationResult = await ValidateTrainingRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    var errorResult = new ModelTrainingResult
                    {
                        TrainingJobId = Guid.NewGuid().ToString(),
                        ModelId = Guid.NewGuid().ToString(),
                        Status = "Failed",
                        ErrorMessage = string.Join("; ", validationResult.Issues),
                        StartedAt = DateTime.UtcNow,
                        CompletedAt = DateTime.UtcNow
                    };

                    lock (_lockObject)
                    {
                        _trainingJobs[errorResult.TrainingJobId] = errorResult;
                    }

                    return errorResult;
                }

                // Create training job
                var trainingJob = new ModelTrainingResult
                {
                    TrainingJobId = Guid.NewGuid().ToString(),
                    ModelId = Guid.NewGuid().ToString(),
                    Status = "Running",
                    StartedAt = DateTime.UtcNow
                };

                // Store the job
                lock (_lockObject)
                {
                    _trainingJobs[trainingJob.TrainingJobId] = trainingJob;
                    _trainingLogs[trainingJob.TrainingJobId] = new List<string>();
                }

                // Start training in background
                _ = Task.Run(async () => await ExecuteTrainingJobAsync(trainingJob.TrainingJobId, request));

                return trainingJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting training job for model {ModelName}", request.ModelName);
                throw;
            }
        }

        /// <summary>
        /// Gets the status of a training job
        /// </summary>
        public async Task<ModelTrainingResult> GetTrainingJobStatusAsync(string jobId)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_trainingJobs.TryGetValue(jobId, out var job))
                    {
                        return Task.FromResult(job);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training job status for {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Gets all training jobs for the current user/organization
        /// </summary>
        public async Task<List<ModelTrainingResult>> GetTrainingJobsAsync(bool includeCompleted = true, int limit = 50)
        {
            try
            {
                lock (_lockObject)
                {
                    var jobs = _trainingJobs.Values.ToList();
                    
                    if (!includeCompleted)
                    {
                        jobs = jobs.Where(j => j.Status == "Running" || j.Status == "Pending").ToList();
                    }

                    return jobs.OrderByDescending(j => j.StartedAt).Take(limit).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training jobs");
                throw;
            }
        }

        /// <summary>
        /// Cancels a running training job
        /// </summary>
        public async Task<bool> CancelTrainingJobAsync(string jobId)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_trainingJobs.TryGetValue(jobId, out var job))
                    {
                        if (job.Status == "Running" || job.Status == "Pending")
                        {
                            job.Status = "Cancelled";
                            job.CompletedAt = DateTime.UtcNow;
                            job.Duration = job.CompletedAt - job.StartedAt;
                            
                            _logger.LogInformation("Training job {JobId} cancelled", jobId);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling training job {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Deletes a completed training job and associated model
        /// </summary>
        public async Task<bool> DeleteTrainingJobAsync(string jobId)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_trainingJobs.TryGetValue(jobId, out var job))
                    {
                        _trainingJobs.Remove(jobId);
                        _trainingLogs.Remove(jobId);
                        
                        _logger.LogInformation("Training job {JobId} deleted", jobId);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting training job {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Gets training job logs for debugging
        /// </summary>
        public async Task<List<string>> GetTrainingJobLogsAsync(string jobId)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_trainingLogs.TryGetValue(jobId, out var logs))
                    {
                        return logs.ToList();
                    }
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training job logs for {JobId}", jobId);
                throw;
            }
        }

        /// <summary>
        /// Validates training request parameters
        /// </summary>
        public async Task<DataValidationResult> ValidateTrainingRequestAsync(ModelTrainingRequest request)
        {
            var result = new DataValidationResult { IsValid = true };

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.ModelName))
                {
                    result.IsValid = false;
                    result.Issues.Add("Model name is required");
                }

                if (string.IsNullOrWhiteSpace(request.ModelType))
                {
                    result.IsValid = false;
                    result.Issues.Add("Model type is required");
                }

                if (string.IsNullOrWhiteSpace(request.Algorithm))
                {
                    result.IsValid = false;
                    result.Issues.Add("Algorithm is required");
                }

                // Validate date range
                if (request.StartDate >= request.EndDate)
                {
                    result.IsValid = false;
                    result.Issues.Add("Start date must be before end date");
                }

                if (request.StartDate > DateTime.UtcNow)
                {
                    result.IsValid = false;
                    result.Issues.Add("Start date cannot be in the future");
                }

                // Validate algorithm availability
                var availableAlgorithms = await GetAvailableAlgorithmsAsync();
                if (!availableAlgorithms.Contains(request.Algorithm))
                {
                    result.IsValid = false;
                    result.Issues.Add($"Algorithm '{request.Algorithm}' is not available");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating training request");
                result.IsValid = false;
                result.Issues.Add($"Validation error: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Gets available training algorithms
        /// </summary>
        public async Task<List<string>> GetAvailableAlgorithmsAsync()
        {
            return new List<string>
            {
                "RandomForest",
                "GradientBoosting",
                "LogisticRegression",
                "SupportVectorMachine",
                "NeuralNetwork",
                "DecisionTree",
                "KNearestNeighbors",
                "NaiveBayes"
            };
        }

        /// <summary>
        /// Gets training job statistics
        /// </summary>
        public async Task<Dictionary<string, object>> GetTrainingStatisticsAsync()
        {
            try
            {
                lock (_lockObject)
                {
                    var jobs = _trainingJobs.Values.ToList();
                    
                    return new Dictionary<string, object>
                    {
                        ["TotalJobs"] = jobs.Count,
                        ["RunningJobs"] = jobs.Count(j => j.Status == "Running"),
                        ["CompletedJobs"] = jobs.Count(j => j.Status == "Completed"),
                        ["FailedJobs"] = jobs.Count(j => j.Status == "Failed"),
                        ["CancelledJobs"] = jobs.Count(j => j.Status == "Cancelled"),
                        ["AverageTrainingTime"] = jobs.Where(j => j.Duration.HasValue).Select(j => j.Duration.Value.TotalMinutes).DefaultIfEmpty(0).Average(),
                        ["SuccessRate"] = jobs.Count > 0 ? (double)jobs.Count(j => j.Status == "Completed") / jobs.Count * 100 : 0
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training statistics");
                throw;
            }
        }

        /// <summary>
        /// Executes the training job in the background
        /// </summary>
        private async Task ExecuteTrainingJobAsync(string jobId, ModelTrainingRequest request)
        {
            try
            {
                AddLog(jobId, $"Starting training job for model '{request.ModelName}'");
                AddLog(jobId, $"Algorithm: {request.Algorithm}, Model Type: {request.ModelType}");

                // Simulate data collection
                AddLog(jobId, "Collecting training data...");
                var trainingData = await _dataPipeline.CollectAssessmentDataAsync(request.StartDate, request.EndDate);
                AddLog(jobId, $"Collected {trainingData.Count} data points");

                // Simulate data preprocessing
                AddLog(jobId, "Preprocessing data...");
                var preprocessedData = await _dataPipeline.PreprocessDataAsync(trainingData);
                AddLog(jobId, $"Preprocessed {preprocessedData.Features.Count} feature vectors");

                // Simulate model training
                AddLog(jobId, "Training model...");
                await Task.Delay(5000); // Simulate training time

                // Update job status
                lock (_lockObject)
                {
                    if (_trainingJobs.TryGetValue(jobId, out var job))
                    {
                        job.Status = "Completed";
                        job.CompletedAt = DateTime.UtcNow;
                        job.Duration = job.CompletedAt - job.StartedAt;
                        job.TrainingSamples = trainingData.Count;
                        job.ValidationSamples = trainingData.Count / 5; // 20% for validation
                        job.Metrics = new Dictionary<string, double>
                        {
                            ["Accuracy"] = 0.85 + (new Random().NextDouble() * 0.1),
                            ["Precision"] = 0.82 + (new Random().NextDouble() * 0.1),
                            ["Recall"] = 0.88 + (new Random().NextDouble() * 0.1),
                            ["F1Score"] = 0.85 + (new Random().NextDouble() * 0.1)
                        };
                    }
                }

                AddLog(jobId, "Training completed successfully");

                // Register model if requested
                if (request.DeployAfterTraining)
                {
                    AddLog(jobId, "Registering model for deployment...");
                    var modelMetadata = new MLModelMetadata
                    {
                        ModelId = Guid.NewGuid().ToString(),
                        ModelName = request.ModelName,
                        Version = "1.0.0",
                        ModelType = request.ModelType,
                        Algorithm = request.Algorithm,
                        TrainedAt = DateTime.UtcNow,
                        Accuracy = 0.85,
                        Precision = 0.82,
                        Recall = 0.88,
                        F1Score = 0.85,
                        TrainingSamples = trainingData.Count,
                        IsActive = true
                    };

                    await _predictionService.RegisterModelAsync(modelMetadata);
                    AddLog(jobId, "Model registered and activated");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing training job {JobId}", jobId);
                
                lock (_lockObject)
                {
                    if (_trainingJobs.TryGetValue(jobId, out var job))
                    {
                        job.Status = "Failed";
                        job.ErrorMessage = ex.Message;
                        job.CompletedAt = DateTime.UtcNow;
                        job.Duration = job.CompletedAt - job.StartedAt;
                    }
                }

                AddLog(jobId, $"Training failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a log entry to the training job
        /// </summary>
        private void AddLog(string jobId, string message)
        {
            lock (_lockObject)
            {
                if (_trainingLogs.TryGetValue(jobId, out var logs))
                {
                    logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}");
                }
            }
        }
    }
} 