//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Analytics;
using CSETWebCore.Model.Analytics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.Model.Dashboard;
using CSETWebCore.Model.Aggregation;

namespace CSETWebCore.Business.Analytics
{
    /// <summary>
    /// Advanced analytics business logic for comprehensive cybersecurity assessment analysis.
    /// Provides trend analysis, benchmarking, predictive analytics, and executive reporting.
    /// </summary>
    public class AdvancedAnalyticsBusiness : IAdvancedAnalyticsBusiness
    {
        private readonly CSETContext _context;

        public AdvancedAnalyticsBusiness(CSETContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets comprehensive executive summary analytics for the assessment.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <returns>Executive summary analytics data</returns>
        public async Task<ExecutiveSummaryAnalytics> GetExecutiveSummaryAsync(int assessmentId)
        {
            var summary = new ExecutiveSummaryAnalytics
            {
                AssessmentId = assessmentId,
                GeneratedDate = DateTime.UtcNow
            };

            // Get assessment details
            var assessment = await _context.ASSESSMENTS
                .Where(a => a.Assessment_Id == assessmentId)
                .FirstOrDefaultAsync();

            if (assessment != null)
            {
                summary.AssessmentName = assessment.Alias;
                summary.AssessmentDate = assessment.Assessment_Date;
                summary.LastModified = assessment.LastModifiedDate;
            }

            // Get demographics
            var demographics = await _context.DEMOGRAPHICS
                .Where(d => d.Assessment_Id == assessmentId)
                .FirstOrDefaultAsync();

            if (demographics != null)
            {
                summary.OrganizationSize = demographics.Size;
                summary.AssetValue = demographics.AssetValue;
                summary.SectorId = demographics.SectorId;
                summary.IndustryId = demographics.IndustryId;
            }

            // Calculate overall compliance score
            var answers = await _context.ANSWER
                .Where(a => a.Assessment_Id == assessmentId)
                .ToListAsync();

            if (answers.Any())
            {
                var yesAnswers = answers.Count(a => a.Answer_Text == "Y" || a.Answer_Text == "A");
                summary.OverallComplianceScore = (double)yesAnswers / answers.Count * 100;
                summary.TotalQuestions = answers.Count;
                summary.CompliantQuestions = yesAnswers;
            }

            // Get risk areas
            summary.RiskAreas = await GetRiskAreasAsync(assessmentId);

            // Get improvement recommendations
            summary.ImprovementRecommendations = await GetImprovementRecommendationsAsync(assessmentId);

            return summary;
        }

        /// <summary>
        /// Gets trend analysis data for the assessment over time.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="timeframe">Timeframe for analysis (days)</param>
        /// <returns>Trend analysis data</returns>
        public async Task<TrendAnalysisData> GetTrendAnalysisAsync(int assessmentId, int timeframe = 365)
        {
            var trendData = new TrendAnalysisData
            {
                AssessmentId = assessmentId,
                Timeframe = timeframe,
                GeneratedDate = DateTime.UtcNow
            };

            // Get historical assessment data
            var historicalData = await _context.ASSESSMENTS
                .Where(a => a.Assessment_Id == assessmentId)
                .OrderByDescending(a => a.Assessment_Date)
                .Take(10) // Last 10 assessments
                .ToListAsync();

            var trendPoints = new List<TrendPoint>();

            foreach (var assessment in historicalData)
            {
                var answers = await _context.ANSWER
                    .Where(a => a.Assessment_Id == assessment.Assessment_Id)
                    .ToListAsync();

                if (answers.Any())
                {
                    var yesAnswers = answers.Count(a => a.Answer_Text == "Y" || a.Answer_Text == "A");
                    var complianceScore = (double)yesAnswers / answers.Count * 100;

                    trendPoints.Add(new TrendPoint
                    {
                        Date = assessment.Assessment_Date,
                        ComplianceScore = complianceScore,
                        TotalQuestions = answers.Count,
                        CompliantQuestions = yesAnswers
                    });
                }
            }

            trendData.TrendPoints = trendPoints.OrderBy(t => t.Date).ToList();

            // Calculate trend metrics
            if (trendData.TrendPoints.Count > 1)
            {
                var firstPoint = trendData.TrendPoints.First();
                var lastPoint = trendData.TrendPoints.Last();
                
                trendData.TrendDirection = lastPoint.ComplianceScore > firstPoint.ComplianceScore ? "Improving" : "Declining";
                trendData.TrendPercentage = ((lastPoint.ComplianceScore - firstPoint.ComplianceScore) / firstPoint.ComplianceScore) * 100;
            }

            return trendData;
        }

        /// <summary>
        /// Gets benchmarking data comparing the assessment to industry standards.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="sectorId">Sector ID for comparison</param>
        /// <param name="industryId">Industry ID for comparison</param>
        /// <returns>Benchmarking data</returns>
        public async Task<BenchmarkingData> GetBenchmarkingDataAsync(int assessmentId, int? sectorId = null, int? industryId = null)
        {
            var benchmarking = new BenchmarkingData
            {
                AssessmentId = assessmentId,
                SectorId = sectorId,
                IndustryId = industryId,
                GeneratedDate = DateTime.UtcNow
            };

            // Get current assessment score
            var currentAnswers = await _context.ANSWER
                .Where(a => a.Assessment_Id == assessmentId)
                .ToListAsync();

            if (currentAnswers.Any())
            {
                var yesAnswers = currentAnswers.Count(a => a.Answer_Text == "Y" || a.Answer_Text == "A");
                benchmarking.CurrentScore = (double)yesAnswers / currentAnswers.Count * 100;
            }

            // Get comparison data
            var comparisonQuery = _context.ASSESSMENTS.AsQueryable();

            if (sectorId.HasValue)
            {
                comparisonQuery = comparisonQuery.Join(
                    _context.DEMOGRAPHICS,
                    a => a.Assessment_Id,
                    d => d.Assessment_Id,
                    (a, d) => new { Assessment = a, Demographics = d })
                    .Where(x => x.Demographics.SectorId == sectorId)
                    .Select(x => x.Assessment);
            }

            if (industryId.HasValue)
            {
                comparisonQuery = comparisonQuery.Join(
                    _context.DEMOGRAPHICS,
                    a => a.Assessment_Id,
                    d => d.Assessment_Id,
                    (a, d) => new { Assessment = a, Demographics = d })
                    .Where(x => x.Demographics.IndustryId == industryId)
                    .Select(x => x.Assessment);
            }

            var comparisonAssessments = await comparisonQuery
                .Where(a => a.Assessment_Id != assessmentId)
                .Take(100) // Limit to 100 for performance
                .ToListAsync();

            var comparisonScores = new List<double>();

            foreach (var assessment in comparisonAssessments)
            {
                var answers = await _context.ANSWER
                    .Where(a => a.Assessment_Id == assessment.Assessment_Id)
                    .ToListAsync();

                if (answers.Any())
                {
                    var yesAnswers = answers.Count(a => a.Answer_Text == "Y" || a.Answer_Text == "A");
                    var score = (double)yesAnswers / answers.Count * 100;
                    comparisonScores.Add(score);
                }
            }

            if (comparisonScores.Any())
            {
                benchmarking.IndustryAverage = comparisonScores.Average();
                benchmarking.IndustryMedian = comparisonScores.OrderBy(s => s).ElementAt(comparisonScores.Count / 2);
                benchmarking.IndustryMin = comparisonScores.Min();
                benchmarking.IndustryMax = comparisonScores.Max();
                benchmarking.PercentileRank = CalculatePercentileRank(benchmarking.CurrentScore, comparisonScores);
            }

            return benchmarking;
        }

        /// <summary>
        /// Gets predictive analytics data for future assessment performance.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <returns>Predictive analytics data</returns>
        public async Task<PredictiveAnalyticsData> GetPredictiveAnalyticsAsync(int assessmentId)
        {
            var predictiveData = new PredictiveAnalyticsData
            {
                AssessmentId = assessmentId,
                GeneratedDate = DateTime.UtcNow
            };

            // Get historical trend data
            var trendData = await GetTrendAnalysisAsync(assessmentId);
            
            if (trendData.TrendPoints.Count > 1)
            {
                // Simple linear regression for prediction
                var xValues = trendData.TrendPoints.Select((p, i) => (double)i).ToArray();
                var yValues = trendData.TrendPoints.Select(p => p.ComplianceScore).ToArray();

                var (slope, intercept) = CalculateLinearRegression(xValues, yValues);

                // Predict next 3 months
                var nextIndex = xValues.Length;
                predictiveData.PredictedScore3Months = slope * (nextIndex + 3) + intercept;
                predictiveData.PredictedScore6Months = slope * (nextIndex + 6) + intercept;
                predictiveData.PredictedScore12Months = slope * (nextIndex + 12) + intercept;

                predictiveData.ConfidenceLevel = CalculateConfidenceLevel(xValues, yValues, slope, intercept);
                predictiveData.TrendStrength = Math.Abs(slope);
            }

            // Get risk predictions
            predictiveData.RiskPredictions = await GetRiskPredictionsAsync(assessmentId);

            return predictiveData;
        }

        /// <summary>
        /// Gets custom report data based on specified parameters.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="reportType">Type of report</param>
        /// <param name="parameters">Report parameters</param>
        /// <returns>Custom report data</returns>
        public async Task<CustomReportData> GetCustomReportAsync(int assessmentId, string reportType, Dictionary<string, object> parameters)
        {
            var reportData = new CustomReportData
            {
                AssessmentId = assessmentId,
                ReportType = reportType,
                Parameters = parameters,
                GeneratedDate = DateTime.UtcNow
            };

            switch (reportType.ToLower())
            {
                case "compliance":
                    reportData.Data = await GetComplianceReportDataAsync(assessmentId, parameters);
                    break;
                case "risk":
                    reportData.Data = await GetRiskReportDataAsync(assessmentId, parameters);
                    break;
                case "trend":
                    reportData.Data = await GetTrendReportDataAsync(assessmentId, parameters);
                    break;
                case "benchmark":
                    reportData.Data = await GetBenchmarkReportDataAsync(assessmentId, parameters);
                    break;
                default:
                    reportData.Data = await GetComprehensiveReportDataAsync(assessmentId, parameters);
                    break;
            }

            return reportData;
        }

        #region Private Helper Methods

        private async Task<List<RiskArea>> GetRiskAreasAsync(int assessmentId)
        {
            var riskAreas = new List<RiskArea>();

            // Get questions with "No" answers
            var noAnswers = await _context.ANSWER
                .Where(a => a.Assessment_Id == assessmentId && a.Answer_Text == "N")
                .ToListAsync();

            // Group by category and calculate risk scores
            var riskGroups = noAnswers
                .GroupBy(a => a.Question_Number?.Split('.')[0] ?? "Unknown")
                .Select(g => new RiskArea
                {
                    Category = g.Key,
                    RiskScore = g.Count() * 10, // Simple risk scoring
                    QuestionCount = g.Count(),
                    HighRiskQuestions = g.Take(5).Select(a => a.Question_Number).ToList()
                })
                .OrderByDescending(r => r.RiskScore)
                .Take(10)
                .ToList();

            return riskGroups;
        }

        private async Task<List<ImprovementRecommendation>> GetImprovementRecommendationsAsync(int assessmentId)
        {
            var recommendations = new List<ImprovementRecommendation>();

            // Get questions with "No" answers
            var noAnswers = await _context.ANSWER
                .Where(a => a.Assessment_Id == assessmentId && a.Answer_Text == "N")
                .Take(20) // Top 20 areas for improvement
                .ToListAsync();

            foreach (var answer in noAnswers)
            {
                recommendations.Add(new ImprovementRecommendation
                {
                    QuestionNumber = answer.Question_Number,
                    Priority = "High",
                    Impact = "Significant",
                    EstimatedEffort = "Medium",
                    Description = $"Implement controls for {answer.Question_Number}"
                });
            }

            return recommendations;
        }

        private double CalculatePercentileRank(double score, List<double> comparisonScores)
        {
            var sortedScores = comparisonScores.OrderBy(s => s).ToList();
            var rank = sortedScores.Count(s => s <= score);
            return (double)rank / sortedScores.Count * 100;
        }

        private (double slope, double intercept) CalculateLinearRegression(double[] x, double[] y)
        {
            var n = x.Length;
            var sumX = x.Sum();
            var sumY = y.Sum();
            var sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            var sumX2 = x.Sum(xi => xi * xi);

            var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            var intercept = (sumY - slope * sumX) / n;

            return (slope, intercept);
        }

        private double CalculateConfidenceLevel(double[] x, double[] y, double slope, double intercept)
        {
            var predicted = x.Select(xi => slope * xi + intercept).ToArray();
            var residuals = y.Zip(predicted, (actual, pred) => Math.Pow(actual - pred, 2)).Sum();
            var total = y.Select(yi => Math.Pow(yi - y.Average(), 2)).Sum();

            return Math.Max(0, 1 - (residuals / total));
        }

        private async Task<List<RiskPrediction>> GetRiskPredictionsAsync(int assessmentId)
        {
            var predictions = new List<RiskPrediction>();

            // Simple risk prediction based on current gaps
            var noAnswers = await _context.ANSWER
                .Where(a => a.Assessment_Id == assessmentId && a.Answer_Text == "N")
                .CountAsync();

            predictions.Add(new RiskPrediction
            {
                RiskType = "Compliance Risk",
                Probability = Math.Min(noAnswers * 5, 100), // Simple probability calculation
                Impact = "High",
                Timeframe = "3-6 months",
                MitigationStrategy = "Implement missing controls"
            });

            return predictions;
        }

        private async Task<object> GetComplianceReportDataAsync(int assessmentId, Dictionary<string, object> parameters)
        {
            var answers = await _context.ANSWER
                .Where(a => a.Assessment_Id == assessmentId)
                .ToListAsync();

            return new
            {
                TotalQuestions = answers.Count,
                CompliantQuestions = answers.Count(a => a.Answer_Text == "Y" || a.Answer_Text == "A"),
                NonCompliantQuestions = answers.Count(a => a.Answer_Text == "N"),
                ComplianceRate = (double)answers.Count(a => a.Answer_Text == "Y" || a.Answer_Text == "A") / answers.Count * 100
            };
        }

        private async Task<object> GetRiskReportDataAsync(int assessmentId, Dictionary<string, object> parameters)
        {
            return await GetRiskAreasAsync(assessmentId);
        }

        private async Task<object> GetTrendReportDataAsync(int assessmentId, Dictionary<string, object> parameters)
        {
            var timeframe = parameters.ContainsKey("timeframe") ? (int)parameters["timeframe"] : 365;
            return await GetTrendAnalysisAsync(assessmentId, timeframe);
        }

        private async Task<object> GetBenchmarkReportDataAsync(int assessmentId, Dictionary<string, object> parameters)
        {
            var sectorId = parameters.ContainsKey("sectorId") ? (int?)parameters["sectorId"] : null;
            var industryId = parameters.ContainsKey("industryId") ? (int?)parameters["industryId"] : null;
            return await GetBenchmarkingDataAsync(assessmentId, sectorId, industryId);
        }

        private async Task<object> GetComprehensiveReportDataAsync(int assessmentId, Dictionary<string, object> parameters)
        {
            return new
            {
                ExecutiveSummary = await GetExecutiveSummaryAsync(assessmentId),
                TrendAnalysis = await GetTrendAnalysisAsync(assessmentId),
                Benchmarking = await GetBenchmarkingDataAsync(assessmentId),
                PredictiveAnalytics = await GetPredictiveAnalyticsAsync(assessmentId)
            };
        }

        #endregion
    }
} 