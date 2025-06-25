//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Interfaces.ML;
using CSETWebCore.Model.ML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Machine Learning controller for predictive analytics and intelligent recommendations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MLController : ControllerBase
    {
        private readonly IMLDataPipeline _dataPipeline;
        private readonly IMLPredictionService _predictionService;
        private readonly IMLModelTrainingService _trainingService;
        private readonly ILogger<MLController> _logger;

        public MLController(
            IMLDataPipeline dataPipeline,
            IMLPredictionService predictionService,
            IMLModelTrainingService trainingService,
            ILogger<MLController> logger)
        {
            _dataPipeline = dataPipeline;
            _predictionService = predictionService;
            _trainingService = trainingService;
            _logger = logger;
        }

        #region Data Pipeline Endpoints

        /// <summary>
        /// Collects assessment data for ML training
        /// </summary>
        /// <param name="startDate">Start date for data collection</param>
        /// <param name="endDate">End date for data collection</param>
        /// <returns>Collected assessment data</returns>
        [HttpGet("data/collect")]
        [ProducesResponseType(typeof(List<AssessmentDataPoint>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<AssessmentDataPoint>>> CollectAssessmentData(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Collecting assessment data from {StartDate} to {EndDate}", startDate, endDate);

                var data = await _dataPipeline.CollectAssessmentDataAsync(startDate, endDate);

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting assessment data");
                return StatusCode(500, new { error = "Failed to collect assessment data", details = ex.Message });
            }
        }

        /// <summary>
        /// Preprocesses assessment data for ML training
        /// </summary>
        /// <param name="rawData">Raw assessment data</param>
        /// <returns>Preprocessed data</returns>
        [HttpPost("data/preprocess")]
        [ProducesResponseType(typeof(PreprocessedData), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PreprocessedData>> PreprocessData([FromBody] List<AssessmentDataPoint> rawData)
        {
            try
            {
                _logger.LogInformation("Preprocessing {Count} data points", rawData.Count);

                var preprocessedData = await _dataPipeline.PreprocessDataAsync(rawData);

                return Ok(preprocessedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preprocessing data");
                return StatusCode(500, new { error = "Failed to preprocess data", details = ex.Message });
            }
        }

        /// <summary>
        /// Validates data quality for ML training
        /// </summary>
        /// <param name="data">Data to validate</param>
        /// <returns>Validation results</returns>
        [HttpPost("data/validate")]
        [ProducesResponseType(typeof(DataValidationResult), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DataValidationResult>> ValidateDataQuality([FromBody] List<AssessmentDataPoint> data)
        {
            try
            {
                _logger.LogInformation("Validating data quality for {Count} data points", data.Count);

                var validationResult = await _dataPipeline.ValidateDataQualityAsync(data);

                return Ok(validationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating data quality");
                return StatusCode(500, new { error = "Failed to validate data quality", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets data pipeline configuration
        /// </summary>
        /// <returns>Pipeline configuration</returns>
        [HttpGet("data/config")]
        [ProducesResponseType(typeof(MLPipelineConfiguration), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<MLPipelineConfiguration>> GetPipelineConfiguration()
        {
            try
            {
                var config = _dataPipeline.GetPipelineConfiguration();
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pipeline configuration");
                return StatusCode(500, new { error = "Failed to get pipeline configuration", details = ex.Message });
            }
        }

        #endregion

        #region Prediction Endpoints

        /// <summary>
        /// Makes a prediction using the specified model
        /// </summary>
        /// <param name="modelId">Model ID to use</param>
        /// <param name="features">Input features</param>
        /// <returns>Prediction result</returns>
        [HttpPost("predict/{modelId}")]
        [ProducesResponseType(typeof(MLPredictionResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<MLPredictionResult>> MakePrediction(
            string modelId,
            [FromBody] double[] features)
        {
            try
            {
                _logger.LogInformation("Making prediction with model {ModelId}", modelId);

                var prediction = await _predictionService.MakePredictionAsync(modelId, features);

                return Ok(prediction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid prediction request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making prediction");
                return StatusCode(500, new { error = "Failed to make prediction", details = ex.Message });
            }
        }

        /// <summary>
        /// Makes a prediction using assessment data
        /// </summary>
        /// <param name="modelId">Model ID to use</param>
        /// <param name="assessmentData">Assessment data</param>
        /// <returns>Prediction result</returns>
        [HttpPost("predict/{modelId}/assessment")]
        [ProducesResponseType(typeof(MLPredictionResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<MLPredictionResult>> PredictFromAssessment(
            string modelId,
            [FromBody] AssessmentDataPoint assessmentData)
        {
            try
            {
                _logger.LogInformation("Making prediction from assessment {AssessmentId} with model {ModelId}", 
                    assessmentData.AssessmentId, modelId);

                var prediction = await _predictionService.PredictFromAssessmentAsync(modelId, assessmentData);

                return Ok(prediction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid prediction request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making prediction from assessment");
                return StatusCode(500, new { error = "Failed to make prediction", details = ex.Message });
            }
        }

        /// <summary>
        /// Generates recommendations based on assessment data
        /// </summary>
        /// <param name="assessmentData">Assessment data</param>
        /// <param name="modelId">Optional model ID to use</param>
        /// <returns>List of recommendations</returns>
        [HttpPost("recommendations")]
        [ProducesResponseType(typeof(List<MLRecommendation>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MLRecommendation>>> GenerateRecommendations(
            [FromBody] AssessmentDataPoint assessmentData,
            [FromQuery] string? modelId = null)
        {
            try
            {
                _logger.LogInformation("Generating recommendations for assessment {AssessmentId}", assessmentData.AssessmentId);

                var recommendations = await _predictionService.GenerateRecommendationsAsync(assessmentData, modelId);

                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations");
                return StatusCode(500, new { error = "Failed to generate recommendations", details = ex.Message });
            }
        }

        #endregion

        #region Model Management Endpoints

        /// <summary>
        /// Registers a model in the prediction service
        /// </summary>
        /// <param name="modelMetadata">Model metadata</param>
        /// <returns>Success status</returns>
        [HttpPost("models")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> RegisterModel([FromBody] MLModelMetadata modelMetadata)
        {
            try
            {
                _logger.LogInformation("Registering model {ModelId} ({ModelName})", modelMetadata.ModelId, modelMetadata.ModelName);

                var success = await _predictionService.RegisterModelAsync(modelMetadata);

                if (!success)
                {
                    return BadRequest(new { error = "Failed to register model" });
                }

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering model");
                return StatusCode(500, new { error = "Failed to register model", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all registered models
        /// </summary>
        /// <returns>List of model metadata</returns>
        [HttpGet("models")]
        [ProducesResponseType(typeof(List<MLModelMetadata>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MLModelMetadata>>> GetRegisteredModels()
        {
            try
            {
                var models = await _predictionService.GetRegisteredModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registered models");
                return StatusCode(500, new { error = "Failed to get registered models", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets model metadata by ID
        /// </summary>
        /// <param name="modelId">Model ID</param>
        /// <returns>Model metadata if found</returns>
        [HttpGet("models/{modelId}")]
        [ProducesResponseType(typeof(MLModelMetadata), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<MLModelMetadata>> GetModelMetadata(string modelId)
        {
            try
            {
                var modelMetadata = await _predictionService.GetModelMetadataAsync(modelId);

                if (modelMetadata == null)
                {
                    return NotFound(new { error = "Model not found" });
                }

                return Ok(modelMetadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model metadata {ModelId}", modelId);
                return StatusCode(500, new { error = "Failed to get model metadata", details = ex.Message });
            }
        }

        /// <summary>
        /// Activates or deactivates a model
        /// </summary>
        /// <param name="modelId">Model ID</param>
        /// <param name="request">Activation request</param>
        /// <returns>Success status</returns>
        [HttpPatch("models/{modelId}/active")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> SetModelActive(
            string modelId,
            [FromBody] SetModelActiveRequest request)
        {
            try
            {
                _logger.LogInformation("Setting model {ModelId} active status to {IsActive}", modelId, request.IsActive);

                var success = await _predictionService.SetModelActiveAsync(modelId, request.IsActive);

                if (!success)
                {
                    return NotFound(new { error = "Model not found" });
                }

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting model active status {ModelId}", modelId);
                return StatusCode(500, new { error = "Failed to set model active status", details = ex.Message });
            }
        }

        #endregion

        #region Model Training Endpoints

        /// <summary>
        /// Starts a new model training job
        /// </summary>
        /// <param name="request">Training request configuration</param>
        /// <returns>Training job result with job ID</returns>
        [HttpPost("training/start")]
        [ProducesResponseType(typeof(ModelTrainingResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ModelTrainingResult>> StartTrainingJob([FromBody] ModelTrainingRequest request)
        {
            try
            {
                _logger.LogInformation("Starting training job for model {ModelName}", request.ModelName);

                var trainingJob = await _trainingService.StartTrainingJobAsync(request);

                return Ok(trainingJob);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid training request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting training job");
                return StatusCode(500, new { error = "Failed to start training job", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets the status of a training job
        /// </summary>
        /// <param name="jobId">Training job ID</param>
        /// <returns>Current training job status</returns>
        [HttpGet("training/{jobId}/status")]
        [ProducesResponseType(typeof(ModelTrainingResult), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ModelTrainingResult>> GetTrainingJobStatus(string jobId)
        {
            try
            {
                var trainingJob = await _trainingService.GetTrainingJobStatusAsync(jobId);

                if (trainingJob == null)
                {
                    return NotFound(new { error = "Training job not found" });
                }

                return Ok(trainingJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training job status {JobId}", jobId);
                return StatusCode(500, new { error = "Failed to get training job status", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all training jobs for the current user/organization
        /// </summary>
        /// <param name="includeCompleted">Whether to include completed jobs</param>
        /// <param name="limit">Maximum number of jobs to return</param>
        /// <returns>List of training jobs</returns>
        [HttpGet("training/jobs")]
        [ProducesResponseType(typeof(List<ModelTrainingResult>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<ModelTrainingResult>>> GetTrainingJobs(
            [FromQuery] bool includeCompleted = true,
            [FromQuery] int limit = 50)
        {
            try
            {
                var trainingJobs = await _trainingService.GetTrainingJobsAsync(includeCompleted, limit);
                return Ok(trainingJobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training jobs");
                return StatusCode(500, new { error = "Failed to get training jobs", details = ex.Message });
            }
        }

        /// <summary>
        /// Cancels a running training job
        /// </summary>
        /// <param name="jobId">Training job ID</param>
        /// <returns>Success status</returns>
        [HttpPost("training/{jobId}/cancel")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> CancelTrainingJob(string jobId)
        {
            try
            {
                _logger.LogInformation("Cancelling training job {JobId}", jobId);

                var success = await _trainingService.CancelTrainingJobAsync(jobId);

                if (!success)
                {
                    return NotFound(new { error = "Training job not found or cannot be cancelled" });
                }

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling training job {JobId}", jobId);
                return StatusCode(500, new { error = "Failed to cancel training job", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a completed training job and associated model
        /// </summary>
        /// <param name="jobId">Training job ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("training/{jobId}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> DeleteTrainingJob(string jobId)
        {
            try
            {
                _logger.LogInformation("Deleting training job {JobId}", jobId);

                var success = await _trainingService.DeleteTrainingJobAsync(jobId);

                if (!success)
                {
                    return NotFound(new { error = "Training job not found" });
                }

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting training job {JobId}", jobId);
                return StatusCode(500, new { error = "Failed to delete training job", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets training job logs for debugging
        /// </summary>
        /// <param name="jobId">Training job ID</param>
        /// <returns>Training logs</returns>
        [HttpGet("training/{jobId}/logs")]
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<string>>> GetTrainingJobLogs(string jobId)
        {
            try
            {
                var logs = await _trainingService.GetTrainingJobLogsAsync(jobId);

                if (logs.Count == 0)
                {
                    return NotFound(new { error = "Training job not found or no logs available" });
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training job logs {JobId}", jobId);
                return StatusCode(500, new { error = "Failed to get training job logs", details = ex.Message });
            }
        }

        /// <summary>
        /// Validates training request parameters
        /// </summary>
        /// <param name="request">Training request to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("training/validate")]
        [ProducesResponseType(typeof(DataValidationResult), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DataValidationResult>> ValidateTrainingRequest([FromBody] ModelTrainingRequest request)
        {
            try
            {
                _logger.LogInformation("Validating training request for model {ModelName}", request.ModelName);

                var validationResult = await _trainingService.ValidateTrainingRequestAsync(request);

                return Ok(validationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating training request");
                return StatusCode(500, new { error = "Failed to validate training request", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets available training algorithms
        /// </summary>
        /// <returns>List of available algorithms</returns>
        [HttpGet("training/algorithms")]
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<string>>> GetAvailableAlgorithms()
        {
            try
            {
                var algorithms = await _trainingService.GetAvailableAlgorithmsAsync();
                return Ok(algorithms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available algorithms");
                return StatusCode(500, new { error = "Failed to get available algorithms", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets training job statistics
        /// </summary>
        /// <returns>Training statistics</returns>
        [HttpGet("training/statistics")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Dictionary<string, object>>> GetTrainingStatistics()
        {
            try
            {
                var statistics = await _trainingService.GetTrainingStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training statistics");
                return StatusCode(500, new { error = "Failed to get training statistics", details = ex.Message });
            }
        }

        #endregion

        #region Health and Diagnostics Endpoints

        /// <summary>
        /// Gets ML service health status
        /// </summary>
        /// <returns>Health status information</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Dictionary<string, object>>> GetHealthStatus()
        {
            try
            {
                var trainingStats = await _trainingService.GetTrainingStatisticsAsync();
                
                var healthStatus = new Dictionary<string, object>
                {
                    ["Status"] = "Healthy",
                    ["Timestamp"] = DateTime.UtcNow,
                    ["RegisteredModels"] = (await _predictionService.GetRegisteredModelsAsync()).Count,
                    ["ActiveModels"] = (await _predictionService.GetRegisteredModelsAsync()).Count(m => m.IsActive),
                    ["DataPipeline"] = "Available",
                    ["PredictionService"] = "Available",
                    ["TrainingService"] = "Available",
                    ["TrainingJobs"] = trainingStats
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health status");
                return StatusCode(500, new { error = "Failed to get health status", details = ex.Message });
            }
        }

        /// <summary>
        /// Tests ML service connectivity
        /// </summary>
        /// <returns>Test result</returns>
        [HttpGet("health/test")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> TestConnectivity()
        {
            try
            {
                // Test data pipeline
                var config = _dataPipeline.GetPipelineConfiguration();
                
                // Test prediction service
                var models = await _predictionService.GetRegisteredModelsAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ML service connectivity test failed");
                return Ok(false);
            }
        }

        #endregion

        #region Request Models

        public class SetModelActiveRequest
        {
            [Required]
            public bool IsActive { get; set; }
        }

        #endregion
    }
} 