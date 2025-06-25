//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Reports;
using CSETWebCore.Interfaces.Assessment;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Reports;
using CSETWebCore.Model.Reports;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSETWebCore.Business.Authorization;

namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for CMMC (Cybersecurity Maturity Model Certification) report generation in CSET.
    /// This controller handles CMMC-specific report data retrieval and formatting for Department of Defense
    /// contractors and suppliers. Supports CMMC compliance assessment reporting and maturity model analysis.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class ReportsCmmcController : ControllerBase
    {
        private readonly ITokenManager _token;
        private readonly IAssessmentBusiness _assessment;
        private readonly IDemographicBusiness _demographic;
        private readonly IReportsDataBusiness _report;

        /// <summary>
        /// Initializes a new instance of the ReportsCmmcController.
        /// </summary>
        /// <param name="token">The token manager for user authentication and assessment context</param>
        /// <param name="assessment">The assessment business service for assessment operations</param>
        /// <param name="demographic">The demographic business service for demographic data</param>
        /// <param name="report">The reports data business service for report generation</param>
        public ReportsCmmcController(ITokenManager token, IAssessmentBusiness assessment, IDemographicBusiness demographic, IReportsDataBusiness report)
        {
            _token = token;
            _assessment = assessment;
            _demographic = demographic;
            _report = report;
        }

        /// <summary>
        /// Retrieves CMMC maturity model data for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with ReportVM containing CMMC maturity model data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates comprehensive CMMC report data including:
        /// - Assessment details and demographics
        /// - Maturity model questions and answers
        /// - Deficiency analysis and gap identification
        /// - Comments and review markers
        /// - Alternative practices and recommendations
        /// 
        /// The response includes:
        /// - Assessment information and metadata
        /// - Questions list with answer status
        /// - Deficiencies list highlighting compliance gaps
        /// - Comments and marked items for review
        /// - Alternative practices for improvement
        /// 
        /// CMMC-specific features:
        /// - Maturity level assessment (ML1-ML3)
        /// - Practice implementation status
        /// - Gap analysis for certification requirements
        /// - Compliance scoring and recommendations
        /// 
        /// The data is processed to include missing parent questions
        /// for complete hierarchical analysis and reporting.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reportscmmc/maturitymodel")]
        [ProducesResponseType(typeof(ReportVM), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetMaturityModel()
        {
            int assessmentId = _token.AssessmentForUser();
            _report.SetReportsAssessmentId(assessmentId);

            var detail = _assessment.GetAssessmentDetail(assessmentId);


            var reportData = new MaturityBasicReportData()
            {
                Information = _report.GetInformation(),
                QuestionsList = _report.GetQuestionsList(),
                DeficienciesList = _report.GetMaturityDeficiencies(),
                Comments = _report.GetCommentsList(),
                MarkedForReviewList = _report.GetMarkedForReviewList(),
                AlternateList = _report.GetAlternatesList()
            };

            reportData.DeficienciesList = reportData.AddMissingParentsTo(reportData.DeficienciesList);
            reportData.Comments = reportData.AddMissingParentsTo(reportData.Comments);
            reportData.MarkedForReviewList = reportData.AddMissingParentsTo(reportData.MarkedForReviewList);
            reportData.AlternateList = reportData.AddMissingParentsTo(reportData.AlternateList);

            var viewModel = new ReportVM(detail, reportData);

            return Ok(viewModel);
        }
    }
}
