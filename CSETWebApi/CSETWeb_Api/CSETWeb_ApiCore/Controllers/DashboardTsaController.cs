//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.DataLayer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSETWebCore.Business.Dashboard;
//using CSETWebCore.Interfaces.Dashboard;
using CSETWebCore.Model.Dashboard;
using System.Threading.Tasks;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Analysis;
using Snickler.EFCore;
using CSETWebCore.Model.Aggregation;
using CSETWebCore.Model.Question;
using DataRowsAnalytics = CSETWebCore.Model.Dashboard.DataRowsAnalytics;
using CSETWebCore.Interfaces.Analytics;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Model.Assessment;

//using CSETWebCore.Interfaces.Dashboard;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for TSA (Transportation Security Administration) dashboard operations in CSET.
    /// This controller handles TSA-specific analytics, maturity dashboard data, standards analysis,
    /// and demographic updates for transportation security assessments. Supports sector and industry
    /// filtering for comparative analysis.
    /// </summary>
    [ApiController]
    public class DashboardTsaController : ControllerBase
    {
        private readonly IConfiguration config;
        private DashboardBusiness _dashboardBusiness;
        private readonly CSETContext _context;
        private readonly ITokenManager _tokenManager;
        private readonly IAnalyticsBusiness _analytics;
        private readonly IDemographicBusiness _demographic;

        /// <summary>
        /// Initializes a new instance of the DashboardTsaController.
        /// </summary>
        /// <param name="config">The configuration service for application settings</param>
        /// <param name="context">The database context for data access operations</param>
        /// <param name="tokenManager">The token manager for user authentication and assessment context</param>
        /// <param name="analytics">The analytics business service for dashboard data generation</param>
        /// <param name="demographic">The demographic business service for assessment demographics</param>
        public DashboardTsaController(IConfiguration config, CSETContext context, ITokenManager tokenManager, IAnalyticsBusiness analytics, IDemographicBusiness demographic)
        {
            this.config = config;
            _context = context;
            _dashboardBusiness = new DashboardBusiness(_context);
            _tokenManager = tokenManager;
            _analytics = analytics;
            _demographic = demographic;
        }

        // [HttpGet]
        // [Route("api/TSA/getSectors")]
        // public async Task<IActionResult> GetSectors()
        // {
        //     var sectors = await _dashboardBusiness.GetSectors();
        //     var flattenSectors = sectors.Select(x => new TreeView
        //     {
        //         Name = x.SectorName,
        //         Children = x.Industries?.Select(y => new TreeView { Name = y }).ToList()
        //     }).ToList();
        //     flattenSectors.Insert(0, new TreeView { Name = "All Sectors", Children = null });
        //     return Ok(flattenSectors);
        //
        // }

        /// <summary>
        /// Retrieves TSA maturity dashboard data with comparative analytics across sectors and industries.
        /// </summary>
        /// <param name="maturity_model_id">The ID of the maturity model to analyze</param>
        /// <param name="sectorId">Optional sector ID to filter results by sector</param>
        /// <param name="industryId">Optional industry ID to filter results by industry</param>
        /// <returns>
        /// 200 OK with ChartDataTSA containing maturity dashboard data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides TSA-specific maturity analytics including:
        /// - Maturity level distribution and percentages
        /// - Comparative analysis across sectors and industries
        /// - Question group heading organization
        /// - Average performance metrics
        /// 
        /// The response includes:
        /// - Maturity data rows with statistical analysis
        /// - Percentage distributions by maturity level
        /// - Question group headings for categorization
        /// - Average scores for benchmarking
        /// 
        /// TSA-specific features:
        /// - Transportation security focus
        /// - Sector-specific maturity analysis
        /// - Industry benchmarking capabilities
        /// - Comparative performance metrics
        /// 
        /// The dashboard data supports:
        /// - TSA compliance reporting
        /// - Transportation security assessments
        /// - Sector-specific analysis
        /// - Performance benchmarking
        /// - Trend identification and analysis
        /// 
        /// Filtering options:
        /// - By maturity model for specific framework analysis
        /// - By sector for transportation industry comparisons
        /// - By industry for detailed competitive analysis
        /// - Combined filters for targeted insights
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/TSA/analyticsMaturityDashboard")]
        [ProducesResponseType(typeof(ChartDataTSA), 200)]
        [ProducesResponseType(401)]
        public IActionResult analyticsMaturityDashboard(int maturity_model_id, int? sectorId, int? industryId)
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            ChartDataTSA chartData = new ChartDataTSA();

            var data = _analytics.getMaturityDashboardData(maturity_model_id, sectorId, industryId);
            var percentage = _analytics
                .GetMaturityGroupsForAssessment(assessmentId, maturity_model_id).ToList();
            chartData.DataRowsMaturity = data;
            chartData.data = (from a in percentage
                              select (double)a.Percentage).ToList();

            chartData.Labels = (from an in data
                                orderby an.Question_Group_Heading
                                select an.Question_Group_Heading).Distinct().ToList();
            foreach (var item in data)
            {
                chartData.data.Add(item.average);
            }

            return Ok(chartData);
        }

        /// <summary>
        /// Retrieves the list of standards applicable to the current TSA assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with list of applicable standards
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint returns the standards list for TSA assessments:
        /// - Applicable security standards and frameworks
        /// - Standard names and identifiers
        /// - Assessment-specific standard selection
        /// - TSA compliance requirements
        /// 
        /// The response includes:
        /// - Standard names and short names
        /// - Standard identifiers and codes
        /// - Applicability indicators
        /// - Assessment context information
        /// 
        /// TSA standards typically include:
        /// - TSA Pipeline Security Guidelines
        /// - Transportation security frameworks
        /// - Industry-specific standards
        /// - Regulatory compliance requirements
        /// 
        /// The standards list supports:
        /// - Assessment configuration
        /// - Compliance tracking
        /// - Standard selection and management
        /// - Reporting and analysis
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/TSA/getStandardList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult getStandardList()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var standardList = _analytics.GetStandardList(assessmentId);
            return Ok(standardList);
        }

        /// <summary>
        /// Retrieves standards results by category with sector and industry filtering for TSA assessments.
        /// </summary>
        /// <param name="sectorId">Optional sector ID to filter results by sector</param>
        /// <param name="industryId">Optional industry ID to filter results by industry</param>
        /// <returns>
        /// 200 OK with array of ChartDataTSA containing standards results by category
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides comprehensive standards analysis for TSA assessments:
        /// - Standards results organized by category
        /// - Sector and industry comparative analysis
        /// - Min, max, average calculations per standard
        /// - Question group heading organization
        /// 
        /// The response includes:
        /// - Chart data for each applicable standard
        /// - Statistical analysis (min, max, average)
        /// - Question group headings for categorization
        /// - Standard-specific performance metrics
        /// 
        /// Analysis features:
        /// - Cross-standard comparison
        /// - Sector-specific benchmarking
        /// - Industry performance analysis
        /// - Category-based organization
        /// 
        /// The standards analysis supports:
        /// - TSA compliance assessment
        /// - Transportation security analysis
        /// - Sector-specific reporting
        /// - Performance benchmarking
        /// - Gap identification and prioritization
        /// 
        /// Filtering options:
        /// - By sector for transportation industry focus
        /// - By industry for specific sector analysis
        /// - Combined filters for targeted insights
        /// - All sectors for comprehensive analysis
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/TSA/getSectorIndustryStandardsTSA")]
        [ProducesResponseType(typeof(ChartDataTSA[]), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetStandardsResultsByCategory1(int? sectorId, int? industryId)
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var standardList = _analytics.GetStandardList(assessmentId);
            // var standardMinMaxAvg = _analytics.GetStandardMinMaxAvg(assessmentId,"TSA2018", sectorId=null, industryId=null);
            ChartDataTSA[] chartDatas = new ChartDataTSA[standardList.Count()];
            int i = 0;
            foreach (var setname in standardList)
            {
                ChartDataTSA chartData = new ChartDataTSA();

                var standardMinMaxAvg = _analytics.GetStandardMinMaxAvg(assessmentId, setname.Set_Name, sectorId, industryId);
                var standardsingleaverage = _analytics.GetStandardSingleAvg(assessmentId, setname.Set_Name);

                chartData.data = (from a in standardsingleaverage
                                  select a.average).ToList();

                chartData.DataRowsStandard = standardMinMaxAvg;
                chartData.StandardList = standardList;
                chartData.label = setname.Short_Name;
                foreach (var c in standardMinMaxAvg)
                {
                    chartData.Labels.Add(c.QUESTION_GROUP_HEADING);
                }

                chartDatas[i++] = chartData;
            }
            return Ok(chartDatas);
        }

        /// <summary>
        /// Updates assessment demographics and refreshes chart data for TSA assessments.
        /// </summary>
        /// <param name="demographics">The demographics data to update</param>
        /// <returns>
        /// 200 OK with updated demographics result
        /// 400 Bad Request if demographics data is invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint updates assessment demographics and triggers chart refresh:
        /// - Saves demographic information to the assessment
        /// - Updates assessment context and metadata
        /// - Triggers chart data recalculation
        /// - Maintains assessment state consistency
        /// 
        /// The update process includes:
        /// - Demographic data validation
        /// - Assessment context updates
        /// - Chart data refresh triggers
        /// - State consistency maintenance
        /// 
        /// TSA-specific demographics may include:
        /// - Transportation sector information
        /// - Facility type and characteristics
        /// - Geographic location data
        /// - Operational context details
        /// 
        /// The update supports:
        /// - Assessment configuration changes
        /// - Demographic data management
        /// - Chart refresh and recalculation
        /// - State consistency maintenance
        /// - Real-time dashboard updates
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/TSA/updateChart")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult UpdateChart([FromBody] Demographics demographics)
        {
            demographics.AssessmentId = _tokenManager.AssessmentForUser();
            return Ok(_demographic.SaveDemographics(demographics));
        }
    }
}

