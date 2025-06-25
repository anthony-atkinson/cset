//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;

namespace CSETWebCore.Model.Analytics
{
    /// <summary>
    /// Executive summary analytics data for high-level assessment overview.
    /// </summary>
    public class ExecutiveSummaryAnalytics
    {
        /// <summary>
        /// Assessment ID
        /// </summary>
        public int AssessmentId { get; set; }

        /// <summary>
        /// Assessment name/alias
        /// </summary>
        public string AssessmentName { get; set; }

        /// <summary>
        /// Assessment date
        /// </summary>
        public DateTime? AssessmentDate { get; set; }

        /// <summary>
        /// Last modified date
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// Organization size
        /// </summary>
        public string OrganizationSize { get; set; }

        /// <summary>
        /// Asset value
        /// </summary>
        public string AssetValue { get; set; }

        /// <summary>
        /// Sector ID
        /// </summary>
        public int? SectorId { get; set; }

        /// <summary>
        /// Industry ID
        /// </summary>
        public int? IndustryId { get; set; }

        /// <summary>
        /// Overall compliance score (percentage)
        /// </summary>
        public double OverallComplianceScore { get; set; }

        /// <summary>
        /// Total number of questions
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// Number of compliant questions
        /// </summary>
        public int CompliantQuestions { get; set; }

        /// <summary>
        /// Risk areas identified
        /// </summary>
        public List<RiskArea> RiskAreas { get; set; } = new List<RiskArea>();

        /// <summary>
        /// Improvement recommendations
        /// </summary>
        public List<ImprovementRecommendation> ImprovementRecommendations { get; set; } = new List<ImprovementRecommendation>();

        /// <summary>
        /// Date when analytics were generated
        /// </summary>
        public DateTime GeneratedDate { get; set; }
    }

    /// <summary>
    /// Trend analysis data showing assessment performance over time.
    /// </summary>
    public class TrendAnalysisData
    {
        /// <summary>
        /// Assessment ID
        /// </summary>
        public int AssessmentId { get; set; }

        /// <summary>
        /// Timeframe for analysis in days
        /// </summary>
        public int Timeframe { get; set; }

        /// <summary>
        /// Trend direction (Improving/Declining/Stable)
        /// </summary>
        public string TrendDirection { get; set; }

        /// <summary>
        /// Trend percentage change
        /// </summary>
        public double TrendPercentage { get; set; }

        /// <summary>
        /// Trend points over time
        /// </summary>
        public List<TrendPoint> TrendPoints { get; set; } = new List<TrendPoint>();

        /// <summary>
        /// Date when analytics were generated
        /// </summary>
        public DateTime GeneratedDate { get; set; }
    }

    /// <summary>
    /// Individual trend point in the analysis.
    /// </summary>
    public class TrendPoint
    {
        /// <summary>
        /// Date of the assessment
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Compliance score at this point
        /// </summary>
        public double ComplianceScore { get; set; }

        /// <summary>
        /// Total questions at this point
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// Compliant questions at this point
        /// </summary>
        public int CompliantQuestions { get; set; }
    }

    /// <summary>
    /// Benchmarking data comparing assessment to industry standards.
    /// </summary>
    public class BenchmarkingData
    {
        /// <summary>
        /// Assessment ID
        /// </summary>
        public int AssessmentId { get; set; }

        /// <summary>
        /// Sector ID for comparison
        /// </summary>
        public int? SectorId { get; set; }

        /// <summary>
        /// Industry ID for comparison
        /// </summary>
        public int? IndustryId { get; set; }

        /// <summary>
        /// Current assessment score
        /// </summary>
        public double CurrentScore { get; set; }

        /// <summary>
        /// Industry average score
        /// </summary>
        public double IndustryAverage { get; set; }

        /// <summary>
        /// Industry median score
        /// </summary>
        public double IndustryMedian { get; set; }

        /// <summary>
        /// Industry minimum score
        /// </summary>
        public double IndustryMin { get; set; }

        /// <summary>
        /// Industry maximum score
        /// </summary>
        public double IndustryMax { get; set; }

        /// <summary>
        /// Percentile rank of current score
        /// </summary>
        public double PercentileRank { get; set; }

        /// <summary>
        /// Date when analytics were generated
        /// </summary>
        public DateTime GeneratedDate { get; set; }
    }

    /// <summary>
    /// Predictive analytics data for future assessment performance.
    /// </summary>
    public class PredictiveAnalyticsData
    {
        /// <summary>
        /// Assessment ID
        /// </summary>
        public int AssessmentId { get; set; }

        /// <summary>
        /// Predicted score in 3 months
        /// </summary>
        public double PredictedScore3Months { get; set; }

        /// <summary>
        /// Predicted score in 6 months
        /// </summary>
        public double PredictedScore6Months { get; set; }

        /// <summary>
        /// Predicted score in 12 months
        /// </summary>
        public double PredictedScore12Months { get; set; }

        /// <summary>
        /// Confidence level of predictions (0-1)
        /// </summary>
        public double ConfidenceLevel { get; set; }

        /// <summary>
        /// Strength of the trend
        /// </summary>
        public double TrendStrength { get; set; }

        /// <summary>
        /// Risk predictions
        /// </summary>
        public List<RiskPrediction> RiskPredictions { get; set; } = new List<RiskPrediction>();

        /// <summary>
        /// Date when analytics were generated
        /// </summary>
        public DateTime GeneratedDate { get; set; }
    }

    /// <summary>
    /// Risk prediction for future assessment periods.
    /// </summary>
    public class RiskPrediction
    {
        /// <summary>
        /// Type of risk
        /// </summary>
        public string RiskType { get; set; }

        /// <summary>
        /// Probability of risk occurrence (0-100)
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Impact level (Low/Medium/High)
        /// </summary>
        public string Impact { get; set; }

        /// <summary>
        /// Timeframe for risk occurrence
        /// </summary>
        public string Timeframe { get; set; }

        /// <summary>
        /// Mitigation strategy
        /// </summary>
        public string MitigationStrategy { get; set; }
    }

    /// <summary>
    /// Risk area identified in the assessment.
    /// </summary>
    public class RiskArea
    {
        /// <summary>
        /// Category name
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Risk score (0-100)
        /// </summary>
        public double RiskScore { get; set; }

        /// <summary>
        /// Number of questions in this category
        /// </summary>
        public int QuestionCount { get; set; }

        /// <summary>
        /// High-risk questions in this category
        /// </summary>
        public List<string> HighRiskQuestions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Improvement recommendation for the assessment.
    /// </summary>
    public class ImprovementRecommendation
    {
        /// <summary>
        /// Question number
        /// </summary>
        public string QuestionNumber { get; set; }

        /// <summary>
        /// Priority level (Low/Medium/High)
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Impact level (Low/Medium/High/Significant)
        /// </summary>
        public string Impact { get; set; }

        /// <summary>
        /// Estimated effort (Low/Medium/High)
        /// </summary>
        public string EstimatedEffort { get; set; }

        /// <summary>
        /// Description of the recommendation
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Custom report data with flexible parameters.
    /// </summary>
    public class CustomReportData
    {
        /// <summary>
        /// Assessment ID
        /// </summary>
        public int AssessmentId { get; set; }

        /// <summary>
        /// Type of report
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// Report parameters
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Report data
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Date when report was generated
        /// </summary>
        public DateTime GeneratedDate { get; set; }
    }
} 