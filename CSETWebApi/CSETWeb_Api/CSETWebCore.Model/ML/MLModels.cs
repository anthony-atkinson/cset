//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;

namespace CSETWebCore.Model.ML
{
    /// <summary>
    /// Assessment data point for ML training
    /// </summary>
    public class AssessmentDataPoint
    {
        /// <summary>
        /// Assessment ID
        /// </summary>
        public int AssessmentId { get; set; }

        /// <summary>
        /// Assessment date
        /// </summary>
        public DateTime AssessmentDate { get; set; }

        /// <summary>
        /// Organization size
        /// </summary>
        public string OrganizationSize { get; set; } = string.Empty;

        /// <summary>
        /// Asset value
        /// </summary>
        public string AssetValue { get; set; } = string.Empty;

        /// <summary>
        /// Sector ID
        /// </summary>
        public int SectorId { get; set; }

        /// <summary>
        /// Industry ID
        /// </summary>
        public int IndustryId { get; set; }

        /// <summary>
        /// Total number of questions
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// Number of compliant questions
        /// </summary>
        public int CompliantQuestions { get; set; }

        /// <summary>
        /// Compliance score (0-100)
        /// </summary>
        public double ComplianceScore { get; set; }

        /// <summary>
        /// Risk score (0-100)
        /// </summary>
        public double RiskScore { get; set; }

        /// <summary>
        /// Category scores by question category
        /// </summary>
        public Dictionary<string, double> CategoryScores { get; set; } = new Dictionary<string, double>();
    }

    /// <summary>
    /// Preprocessed data ready for ML training
    /// </summary>
    public class PreprocessedData
    {
        /// <summary>
        /// Feature vectors
        /// </summary>
        public List<double[]> Features { get; set; } = new List<double[]>();

        /// <summary>
        /// Labels for supervised learning
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();

        /// <summary>
        /// Feature names for interpretability
        /// </summary>
        public string[] FeatureNames { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// ML pipeline configuration
    /// </summary>
    public class MLPipelineConfiguration
    {
        /// <summary>
        /// Data retention period in days
        /// </summary>
        public int DataRetentionDays { get; set; } = 365;

        /// <summary>
        /// Batch size for processing
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Whether to normalize features
        /// </summary>
        public bool FeatureNormalization { get; set; } = true;

        /// <summary>
        /// Whether to enable data validation
        /// </summary>
        public bool DataValidationEnabled { get; set; } = true;

        /// <summary>
        /// Maximum number of data points to process
        /// </summary>
        public int MaxDataPoints { get; set; } = 10000;
    }

    /// <summary>
    /// Data validation result
    /// </summary>
    public class DataValidationResult
    {
        /// <summary>
        /// Whether the data is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Validation issues found
        /// </summary>
        public List<string> Issues { get; set; } = new List<string>();

        /// <summary>
        /// Data statistics
        /// </summary>
        public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// ML model metadata
    /// </summary>
    public class MLModelMetadata
    {
        /// <summary>
        /// Model ID
        /// </summary>
        public string ModelId { get; set; } = string.Empty;

        /// <summary>
        /// Model name
        /// </summary>
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// Model version
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Model type (Classification, Regression, etc.)
        /// </summary>
        public string ModelType { get; set; } = string.Empty;

        /// <summary>
        /// Algorithm used
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Training date
        /// </summary>
        public DateTime TrainedAt { get; set; }

        /// <summary>
        /// Model accuracy
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// Model precision
        /// </summary>
        public double Precision { get; set; }

        /// <summary>
        /// Model recall
        /// </summary>
        public double Recall { get; set; }

        /// <summary>
        /// Model F1 score
        /// </summary>
        public double F1Score { get; set; }

        /// <summary>
        /// Number of training samples
        /// </summary>
        public int TrainingSamples { get; set; }

        /// <summary>
        /// Feature names
        /// </summary>
        public string[] FeatureNames { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Model parameters
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Whether the model is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Model file path
        /// </summary>
        public string ModelFilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// ML prediction result
    /// </summary>
    public class MLPredictionResult
    {
        /// <summary>
        /// Prediction ID
        /// </summary>
        public string PredictionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Model ID used for prediction
        /// </summary>
        public string ModelId { get; set; } = string.Empty;

        /// <summary>
        /// Predicted class/label
        /// </summary>
        public string PredictedLabel { get; set; } = string.Empty;

        /// <summary>
        /// Prediction confidence (0-1)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Prediction probabilities for each class
        /// </summary>
        public Dictionary<string, double> ClassProbabilities { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Feature importance scores
        /// </summary>
        public Dictionary<string, double> FeatureImportance { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Prediction timestamp
        /// </summary>
        public DateTime PredictedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Input features used
        /// </summary>
        public double[] InputFeatures { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// ML recommendation
    /// </summary>
    public class MLRecommendation
    {
        /// <summary>
        /// Recommendation ID
        /// </summary>
        public string RecommendationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Recommendation title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Recommendation description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Recommendation category
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Priority level (Low, Medium, High, Critical)
        /// </summary>
        public string Priority { get; set; } = string.Empty;

        /// <summary>
        /// Expected impact
        /// </summary>
        public string Impact { get; set; } = string.Empty;

        /// <summary>
        /// Estimated effort
        /// </summary>
        public string Effort { get; set; } = string.Empty;

        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Related question numbers
        /// </summary>
        public List<string> RelatedQuestions { get; set; } = new List<string>();

        /// <summary>
        /// Supporting evidence
        /// </summary>
        public List<string> Evidence { get; set; } = new List<string>();

        /// <summary>
        /// Recommendation timestamp
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// ML model training request
    /// </summary>
    public class ModelTrainingRequest
    {
        /// <summary>
        /// Model name
        /// </summary>
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// Model type
        /// </summary>
        public string ModelType { get; set; } = string.Empty;

        /// <summary>
        /// Algorithm to use
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Training data start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Training data end date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Model parameters
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Whether to validate the model
        /// </summary>
        public bool ValidateModel { get; set; } = true;

        /// <summary>
        /// Whether to deploy the model after training
        /// </summary>
        public bool DeployAfterTraining { get; set; } = false;
    }

    /// <summary>
    /// ML model training result
    /// </summary>
    public class ModelTrainingResult
    {
        /// <summary>
        /// Training job ID
        /// </summary>
        public string TrainingJobId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Model ID
        /// </summary>
        public string ModelId { get; set; } = string.Empty;

        /// <summary>
        /// Training status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Training start time
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Training end time
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Training duration
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Model performance metrics
        /// </summary>
        public Dictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Training error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Number of training samples
        /// </summary>
        public int TrainingSamples { get; set; }

        /// <summary>
        /// Number of validation samples
        /// </summary>
        public int ValidationSamples { get; set; }
    }

    /// <summary>
    /// A/B test configuration
    /// </summary>
    public class ABTestConfiguration
    {
        /// <summary>
        /// Test ID
        /// </summary>
        public string TestId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Test name
        /// </summary>
        public string TestName { get; set; } = string.Empty;

        /// <summary>
        /// Test description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Control model ID
        /// </summary>
        public string ControlModelId { get; set; } = string.Empty;

        /// <summary>
        /// Treatment model ID
        /// </summary>
        public string TreatmentModelId { get; set; } = string.Empty;

        /// <summary>
        /// Traffic split percentage for treatment (0-100)
        /// </summary>
        public double TrafficSplitPercentage { get; set; } = 50.0;

        /// <summary>
        /// Test start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Test end date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Success metrics
        /// </summary>
        public List<string> SuccessMetrics { get; set; } = new List<string>();

        /// <summary>
        /// Whether the test is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Minimum sample size for statistical significance
        /// </summary>
        public int MinimumSampleSize { get; set; } = 1000;
    }

    /// <summary>
    /// A/B test result
    /// </summary>
    public class ABTestResult
    {
        /// <summary>
        /// Test ID
        /// </summary>
        public string TestId { get; set; } = string.Empty;

        /// <summary>
        /// Control group metrics
        /// </summary>
        public Dictionary<string, double> ControlMetrics { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Treatment group metrics
        /// </summary>
        public Dictionary<string, double> TreatmentMetrics { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Statistical significance (p-value)
        /// </summary>
        public Dictionary<string, double> PValues { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Effect sizes
        /// </summary>
        public Dictionary<string, double> EffectSizes { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Winner determination
        /// </summary>
        public string Winner { get; set; } = string.Empty;

        /// <summary>
        /// Confidence level
        /// </summary>
        public double ConfidenceLevel { get; set; }

        /// <summary>
        /// Sample sizes
        /// </summary>
        public Dictionary<string, int> SampleSizes { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Test duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Recommendations
        /// </summary>
        public List<string> Recommendations { get; set; } = new List<string>();
    }
} 