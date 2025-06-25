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
    /// Interface for ML prediction service operations
    /// </summary>
    public interface IMLPredictionService
    {
        /// <summary>
        /// Makes a prediction using the specified model
        /// </summary>
        /// <param name="modelId">Model ID to use for prediction</param>
        /// <param name="features">Input features</param>
        /// <returns>Prediction result</returns>
        Task<MLPredictionResult> MakePredictionAsync(string modelId, double[] features);

        /// <summary>
        /// Makes a prediction using assessment data
        /// </summary>
        /// <param name="modelId">Model ID to use</param>
        /// <param name="assessmentData">Assessment data point</param>
        /// <returns>Prediction result</returns>
        Task<MLPredictionResult> PredictFromAssessmentAsync(string modelId, AssessmentDataPoint assessmentData);

        /// <summary>
        /// Generates recommendations based on assessment data
        /// </summary>
        /// <param name="assessmentData">Assessment data</param>
        /// <param name="modelId">Optional model ID to use</param>
        /// <returns>List of recommendations</returns>
        Task<List<MLRecommendation>> GenerateRecommendationsAsync(AssessmentDataPoint assessmentData, string? modelId = null);

        /// <summary>
        /// Registers a model in the prediction service
        /// </summary>
        /// <param name="modelMetadata">Model metadata</param>
        /// <returns>Success status</returns>
        Task<bool> RegisterModelAsync(MLModelMetadata modelMetadata);

        /// <summary>
        /// Gets all registered models
        /// </summary>
        /// <returns>List of model metadata</returns>
        Task<List<MLModelMetadata>> GetRegisteredModelsAsync();

        /// <summary>
        /// Gets model metadata by ID
        /// </summary>
        /// <param name="modelId">Model ID</param>
        /// <returns>Model metadata if found</returns>
        Task<MLModelMetadata?> GetModelMetadataAsync(string modelId);

        /// <summary>
        /// Activates or deactivates a model
        /// </summary>
        /// <param name="modelId">Model ID</param>
        /// <param name="isActive">Whether to activate the model</param>
        /// <returns>Success status</returns>
        Task<bool> SetModelActiveAsync(string modelId, bool isActive);
    }
} 