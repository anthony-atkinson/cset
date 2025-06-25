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
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Model.ML;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CSETWebCore.Business.ML
{
    /// <summary>
    /// Machine Learning data pipeline for collecting and preprocessing assessment data
    /// </summary>
    public class MLDataPipeline : IMLDataPipeline
    {
        private readonly CSETContext _context;
        private readonly ILogger<MLDataPipeline> _logger;
        private readonly IConfiguration _configuration;

        public MLDataPipeline(CSETContext context, ILogger<MLDataPipeline> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Collects assessment data for ML training
        /// </summary>
        /// <param name="startDate">Start date for data collection</param>
        /// <param name="endDate">End date for data collection</param>
        /// <returns>Collected assessment data</returns>
        public async Task<List<AssessmentDataPoint>> CollectAssessmentDataAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Collecting assessment data from {StartDate} to {EndDate}", startDate, endDate);

                var assessments = await _context.ASSESSMENTS
                    .Where(a => a.Assessment_Date >= startDate && a.Assessment_Date <= endDate)
                    .Include(a => a.DEMOGRAPHICS)
                    .Include(a => a.ANSWER)
                    .ThenInclude(ans => ans.QUESTION)
                    .ToListAsync();

                var dataPoints = new List<AssessmentDataPoint>();

                foreach (var assessment in assessments)
                {
                    var dataPoint = await CreateAssessmentDataPointAsync(assessment);
                    dataPoints.Add(dataPoint);
                }

                _logger.LogInformation("Collected {Count} assessment data points", dataPoints.Count);
                return dataPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting assessment data");
                throw;
            }
        }

        /// <summary>
        /// Preprocesses assessment data for ML training
        /// </summary>
        /// <param name="rawData">Raw assessment data</param>
        /// <returns>Preprocessed data ready for training</returns>
        public async Task<PreprocessedData> PreprocessDataAsync(List<AssessmentDataPoint> rawData)
        {
            try
            {
                _logger.LogInformation("Preprocessing {Count} data points", rawData.Count);

                var preprocessed = new PreprocessedData
                {
                    Features = new List<double[]>(),
                    Labels = new List<string>(),
                    FeatureNames = GetFeatureNames(),
                    Metadata = new Dictionary<string, object>()
                };

                foreach (var dataPoint in rawData)
                {
                    var features = ExtractFeatures(dataPoint);
                    var label = DetermineLabel(dataPoint);

                    preprocessed.Features.Add(features);
                    preprocessed.Labels.Add(label);
                }

                // Normalize features
                preprocessed.Features = NormalizeFeatures(preprocessed.Features);

                _logger.LogInformation("Preprocessing completed. Features: {FeatureCount}, Labels: {LabelCount}", 
                    preprocessed.Features.Count, preprocessed.Labels.Count);

                return preprocessed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preprocessing data");
                throw;
            }
        }

        /// <summary>
        /// Extracts features from assessment data for ML models
        /// </summary>
        /// <param name="dataPoint">Assessment data point</param>
        /// <returns>Feature vector</returns>
        public double[] ExtractFeatures(AssessmentDataPoint dataPoint)
        {
            var features = new List<double>();

            // Basic assessment features
            features.Add(dataPoint.TotalQuestions);
            features.Add(dataPoint.CompliantQuestions);
            features.Add(dataPoint.ComplianceScore);
            features.Add(dataPoint.RiskScore);

            // Demographic features (one-hot encoded)
            features.AddRange(EncodeOrganizationSize(dataPoint.OrganizationSize));
            features.AddRange(EncodeAssetValue(dataPoint.AssetValue));
            features.AddRange(EncodeSector(dataPoint.SectorId));
            features.AddRange(EncodeIndustry(dataPoint.IndustryId));

            // Question category features
            features.AddRange(ExtractCategoryFeatures(dataPoint.CategoryScores));

            // Temporal features
            features.Add(ExtractTemporalFeatures(dataPoint.AssessmentDate));

            return features.ToArray();
        }

        /// <summary>
        /// Creates a data pipeline configuration
        /// </summary>
        /// <returns>Pipeline configuration</returns>
        public MLPipelineConfiguration GetPipelineConfiguration()
        {
            return new MLPipelineConfiguration
            {
                DataRetentionDays = _configuration.GetValue<int>("ML:DataRetentionDays", 365),
                BatchSize = _configuration.GetValue<int>("ML:BatchSize", 1000),
                FeatureNormalization = _configuration.GetValue<bool>("ML:FeatureNormalization", true),
                DataValidationEnabled = _configuration.GetValue<bool>("ML:DataValidationEnabled", true),
                MaxDataPoints = _configuration.GetValue<int>("ML:MaxDataPoints", 10000)
            };
        }

        /// <summary>
        /// Validates data quality for ML training
        /// </summary>
        /// <param name="data">Data to validate</param>
        /// <returns>Validation results</returns>
        public async Task<DataValidationResult> ValidateDataQualityAsync(List<AssessmentDataPoint> data)
        {
            var result = new DataValidationResult
            {
                IsValid = true,
                Issues = new List<string>(),
                Statistics = new Dictionary<string, object>()
            };

            if (!data.Any())
            {
                result.IsValid = false;
                result.Issues.Add("No data points available");
                return result;
            }

            // Check for missing values
            var missingValues = data.Count(d => d.ComplianceScore < 0 || d.TotalQuestions <= 0);
            if (missingValues > 0)
            {
                result.Issues.Add($"{missingValues} data points have missing or invalid values");
            }

            // Check data distribution
            var complianceScores = data.Select(d => d.ComplianceScore).ToList();
            result.Statistics["MeanComplianceScore"] = complianceScores.Average();
            result.Statistics["StdDevComplianceScore"] = CalculateStandardDeviation(complianceScores);
            result.Statistics["MinComplianceScore"] = complianceScores.Min();
            result.Statistics["MaxComplianceScore"] = complianceScores.Max();

            // Check for class imbalance
            var lowRisk = data.Count(d => d.ComplianceScore >= 80);
            var mediumRisk = data.Count(d => d.ComplianceScore >= 60 && d.ComplianceScore < 80);
            var highRisk = data.Count(d => d.ComplianceScore < 60);

            result.Statistics["LowRiskCount"] = lowRisk;
            result.Statistics["MediumRiskCount"] = mediumRisk;
            result.Statistics["HighRiskCount"] = highRisk;

            if (highRisk < data.Count * 0.1 || lowRisk < data.Count * 0.1)
            {
                result.Issues.Add("Data shows class imbalance - consider data augmentation");
            }

            return result;
        }

        #region Private Helper Methods

        private async Task<AssessmentDataPoint> CreateAssessmentDataPointAsync(ASSESSMENTS assessment)
        {
            var dataPoint = new AssessmentDataPoint
            {
                AssessmentId = assessment.Assessment_Id,
                AssessmentDate = assessment.Assessment_Date,
                OrganizationSize = assessment.DEMOGRAPHICS?.Size ?? "Unknown",
                AssetValue = assessment.DEMOGRAPHICS?.AssetValue ?? "Unknown",
                SectorId = assessment.DEMOGRAPHICS?.SectorId ?? 0,
                IndustryId = assessment.DEMOGRAPHICS?.IndustryId ?? 0,
                CategoryScores = new Dictionary<string, double>()
            };

            // Calculate compliance metrics
            var answers = assessment.ANSWER?.ToList() ?? new List<ANSWER>();
            dataPoint.TotalQuestions = answers.Count;
            dataPoint.CompliantQuestions = answers.Count(a => a.Answer_Text == "Y" || a.Answer_Text == "A");
            dataPoint.ComplianceScore = dataPoint.TotalQuestions > 0 
                ? (double)dataPoint.CompliantQuestions / dataPoint.TotalQuestions * 100 
                : 0;

            // Calculate risk score
            dataPoint.RiskScore = CalculateRiskScore(answers);

            // Calculate category scores
            dataPoint.CategoryScores = await CalculateCategoryScoresAsync(answers);

            return dataPoint;
        }

        private double CalculateRiskScore(List<ANSWER> answers)
        {
            if (!answers.Any()) return 0;

            var riskFactors = new Dictionary<string, double>
            {
                ["N"] = 10.0, // No answer - high risk
                ["U"] = 5.0,  // Unanswered - medium risk
                ["Y"] = 0.0,  // Yes answer - no risk
                ["A"] = 0.0   // Alternative answer - no risk
            };

            var totalRisk = answers.Sum(a => riskFactors.GetValueOrDefault(a.Answer_Text, 5.0));
            return Math.Min(totalRisk / answers.Count, 100.0);
        }

        private async Task<Dictionary<string, double>> CalculateCategoryScoresAsync(List<ANSWER> answers)
        {
            var categoryScores = new Dictionary<string, double>();

            var categories = answers
                .Where(a => !string.IsNullOrEmpty(a.Question_Number))
                .GroupBy(a => a.Question_Number?.Split('.')[0] ?? "Unknown")
                .ToList();

            foreach (var category in categories)
            {
                var compliantAnswers = category.Count(a => a.Answer_Text == "Y" || a.Answer_Text == "A");
                var totalAnswers = category.Count();
                var score = totalAnswers > 0 ? (double)compliantAnswers / totalAnswers * 100 : 0;
                categoryScores[category.Key] = score;
            }

            return categoryScores;
        }

        private string DetermineLabel(AssessmentDataPoint dataPoint)
        {
            if (dataPoint.ComplianceScore >= 80) return "LowRisk";
            if (dataPoint.ComplianceScore >= 60) return "MediumRisk";
            return "HighRisk";
        }

        private List<double> EncodeOrganizationSize(string size)
        {
            var sizes = new[] { "Tiny", "Small", "Medium", "Large", "Very Large" };
            return sizes.Select(s => s == size ? 1.0 : 0.0).ToList();
        }

        private List<double> EncodeAssetValue(string value)
        {
            var values = new[] { "Very Low", "Low", "Medium", "High", "Very High" };
            return values.Select(v => v == value ? 1.0 : 0.0).ToList();
        }

        private List<double> EncodeSector(int sectorId)
        {
            // Simplified encoding - in production, this would be based on actual sector data
            var sectors = Enumerable.Range(1, 10).ToArray();
            return sectors.Select(s => s == sectorId ? 1.0 : 0.0).ToList();
        }

        private List<double> EncodeIndustry(int industryId)
        {
            // Simplified encoding - in production, this would be based on actual industry data
            var industries = Enumerable.Range(1, 20).ToArray();
            return industries.Select(i => i == industryId ? 1.0 : 0.0).ToList();
        }

        private List<double> ExtractCategoryFeatures(Dictionary<string, double> categoryScores)
        {
            // Extract top 10 category scores as features
            var topCategories = categoryScores
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .Select(kvp => kvp.Value)
                .ToList();

            // Pad with zeros if less than 10 categories
            while (topCategories.Count < 10)
            {
                topCategories.Add(0.0);
            }

            return topCategories;
        }

        private double ExtractTemporalFeatures(DateTime assessmentDate)
        {
            // Convert date to days since epoch for temporal features
            return (assessmentDate - new DateTime(1970, 1, 1)).TotalDays;
        }

        private List<double[]> NormalizeFeatures(List<double[]> features)
        {
            if (!features.Any()) return features;

            var featureCount = features[0].Length;
            var normalized = new List<double[]>();

            // Calculate min/max for each feature
            var mins = new double[featureCount];
            var maxs = new double[featureCount];

            for (int i = 0; i < featureCount; i++)
            {
                mins[i] = features.Min(f => f[i]);
                maxs[i] = features.Max(f => f[i]);
            }

            // Normalize each feature vector
            foreach (var feature in features)
            {
                var normalizedFeature = new double[featureCount];
                for (int i = 0; i < featureCount; i++)
                {
                    var range = maxs[i] - mins[i];
                    normalizedFeature[i] = range > 0 ? (feature[i] - mins[i]) / range : 0;
                }
                normalized.Add(normalizedFeature);
            }

            return normalized;
        }

        private string[] GetFeatureNames()
        {
            return new[]
            {
                "TotalQuestions", "CompliantQuestions", "ComplianceScore", "RiskScore",
                "OrgSize_Tiny", "OrgSize_Small", "OrgSize_Medium", "OrgSize_Large", "OrgSize_VeryLarge",
                "AssetValue_VeryLow", "AssetValue_Low", "AssetValue_Medium", "AssetValue_High", "AssetValue_VeryHigh",
                "Sector_1", "Sector_2", "Sector_3", "Sector_4", "Sector_5", "Sector_6", "Sector_7", "Sector_8", "Sector_9", "Sector_10",
                "Industry_1", "Industry_2", "Industry_3", "Industry_4", "Industry_5", "Industry_6", "Industry_7", "Industry_8", "Industry_9", "Industry_10",
                "Industry_11", "Industry_12", "Industry_13", "Industry_14", "Industry_15", "Industry_16", "Industry_17", "Industry_18", "Industry_19", "Industry_20",
                "CategoryScore_1", "CategoryScore_2", "CategoryScore_3", "CategoryScore_4", "CategoryScore_5",
                "CategoryScore_6", "CategoryScore_7", "CategoryScore_8", "CategoryScore_9", "CategoryScore_10",
                "TemporalFeature"
            };
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            if (!values.Any()) return 0;

            var mean = values.Average();
            var sumSquaredDiff = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumSquaredDiff / values.Count);
        }

        #endregion
    }
} 