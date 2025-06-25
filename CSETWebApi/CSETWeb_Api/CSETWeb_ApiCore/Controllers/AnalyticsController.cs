//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.Extensions.Configuration;
using CSETWebCore.Business.Authorization;
using CSETWebCore.Interfaces.Assessment;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Model.Analytics;
using CSETWebCore.Model.Assessment;
using CSETWebCore.Model.Question;
using CSETWebCore.Business.Question;
using CSETWebCore.Interfaces.Analytics;


namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for analytics functionality in CSET.
    /// This controller handles the generation of analytics data for assessments,
    /// including demographic analysis, maturity model comparisons, and statistical reporting.
    /// Supports both individual assessment analytics and aggregated analysis across multiple assessments.
    /// Requires authentication and authorization via CsetAuthorize attribute.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IRequirementBusiness _requirement;
        private readonly IAssessmentBusiness _assessment;
        private readonly ITokenManager _token;
        private readonly IDemographicBusiness _demographic;
        private readonly IQuestionRequirementManager _questionRequirement;
        private readonly IQuestionBusiness _question;
        private readonly IAnalyticsBusiness _analytics;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the AnalyticsController.
        /// </summary>
        /// <param name="requirement">Requirement business logic service</param>
        /// <param name="assessment">Assessment business logic service</param>
        /// <param name="token">Token manager for authentication and authorization</param>
        /// <param name="demographic">Demographic business logic service</param>
        /// <param name="questionRequirement">Question requirement manager service</param>
        /// <param name="question">Question business logic service</param>
        /// <param name="analytics">Analytics business logic service</param>
        /// <param name="configuration">Configuration service for application settings</param>
        public AnalyticsController(IRequirementBusiness requirement, IAssessmentBusiness assessment,
            ITokenManager token, IDemographicBusiness demographic,
            IQuestionRequirementManager questionRequirement,
            IQuestionBusiness question, IAnalyticsBusiness analytics,
            IConfiguration configuration)
        {
            _requirement = requirement;
            _assessment = assessment;
            _token = token;
            _demographic = demographic;
            _questionRequirement = questionRequirement;
            _question = question;
            _analytics = analytics;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves comprehensive analytics information for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with analytics data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns a complete analytics package including assessment details, demographics,
        /// and question/answer data. The demographics include asset value, size, industry,
        /// and sector information. Question answers are ordered by question ID for consistency.
        /// This endpoint provides the foundation data for analytics dashboards and reporting.
        /// </remarks>
        [HttpGet]
        [Route("api/analytics/getAnalytics")]
        public IActionResult GetAnalytics()
        {
            var demographics = GetDemographics();
            var assessment = GetAnalyticsAssessment();
            assessment.Assets = demographics.AssetValue;
            assessment.Size = demographics.Size;
            assessment.IndustryId = demographics.IndustryId;
            assessment.SectorId = demographics.SectorId;

            return Ok(new Analytics
            {
                Assessment = assessment,
                Demographics = demographics,
                QuestionAnswers = GetQuestionsAnswers()
            });
        }

        /// <summary>
        /// Retrieves aggregation analytics for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with aggregation data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns aggregation analytics data for the current assessment.
        /// This includes comparative analysis against other assessments in the system.
        /// Useful for understanding how the current assessment performs relative to others.
        /// </remarks>
        [HttpGet]
        [Route("api/analytics/getAggregation")]
        public IActionResult GetAggregation()
        {
            int assessmentId = _token.AssessmentForUser();
            var agg = _analytics.GetAggregationAssessment(assessmentId);

            return Ok(agg);
        }

        /// <summary>
        /// Retrieves maturity model analytics with comparative data.
        /// </summary>
        /// <param name="modelId">ID of the maturity model to analyze</param>
        /// <param name="sectorId">Optional sector ID for filtering comparisons</param>
        /// <param name="industryId">Optional industry ID for filtering comparisons</param>
        /// <returns>
        /// 200 OK with maturity analytics data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Generates comprehensive maturity model analytics including comparative data.
        /// Uses stored procedures to compute maturity groupings, averages, and sample sizes.
        /// Returns data structured for chart visualization with categories containing
        /// minimum, maximum, average, median, and current assessment scores.
        /// The sample size indicates the number of assessments used for comparison.
        /// </remarks>
        [HttpGet]
        [Route("api/analytics/maturity/bars")]
        public IActionResult GetAnalyticsNew(int modelId, int? sectorId, int? industryId)
        {
            int assessmentId = _token.AssessmentForUser();
            string connectionString = _configuration.GetConnectionString("CSET_DB") ?? "";

            var dtPool = new DataTable();
            var dtTargetAssessment = new DataTable();
            var SampleSize = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("analytics_setup_maturity_groupings", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.ExecuteNonQuery();
                }
                using (SqlCommand command = new SqlCommand("FillAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    // Add input parameter
                    command.Parameters.Add(new SqlParameter("@assessment_id", assessmentId));
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("analytics_Compute_MaturityAll", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameter
                    command.Parameters.Add(new SqlParameter("@maturity_model_id", modelId));
                    command.Parameters.Add(new SqlParameter("@sector_id", sectorId));
                    command.Parameters.Add(new SqlParameter("@industry_id", industryId));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dtPool.Load(reader);
                    }
                }

                using (SqlCommand command = new SqlCommand("analytics_compute_single_averages_maturity", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameter
                    command.Parameters.Add(new SqlParameter("@assessment_id", assessmentId));
                    command.Parameters.Add(new SqlParameter("@maturity_model_id", modelId));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dtTargetAssessment.Load(reader);
                    }
                }

                using (SqlCommand command = new SqlCommand("analytics_Compute_MaturitySampleSize", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameter
                    command.Parameters.Add(new SqlParameter("@maturity_model_id", modelId));
                    command.Parameters.Add(new SqlParameter("@sector_id", sectorId));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        SampleSize.Load(reader);
                    }
                }
            }

            var response = new NewResponse();

            foreach (DataRow row in dtPool.Rows)
            {
                var cat = new Category();

                response.Categories.Add(cat);

                cat.Label = row["Question_Group_Heading"].ToString() ?? "[unknown]";

                cat.Min = (double)row["minimum"];
                cat.Max = (double)row["maximum"];
                cat.Avg = (double)row["average"];
                cat.Median = (int)row["median"];
            }

            foreach (DataRow row in dtTargetAssessment.Rows)
            {
                var r = response.Categories.FirstOrDefault(x => x.Label == row["title"].ToString());
                if (r != null)
                {
                    r.MyScore = (int)row["Percentage"];
                }
            }

            int total_count = 0;
            foreach (DataRow row in SampleSize.Rows)
            {
                if (sectorId == null)
                {
                    if (row["SectorId"].ToString() == "")
                    {
                        total_count += Convert.ToInt32(row["AssessmentCount"]);
                        break;
                    }
                }

                else if (sectorId == Convert.ToInt32(row["SectorId"]))
                {
                    total_count += Convert.ToInt32(row["AssessmentCount"]);
                    break;
                }
                else
                {
                    total_count += Convert.ToInt32(row["AssessmentCount"]);
                    break;
                }
            }
            response.SampleSize = total_count;

            return Ok(response);
        }

        /// <summary>
        /// Retrieves analytics assessment details for the current assessment.
        /// </summary>
        /// <returns>AnalyticsAssessment object containing assessment details</returns>
        /// <remarks>
        /// Private helper method that retrieves detailed analytics information
        /// for the current assessment based on the user's token.
        /// </remarks>
        private AnalyticsAssessment GetAnalyticsAssessment()
        {
            int assessmentId = _token.AssessmentForUser();
            var assessment = _assessment.GetAnalyticsAssessmentDetail(assessmentId);
            return assessment;
        }

        /// <summary>
        /// Returns an instance of Demographics for Anonymous export.
        /// </summary>
        /// <returns>AnalyticsDemographic object containing demographic information</returns>
        /// <remarks>
        /// Private helper method that retrieves anonymous demographic data
        /// for the current assessment. This data is used for analytics and reporting
        /// without exposing personally identifiable information.
        /// </remarks>
        private AnalyticsDemographic GetDemographics()
        {
            int assessmentId = _token.AssessmentForUser();
            return _demographic.GetAnonymousDemographics(assessmentId);
        }

        /// <summary>
        /// Returns questions/answers for current selected assessment.
        /// </summary>
        /// <returns>List of AnalyticsQuestionAnswer objects</returns>
        /// <remarks>
        /// Private helper method that retrieves question and answer data
        /// for the current assessment. Handles both question-based and requirement-based
        /// assessment modes. Returns data ordered by question ID for consistency.
        /// </remarks>
        private List<AnalyticsQuestionAnswer> GetQuestionsAnswers()
        {
            int assessmentId = _token.AssessmentForUser();
            string applicationMode = _questionRequirement.GetApplicationMode(assessmentId);

            if (applicationMode.ToLower().StartsWith("questions"))
            {
                _question.SetQuestionAssessmentId(assessmentId);
                QuestionResponse resp = _question.GetQuestionListWithSet("*");
                return _question.GetAnalyticQuestionAnswers(resp).OrderBy(x => x.QuestionId).ToList();
            }
            else
            {
                _requirement.SetRequirementAssessmentId(assessmentId);
                QuestionResponse resp = _requirement.GetRequirementsList();
                return _question.GetAnalyticQuestionAnswers(resp).OrderBy(x => x.QuestionId).ToList();
            }
        }
    }

    /// <summary>
    /// Response model for maturity analytics data.
    /// </summary>
    public class NewResponse
    {
        /// <summary>
        /// List of categories with their statistical data.
        /// </summary>
        public List<Category> Categories { get; set; } = [];

        /// <summary>
        /// Total number of assessments used for comparison.
        /// </summary>
        public int SampleSize { get; set; } = 0;
    }

    /// <summary>
    /// Represents a category in maturity analytics with statistical measures.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Label/name of the category.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Minimum score across all assessments for this category.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// Maximum score across all assessments for this category.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// Median score across all assessments for this category.
        /// </summary>
        public double Median { get; set; }

        /// <summary>
        /// Average score across all assessments for this category.
        /// </summary>
        public double Avg { get; set; }

        /// <summary>
        /// Current assessment's score for this category.
        /// </summary>
        public double MyScore { get; set; }
    }

    /// <summary>
    /// Response model for analytics data with bar chart information.
    /// </summary>
    public class AnalyticsResponse
    {
        /// <summary>
        /// List of minimum values for each category.
        /// </summary>
        public List<double> Min { get; set; } = [];

        /// <summary>
        /// List of maximum values for each category.
        /// </summary>
        public List<double> Max { get; set; } = [];

        /// <summary>
        /// List of median values for each category.
        /// </summary>
        public List<int> Median { get; set; } = [];

        /// <summary>
        /// List of average values for each category.
        /// </summary>
        public List<double> Average { get; set; } = [];

        /// <summary>
        /// Bar chart data containing values and labels.
        /// </summary>
        public BarItem BarData { get; set; } = new BarItem();

        /// <summary>
        /// Total number of assessments used for comparison.
        /// </summary>
        public int SampleSize { get; set; } = 0;
    }

    /// <summary>
    /// Represents bar chart data with values and labels.
    /// </summary>
    public class BarItem
    {
        /// <summary>
        /// List of values for the bar chart.
        /// </summary>
        public List<double> Values { get; set; } = [];

        /// <summary>
        /// List of labels for the bar chart.
        /// </summary>
        public List<string> Labels { get; set; } = [];
    }
}
