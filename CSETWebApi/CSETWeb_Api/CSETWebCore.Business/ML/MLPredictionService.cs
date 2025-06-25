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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CSETWebCore.Business.ML
{
    /// <summary>
    /// Machine Learning prediction service for generating predictions and recommendations
    /// </summary>
    public class MLPredictionService : IMLPredictionService
    {
        private readonly ILogger<MLPredictionService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMLDataPipeline _dataPipeline;
        private readonly Dictionary<string, MLModelMetadata> _modelRegistry;
        private readonly Dictionary<string, object> _activeModels;

        public MLPredictionService(
            ILogger<MLPredictionService> logger,
            IConfiguration configuration,
            IMLDataPipeline dataPipeline)
        {
            _logger = logger;
            _configuration = configuration;
            _dataPipeline = dataPipeline;
            _modelRegistry = new Dictionary<string, MLModelMetadata>();
            _activeModels = new Dictionary<string, object>();
        }

        /// <summary>
        /// Makes a prediction using the specified model
        /// </summary>
        /// <param name="modelId">Model ID to use for prediction</param>
        /// <param name="features">Input features</param>
        /// <returns>Prediction result</returns>
        public async Task<MLPredictionResult> MakePredictionAsync(string modelId, double[] features)
        {
            try
            {
                _logger.LogInformation("Making prediction with model {ModelId}", modelId);

                if (!_modelRegistry.ContainsKey(modelId))
                {
                    throw new ArgumentException($"Model {modelId} not found in registry");
                }

                var modelMetadata = _modelRegistry[modelId];
                if (!modelMetadata.IsActive)
                {
                    throw new InvalidOperationException($"Model {modelId} is not active");
                }

                // Get the model instance
                var model = GetModelInstance(modelId);
                if (model == null)
                {
                    throw new InvalidOperationException($"Model {modelId} instance not available");
                }

                // Make prediction using the model
                var prediction = await PredictWithModelAsync(model, features, modelMetadata);

                _logger.LogInformation("Prediction completed: {PredictedLabel} with confidence {Confidence}", 
                    prediction.PredictedLabel, prediction.Confidence);

                return prediction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making prediction with model {ModelId}", modelId);
                throw;
            }
        }

        /// <summary>
        /// Makes a prediction using assessment data
        /// </summary>
        /// <param name="modelId">Model ID to use</param>
        /// <param name="assessmentData">Assessment data point</param>
        /// <returns>Prediction result</returns>
        public async Task<MLPredictionResult> PredictFromAssessmentAsync(string modelId, AssessmentDataPoint assessmentData)
        {
            try
            {
                _logger.LogInformation("Making prediction from assessment {AssessmentId} with model {ModelId}", 
                    assessmentData.AssessmentId, modelId);

                // Extract features from assessment data
                var features = _dataPipeline.ExtractFeatures(assessmentData);

                // Make prediction
                var prediction = await MakePredictionAsync(modelId, features);

                // Add assessment-specific metadata
                prediction.Metadata["AssessmentId"] = assessmentData.AssessmentId;
                prediction.Metadata["AssessmentDate"] = assessmentData.AssessmentDate;
                prediction.Metadata["OrganizationSize"] = assessmentData.OrganizationSize;
                prediction.Metadata["AssetValue"] = assessmentData.AssetValue;

                return prediction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making prediction from assessment {AssessmentId}", assessmentData.AssessmentId);
                throw;
            }
        }

        /// <summary>
        /// Generates recommendations based on assessment data
        /// </summary>
        /// <param name="assessmentData">Assessment data</param>
        /// <param name="modelId">Optional model ID to use</param>
        /// <returns>List of recommendations</returns>
        public async Task<List<MLRecommendation>> GenerateRecommendationsAsync(AssessmentDataPoint assessmentData, string? modelId = null)
        {
            try
            {
                _logger.LogInformation("Generating recommendations for assessment {AssessmentId}", assessmentData.AssessmentId);

                var recommendations = new List<MLRecommendation>();

                // Generate risk-based recommendations
                var riskRecommendations = await GenerateRiskBasedRecommendationsAsync(assessmentData);
                recommendations.AddRange(riskRecommendations);

                // Generate ML-based recommendations if model is specified
                if (!string.IsNullOrEmpty(modelId))
                {
                    var mlRecommendations = await GenerateMLBasedRecommendationsAsync(assessmentData, modelId);
                    recommendations.AddRange(mlRecommendations);
                }

                // Generate improvement recommendations
                var improvementRecommendations = await GenerateImprovementRecommendationsAsync(assessmentData);
                recommendations.AddRange(improvementRecommendations);

                // Sort by priority and confidence
                recommendations = recommendations
                    .OrderByDescending(r => GetPriorityScore(r.Priority))
                    .ThenByDescending(r => r.Confidence)
                    .ToList();

                _logger.LogInformation("Generated {Count} recommendations for assessment {AssessmentId}", 
                    recommendations.Count, assessmentData.AssessmentId);

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations for assessment {AssessmentId}", assessmentData.AssessmentId);
                throw;
            }
        }

        /// <summary>
        /// Registers a model in the prediction service
        /// </summary>
        /// <param name="modelMetadata">Model metadata</param>
        /// <returns>Success status</returns>
        public async Task<bool> RegisterModelAsync(MLModelMetadata modelMetadata)
        {
            try
            {
                _logger.LogInformation("Registering model {ModelId} ({ModelName})", modelMetadata.ModelId, modelMetadata.ModelName);

                if (_modelRegistry.ContainsKey(modelMetadata.ModelId))
                {
                    _logger.LogWarning("Model {ModelId} already registered, updating metadata", modelMetadata.ModelId);
                }

                _modelRegistry[modelMetadata.ModelId] = modelMetadata;

                // Load the model if it's active
                if (modelMetadata.IsActive)
                {
                    await LoadModelAsync(modelMetadata);
                }

                _logger.LogInformation("Model {ModelId} registered successfully", modelMetadata.ModelId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering model {ModelId}", modelMetadata.ModelId);
                return false;
            }
        }

        /// <summary>
        /// Gets all registered models
        /// </summary>
        /// <returns>List of model metadata</returns>
        public async Task<List<MLModelMetadata>> GetRegisteredModelsAsync()
        {
            return await Task.FromResult(_modelRegistry.Values.ToList());
        }

        /// <summary>
        /// Gets model metadata by ID
        /// </summary>
        /// <param name="modelId">Model ID</param>
        /// <returns>Model metadata if found</returns>
        public async Task<MLModelMetadata?> GetModelMetadataAsync(string modelId)
        {
            return await Task.FromResult(_modelRegistry.GetValueOrDefault(modelId));
        }

        /// <summary>
        /// Activates or deactivates a model
        /// </summary>
        /// <param name="modelId">Model ID</param>
        /// <param name="isActive">Whether to activate the model</param>
        /// <returns>Success status</returns>
        public async Task<bool> SetModelActiveAsync(string modelId, bool isActive)
        {
            try
            {
                if (!_modelRegistry.ContainsKey(modelId))
                {
                    throw new ArgumentException($"Model {modelId} not found");
                }

                var modelMetadata = _modelRegistry[modelId];
                modelMetadata.IsActive = isActive;

                if (isActive)
                {
                    await LoadModelAsync(modelMetadata);
                }
                else
                {
                    await UnloadModelAsync(modelId);
                }

                _logger.LogInformation("Model {ModelId} {Status}", modelId, isActive ? "activated" : "deactivated");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting model {ModelId} active status to {IsActive}", modelId, isActive);
                return false;
            }
        }

        #region Private Helper Methods

        private async Task<MLPredictionResult> PredictWithModelAsync(object model, double[] features, MLModelMetadata modelMetadata)
        {
            // This is a simplified implementation
            // In production, this would use actual ML libraries like ML.NET, TensorFlow, or ONNX Runtime
            
            var prediction = new MLPredictionResult
            {
                ModelId = modelMetadata.ModelId,
                InputFeatures = features,
                PredictedAt = DateTime.UtcNow
            };

            // Simple rule-based prediction for demonstration
            // In production, this would use the actual trained model
            var complianceScore = features.Length > 2 ? features[2] : 0; // Compliance score feature
            
            if (complianceScore >= 80)
            {
                prediction.PredictedLabel = "LowRisk";
                prediction.Confidence = 0.85;
                prediction.ClassProbabilities = new Dictionary<string, double>
                {
                    ["LowRisk"] = 0.85,
                    ["MediumRisk"] = 0.10,
                    ["HighRisk"] = 0.05
                };
            }
            else if (complianceScore >= 60)
            {
                prediction.PredictedLabel = "MediumRisk";
                prediction.Confidence = 0.75;
                prediction.ClassProbabilities = new Dictionary<string, double>
                {
                    ["LowRisk"] = 0.15,
                    ["MediumRisk"] = 0.75,
                    ["HighRisk"] = 0.10
                };
            }
            else
            {
                prediction.PredictedLabel = "HighRisk";
                prediction.Confidence = 0.90;
                prediction.ClassProbabilities = new Dictionary<string, double>
                {
                    ["LowRisk"] = 0.05,
                    ["MediumRisk"] = 0.15,
                    ["HighRisk"] = 0.80
                };
            }

            // Calculate feature importance (simplified)
            prediction.FeatureImportance = CalculateFeatureImportance(features, modelMetadata.FeatureNames);

            return prediction;
        }

        private Dictionary<string, double> CalculateFeatureImportance(double[] features, string[] featureNames)
        {
            var importance = new Dictionary<string, double>();
            
            for (int i = 0; i < Math.Min(features.Length, featureNames.Length); i++)
            {
                // Simple importance calculation based on feature magnitude
                importance[featureNames[i]] = Math.Abs(features[i]);
            }

            // Normalize importance scores
            var maxImportance = importance.Values.Max();
            if (maxImportance > 0)
            {
                foreach (var key in importance.Keys.ToList())
                {
                    importance[key] /= maxImportance;
                }
            }

            return importance;
        }

        private async Task<List<MLRecommendation>> GenerateRiskBasedRecommendationsAsync(AssessmentDataPoint assessmentData)
        {
            var recommendations = new List<MLRecommendation>();

            // High risk recommendations
            if (assessmentData.RiskScore > 70)
            {
                recommendations.Add(new MLRecommendation
                {
                    Title = "Address High-Risk Areas",
                    Description = "Your assessment shows several high-risk areas that require immediate attention.",
                    Category = "Risk Management",
                    Priority = "Critical",
                    Impact = "High",
                    Effort = "High",
                    Confidence = 0.95,
                    Evidence = new List<string> { $"Risk score: {assessmentData.RiskScore:F1}%" }
                });
            }

            // Low compliance recommendations
            if (assessmentData.ComplianceScore < 60)
            {
                recommendations.Add(new MLRecommendation
                {
                    Title = "Improve Compliance Score",
                    Description = "Focus on implementing controls to improve your overall compliance score.",
                    Category = "Compliance",
                    Priority = "High",
                    Impact = "High",
                    Effort = "Medium",
                    Confidence = 0.90,
                    Evidence = new List<string> { $"Compliance score: {assessmentData.ComplianceScore:F1}%" }
                });
            }

            return recommendations;
        }

        private async Task<List<MLRecommendation>> GenerateMLBasedRecommendationsAsync(AssessmentDataPoint assessmentData, string modelId)
        {
            var recommendations = new List<MLRecommendation>();

            try
            {
                // Make prediction to get ML-based insights
                var prediction = await PredictFromAssessmentAsync(modelId, assessmentData);

                // Generate recommendations based on prediction
                if (prediction.PredictedLabel == "HighRisk")
                {
                    recommendations.Add(new MLRecommendation
                    {
                        Title = "ML-Identified Risk Mitigation",
                        Description = "Machine learning analysis indicates high risk. Consider implementing additional security controls.",
                        Category = "ML Insights",
                        Priority = "High",
                        Impact = "High",
                        Effort = "Medium",
                        Confidence = prediction.Confidence,
                        Evidence = new List<string> 
                        { 
                            $"ML prediction: {prediction.PredictedLabel}",
                            $"Confidence: {prediction.Confidence:P1}"
                        }
                    });
                }

                // Add recommendations based on feature importance
                var topFeatures = prediction.FeatureImportance
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(3);

                foreach (var feature in topFeatures)
                {
                    recommendations.Add(new MLRecommendation
                    {
                        Title = $"Focus on {feature.Key}",
                        Description = $"This factor has high importance in the ML model. Consider reviewing related controls.",
                        Category = "ML Insights",
                        Priority = "Medium",
                        Impact = "Medium",
                        Effort = "Low",
                        Confidence = feature.Value,
                        Evidence = new List<string> { $"Feature importance: {feature.Value:P1}" }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generating ML-based recommendations");
            }

            return recommendations;
        }

        private async Task<List<MLRecommendation>> GenerateImprovementRecommendationsAsync(AssessmentDataPoint assessmentData)
        {
            var recommendations = new List<MLRecommendation>();

            // Find categories with low scores
            var lowScoreCategories = assessmentData.CategoryScores
                .Where(kvp => kvp.Value < 60)
                .OrderBy(kvp => kvp.Value)
                .Take(5);

            foreach (var category in lowScoreCategories)
            {
                recommendations.Add(new MLRecommendation
                {
                    Title = $"Improve {category.Key} Category",
                    Description = $"Focus on improving controls in the {category.Key} category.",
                    Category = "Improvement",
                    Priority = "Medium",
                    Impact = "Medium",
                    Effort = "Medium",
                    Confidence = 0.80,
                    Evidence = new List<string> { $"Category score: {category.Value:F1}%" },
                    RelatedQuestions = new List<string> { category.Key }
                });
            }

            return recommendations;
        }

        private int GetPriorityScore(string priority)
        {
            return priority.ToLowerInvariant() switch
            {
                "critical" => 4,
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 0
            };
        }

        private object? GetModelInstance(string modelId)
        {
            return _activeModels.GetValueOrDefault(modelId);
        }

        private async Task LoadModelAsync(MLModelMetadata modelMetadata)
        {
            // In production, this would load the actual model file
            // For now, we'll just create a placeholder
            _activeModels[modelMetadata.ModelId] = new { ModelId = modelMetadata.ModelId };
            await Task.CompletedTask;
        }

        private async Task UnloadModelAsync(string modelId)
        {
            _activeModels.Remove(modelId);
            await Task.CompletedTask;
        }

        #endregion
    }
} 