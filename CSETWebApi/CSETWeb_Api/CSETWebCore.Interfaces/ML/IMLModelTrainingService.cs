//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Model.ML;

namespace CSETWebCore.Interfaces.ML
{
    /// <summary>
    /// Interface for ML model training service operations
    /// </summary>
    public interface IMLModelTrainingService
    {
        /// <summary>
        /// Starts a new model training job
        /// </summary>
        /// <param name="request">Training request configuration</param>
        /// <returns>Training job result with job ID</returns>
        Task<ModelTrainingResult> StartTrainingJobAsync(ModelTrainingRequest request);

        /// <summary>
        /// Gets the status of a training job
        /// </summary>
        /// <param name="jobId">Training job ID</param>
        /// <returns>Current training job status</returns>
        Task<ModelTrainingResult> GetTrainingJobStatusAsync(string jobId);

        /// <summary>
        /// Gets all training jobs for the current user/organization
        /// </summary>
        /// <param name="includeCompleted">Whether to include completed jobs</param>
        /// <param name="limit">Maximum number of jobs to return</param>
        /// <returns>List of training jobs</returns>
        Task<List<ModelTrainingResult>> GetTrainingJobsAsync(bool includeCompleted = true, int limit = 50);

        /// <summary>
        /// Cancels a running training job
        /// </summary>
        /// <param name="jobId">Training job ID</param>
        /// <returns>Success status</returns>
        Task<bool> CancelTrainingJobAsync(string jobId);

        /// <summary>
        /// Deletes a completed training job and associated model
        /// </summary>
        /// <param name="jobId">Training job ID</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteTrainingJobAsync(string jobId);

        /// <summary>
        /// Gets training job logs for debugging
        /// </summary>
        /// <param name="jobId">Training job ID</param>
        /// <returns>Training logs</returns>
        Task<List<string>> GetTrainingJobLogsAsync(string jobId);

        /// <summary>
        /// Validates training request parameters
        /// </summary>
        /// <param name="request">Training request to validate</param>
        /// <returns>Validation result</returns>
        Task<DataValidationResult> ValidateTrainingRequestAsync(ModelTrainingRequest request);

        /// <summary>
        /// Gets available training algorithms
        /// </summary>
        /// <returns>List of available algorithms</returns>
        Task<List<string>> GetAvailableAlgorithmsAsync();

        /// <summary>
        /// Gets training job statistics
        /// </summary>
        /// <returns>Training statistics</returns>
        Task<Dictionary<string, object>> GetTrainingStatisticsAsync();
    }
} 