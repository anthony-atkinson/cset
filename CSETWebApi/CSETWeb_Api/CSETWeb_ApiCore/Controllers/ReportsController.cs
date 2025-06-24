//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Demographic;
using CSETWebCore.Business.GalleryParser;
using CSETWebCore.Business.Maturity;
using CSETWebCore.Business.Question;
using CSETWebCore.Business.Reports;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Aggregation;
using CSETWebCore.Interfaces.Document;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Interfaces.Reports;
using CSETWebCore.Model.Aggregation;
using CSETWebCore.Model.Assessment;
using CSETWebCore.Model.Demographic;
using CSETWebCore.Model.Reports;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for generating various types of cybersecurity assessment reports in CSET.
    /// Supports executive summaries, detailed reports, maturity model reports, and specialized reports
    /// for different frameworks and standards.
    /// </summary>
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly CSETContext _context;
        private readonly IReportsDataBusiness _report;
        private readonly ITokenManager _token;
        private readonly IAggregationBusiness _aggregation;
        private readonly IQuestionBusiness _question;
        private readonly IQuestionRequirementManager _questionRequirement;
        private readonly IAssessmentUtil _assessmentUtil;
        private readonly IAdminTabBusiness _adminTabBusiness;
        private readonly IGalleryEditor _galleryEditor;
        private TranslationOverlay _overlay;
        private readonly IDocumentBusiness _documentBusiness;

        /// <summary>
        /// Initializes a new instance of the ReportsController.
        /// </summary>
        /// <param name="context">Database context for data access</param>
        /// <param name="report">Service for report data operations</param>
        /// <param name="token">Service for JWT token management</param>
        /// <param name="aggregation">Service for aggregation operations</param>
        /// <param name="question">Service for question operations</param>
        /// <param name="questionRequirement">Service for question requirement management</param>
        /// <param name="assessmentUtil">Utility service for assessment operations</param>
        /// <param name="adminTabBusiness">Service for admin operations</param>
        /// <param name="galleryEditor">Service for gallery editing</param>
        /// <param name="documentBusiness">Service for document management</param>
        public ReportsController(CSETContext context, IReportsDataBusiness report, ITokenManager token,
            IAggregationBusiness aggregation, IQuestionBusiness question, IQuestionRequirementManager questionRequirement,
            IAssessmentUtil assessmentUtil, IAdminTabBusiness adminTabBusiness, IGalleryEditor galleryEditor,
            IDocumentBusiness documentBusiness)
        {
            _context = context;
            _report = report;
            _token = token;
            _aggregation = aggregation;
            _question = question;
            _questionRequirement = questionRequirement;
            _assessmentUtil = assessmentUtil;
            _adminTabBusiness = adminTabBusiness;
            _galleryEditor = galleryEditor;
            _overlay = new TranslationOverlay();
            _documentBusiness = documentBusiness;
        }

        /// <summary>
        /// Retrieves basic assessment information for report generation.
        /// </summary>
        /// <returns>
        /// 200 OK with assessment information data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides the foundational assessment data needed for various report types.
        /// The information includes assessment metadata, demographics, and basic configuration.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/info")]
        [ProducesResponseType(typeof(BasicReportData.INFORMATION), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetAssessmentInfoForReport()
        {
            int assessmentId = _token.AssessmentForUser();
            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData.INFORMATION info = _report.GetInformation();

            return Ok(info);
        }

        /// <summary>
        /// Generates a comprehensive security plan report for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with security plan data including controls, SAL information, and diagram data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates a detailed security plan report that includes:
        /// - Assessment information and demographics
        /// - Control lists (diagram-based or standard)
        /// - Security Assurance Level (SAL) tables
        /// - NIST information types and SAL mappings
        /// - Diagram zones (if diagram is enabled)
        /// 
        /// The report adapts based on whether the assessment uses diagrams or standard controls.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/securityplan")]
        [ProducesResponseType(typeof(BasicReportData), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetSecurityPlan()
        {
            int assessmentId = _token.AssessmentForUser();
            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData data = new BasicReportData();

            var ss = _context.STANDARD_SELECTION.Where(x => x.Assessment_Id == assessmentId).FirstOrDefault();
            if (ss != null)
            {
                data.ApplicationMode = ss.Application_Mode;
            }
            // Check if assessment is a diagram 
            data.information = _report.GetInformation();
            if (data.information.UseDiagram == true)
            {

                data.ControlList = _report.GetControlsDiagram(data.ApplicationMode);

            }
            else
            {
                data.ControlList = _report.GetControls(data.ApplicationMode);
            }

            data.genSalTable = _report.GetGenSals();
            data.salTable = _report.GetSals();
            data.nistTypes = _report.GetNistInfoTypes();
            data.nistSalTable = _report.GetNistSals();

            data.Zones = _report.GetDiagramZones();
            return Ok(data);
        }

        /// <summary>
        /// Generates an executive summary report for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with executive report data including top categories and questions
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates a high-level executive summary that includes:
        /// - Assessment information and demographics
        /// - Security Assurance Level (SAL) information
        /// - Top 5 categories by risk
        /// - Top 5 questions requiring attention
        /// - NIST information types and SAL mappings
        /// 
        /// This report is designed for executive audiences who need a concise overview
        /// of the assessment results and key areas of concern.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/executive")]
        [ProducesResponseType(typeof(BasicReportData), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetExecutive()
        {
            int assessmentId = _token.AssessmentForUser();

            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData data = new BasicReportData();
            data.information = _report.GetInformation();

            data.genSalTable = _report.GetGenSals();
            data.salTable = _report.GetSals();
            data.nistTypes = _report.GetNistInfoTypes();
            data.nistSalTable = _report.GetNistSals();

            data.top5Categories = _report.GetTop5Categories();
            data.top5Questions = _report.GetTop5Questions();
            return Ok(data);
        }

        /// <summary>
        /// Returns basic report info plus basic maturity model info without all of the questions like "executivecmmc" does.
        /// </summary>
        /// <returns>
        /// 200 OK with executive maturity report data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides a streamlined maturity model report that includes:
        /// - Assessment information and demographics
        /// - Basic maturity model data without detailed questions
        /// - High-level maturity insights for executive review
        /// 
        /// This is a lighter version of the full CMMC executive report, focusing on
        /// maturity model results without overwhelming detail.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/executivematurity")]
        [ProducesResponseType(typeof(MaturityReportData), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetExecutiveMaturity()
        {
            int assessmentId = _token.AssessmentForUser();
            _report.SetReportsAssessmentId(assessmentId);
            MaturityReportData data = new MaturityReportData(_context);
            data.MaturityModels = [_report.GetBasicMaturityModel()];
            data.information = _report.GetInformation();

            return Ok(data);
        }

        /// <summary>
        /// Generates a comprehensive CMMC (Cybersecurity Maturity Model Certification) executive report.
        /// </summary>
        /// <returns>
        /// 200 OK with detailed CMMC report data including maturity analysis
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates a complete CMMC executive report that includes:
        /// - Assessment information and demographics
        /// - Full maturity model data with detailed analysis
        /// - CMMC-specific maturity insights and recommendations
        /// - Comprehensive maturity assessment results
        /// 
        /// This report provides detailed CMMC compliance information suitable for
        /// both executive review and detailed analysis.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/executivecmmc")]
        [ProducesResponseType(typeof(MaturityReportData), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetCMMCReport()
        {
            int assessmentId = _token.AssessmentForUser();
            _report.SetReportsAssessmentId(assessmentId);
            MaturityReportData data = new MaturityReportData(_context);
            data.AnalyzeMaturityData();
            data.MaturityModels = _report.GetMaturityModelData();
            data.information = _report.GetInformation();
            data.AnalyzeMaturityData();

            return Ok(data);
        }

        /// <summary>
        /// Generates a CMMC site summary report for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with CMMC site summary data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates a CMMC site summary report that includes:
        /// - Assessment information and demographics
        /// - Maturity model data with analysis
        /// - Site-specific CMMC compliance information
        /// - Optimized for site-level reporting
        /// 
        /// The report is designed for site-level CMMC assessments and includes
        /// navigation property cleanup to avoid circular references in JSON serialization.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/sitesummarycmmc")]
        [ProducesResponseType(typeof(MaturityReportData), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetSiteSummaryCMMCReport()
        {
            int assessmentId = _token.AssessmentForUser();
            _report.SetReportsAssessmentId(assessmentId);
            MaturityReportData data = new MaturityReportData(_context);

            data.MaturityModels = _report.GetMaturityModelData();
            data.information = _report.GetInformation();
            data.AnalyzeMaturityData();


            // null out a few navigation properties to avoid circular references that blow up the JSON stringifier
            data.MaturityModels.ForEach(d =>
            {
                d.MaturityQuestions.ForEach(q =>
                {
                    q.Answer.Assessment = null;
                });
            });


            return Ok(data);
        }



        //--------------------------------
        // RRA Controllers
        //--------------------------------

        /// <summary>
        /// Generates the main RRA (Risk and Resilience Assessment) report.
        /// </summary>
        /// <returns>
        /// 200 OK with RRA main report data including maturity analysis
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates the main RRA report that includes:
        /// - Assessment information and demographics
        /// - Maturity model data with analysis
        /// - Risk and resilience assessment results
        /// - Comprehensive maturity insights
        /// 
        /// The RRA framework focuses on risk management and resilience capabilities
        /// for critical infrastructure organizations.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/rramain")]
        [ProducesResponseType(typeof(MaturityReportData), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetRRAMainReport()
        {
            int assessmentId = _token.AssessmentForUser();

            var mm = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);
            ReportsDataBusiness reportsDataManager = new ReportsDataBusiness(_context, _assessmentUtil, _adminTabBusiness, null, mm, _questionRequirement, _token);
            reportsDataManager.SetReportsAssessmentId(assessmentId);

            MaturityReportData data = new MaturityReportData(_context);
            data.AnalyzeMaturityData();
            data.MaturityModels = reportsDataManager.GetMaturityModelData();
            data.information = reportsDataManager.GetInformation();
            data.AnalyzeMaturityData();

            return Ok(data);
        }

        /// <summary>
        /// Generates a detailed RRA (Risk and Resilience Assessment) report.
        /// </summary>
        /// <returns>
        /// 200 OK with detailed RRA report data including goal-based summaries
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates a detailed RRA report that includes:
        /// - Overall RRA summary statistics
        /// - Goal-based RRA summaries
        /// - Detailed breakdown by resilience goals
        /// - Translated content based on user language preference
        /// 
        /// The detailed report provides granular insights into specific resilience
        /// goals and capabilities within the RRA framework.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/rradetail")]
        [ProducesResponseType(typeof(MaturityReportDetailData), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetRRADetailReport()
        {
            int assessmentId = _token.AssessmentForUser();
            var lang = _token.GetCurrentLanguage();

            _context.FillEmptyMaturityQuestionsForAnalysis(assessmentId);

            RRASummary summary = new RRASummary(_context);
            MaturityReportDetailData data = new MaturityReportDetailData();
            data.RRASummaryOverall = summary.GetSummaryOverall(assessmentId);


            data.RRASummary = summary.GetRRASummary(assessmentId);

            data.RRASummaryByGoal = summary.GetRRASummaryByGoal(assessmentId);

            foreach (DataLayer.Manual.usp_getRRASummaryByGoal q in data.RRASummaryByGoal)
            {
                var o = _overlay.GetMaturityGrouping(q.Grouping_Id, lang);
                if (o != null)
                {
                    q.Title = o.Title;
                }
            }

            data.RRASummaryByGoalOverall = summary.GetRRASummaryByGoalOverall(assessmentId);
            return Ok(data);
        }

        /// <summary>
        /// Returns a list of RRA (Risk and Resilience Assessment) questions.
        /// </summary>
        /// <returns>
        /// 200 OK with list of RRA maturity questions
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all RRA maturity questions for the current assessment.
        /// The questions are returned in the user's preferred language and include
        /// all maturity model questions specific to the RRA framework.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/rraquestions")]
        [ProducesResponseType(typeof(List<MaturityQuestion>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetRRAQuestions()
        {
            var questions = new List<MaturityQuestion>();

            int assessmentId = _token.AssessmentForUser();
            string lang = _token.GetCurrentLanguage();

            var biz = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);

            var resp = biz.GetMaturityQuestions(assessmentId, true, 0, lang);

            // get all supplemental info for questions, because it is not included in the previous method
            var dict = biz.GetReferences(assessmentId);


            resp.Groupings.First().SubGroupings.ForEach(goal => goal.Questions.ForEach(q =>
            {
                var newQ = new MaturityQuestion
                {
                    Mat_Question_Id = q.QuestionId,
                    Question_Title = q.DisplayNumber,
                    Question_Text = q.QuestionText,
                    Answer = new ANSWER() { Answer_Text = q.Answer },
                    ReferenceText = dict[q.QuestionId]
                };

                questions.Add(newQ);
            }));

            foreach (MaturityQuestion q in questions)
            {
                var translatedGroup = _overlay.GetMaturityQuestion(q.Mat_Question_Id, lang);
                if (translatedGroup != null)
                {
                    q.Question_Text = translatedGroup.QuestionText;
                    q.ReferenceText = translatedGroup.ReferenceText;
                }
            }

            return Ok(questions);
        }



        //--------------------------------
        // VADR Controllers
        //--------------------------------

        /// <summary>
        /// Generates the main VADR (Vulnerability and Disposition Report) report.
        /// </summary>
        /// <returns>
        /// 200 OK with VADR main report data including maturity analysis
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates the main VADR report that includes:
        /// - Assessment information and demographics
        /// - Maturity model data with analysis
        /// - Vulnerability assessment results
        /// - Comprehensive maturity insights
        /// 
        /// The VADR framework focuses on vulnerability management and disposition
        /// of security findings for critical infrastructure organizations.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/vadrmain")]
        [ProducesResponseType(typeof(MaturityReportData), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetVADRMainReport()
        {
            int assessmentId = _token.AssessmentForUser();

            var mm = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);
            ReportsDataBusiness reportsDataManager = new ReportsDataBusiness(_context, _assessmentUtil, _adminTabBusiness, null, mm, _questionRequirement, _token);
            reportsDataManager.SetReportsAssessmentId(assessmentId);

            MaturityReportData data = new MaturityReportData(_context);
            data.AnalyzeMaturityData();
            data.MaturityModels = reportsDataManager.GetMaturityModelData();
            data.information = reportsDataManager.GetInformation();
            data.AnalyzeMaturityData();

            return Ok(data);
        }

        /// <summary>
        /// Generates a detailed VADR (Vulnerability and Disposition Report) report.
        /// </summary>
        /// <returns>
        /// 200 OK with detailed VADR report data including goal-based summaries
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates a detailed VADR report that includes:
        /// - Overall VADR summary statistics
        /// - Goal-based VADR summaries
        /// - Detailed breakdown by vulnerability goals
        /// - Comprehensive vulnerability disposition information
        /// 
        /// The detailed report provides granular insights into specific vulnerability
        /// management goals and capabilities within the VADR framework.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/vadrdetail")]
        [ProducesResponseType(typeof(MaturityReportDetailData), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetVADRDetailReport()
        {
            int assessmentId = _token.AssessmentForUser();

            _context.FillEmptyMaturityQuestionsForAnalysis(assessmentId);

            VADRReports summary = new VADRReports(_context);
            MaturityReportDetailData data = new MaturityReportDetailData();
            data.VADRSummaryOverall = summary.GetSummaryOverall(assessmentId);
            data.VADRSummary = summary.GetVADRSummary(assessmentId);
            data.VADRSummaryByGoal = summary.GetVADRSummaryByGoal(assessmentId);
            data.VADRSummaryByGoalOverall = summary.GetVADRSummaryByGoalOverall(assessmentId);
            return Ok(data);
        }

        /// <summary>
        /// Returns a list of VADR (Vulnerability and Disposition Report) questions.
        /// </summary>
        /// <returns>
        /// 200 OK with list of VADR maturity questions
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all VADR maturity questions for the current assessment.
        /// The questions are returned in the user's preferred language and include
        /// all maturity model questions specific to the VADR framework.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/vadrquestions")]
        [ProducesResponseType(typeof(List<MaturityQuestion>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetVADRQuestions()
        {
            var questions = new List<MaturityQuestion>();

            int assessmentId = _token.AssessmentForUser();
            string lang = _token.GetCurrentLanguage();

            var mm = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);

            var resp = mm.GetMaturityQuestions(assessmentId, true, 0, lang);

            // get all supplemental info for questions, because it is not included in the previous method
            var dict = mm.GetSourceFiles();


            resp.Groupings.ForEach(g =>
            {
                g.SubGroupings.ForEach(goal => goal.Questions.ForEach(q =>
                {
                    string refText;
                    if (!dict.TryGetValue(q.QuestionId, out refText))
                    {
                        refText = "None";
                    }
                    var newQ = new MaturityQuestion
                    {
                        Question_Title = q.DisplayNumber,
                        Question_Text = q.QuestionText,
                        Answer = new ANSWER() { Answer_Text = q.Answer },
                        ReferenceText = refText,
                        Parent_Question_Id = q.ParentQuestionId
                    };

                    questions.Add(newQ);
                }));
            });

            return Ok(questions);
        }

        /// <summary>
        /// Generates and exports an Excel spreadsheet with a POAM (Plan of Action and Milestones) template.
        /// </summary>
        /// <returns>
        /// 200 OK with Excel file download
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates an Excel spreadsheet containing a POAM template
        /// based on the current assessment's maturity questions. The file includes:
        /// - Assessment information and demographics
        /// - Maturity questions with current answers
        /// - POAM template structure for tracking remediation
        /// - Exportable format for external tracking systems
        /// 
        /// The Excel file is automatically named based on the assessment ID and
        /// can be used for external POAM tracking and management.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/poam/excelexport")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetExcelExport()
        {
            int assessmentId = _token.AssessmentForUser();
            string lang = _token.GetCurrentLanguage();

            // Create a memory stream to hold the Excel file
            using (var memoryStream = new MemoryStream())
            {
                var mm = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness).GetMaturityQuestions(assessmentId, true, 0, lang);

                // Generate the Excel file
                ExportPoamBusiness.GenerateSpreadSheet(memoryStream, mm);

                // Return the file as a downloadable attachment
                return File(
                    memoryStream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ExportPoamBusiness.GetFilename(assessmentId, _context)
                );
            }
        }

        //--------------------------------
        // HYDRO Controllers
        //--------------------------------

        /// <summary>
        /// Retrieves HYDRO donut chart data for visualization.
        /// </summary>
        /// <returns>
        /// 200 OK with HYDRO donut chart data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides data for HYDRO framework visualization charts.
        /// The data is formatted for donut chart display showing HYDRO maturity
        /// levels and distribution across different categories.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/getHydroDonutData")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetHydroDonutData()
        {
            int assessmentId = _token.AssessmentForUser();
            _context.FillEmptyMaturityQuestionsForAnalysis(assessmentId);

            var hmm = new HydroMaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);

            return Ok(hmm.GetHydroDonutData(assessmentId));
        }

        /// <summary>
        /// Retrieves HYDRO action items for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with HYDRO action items data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides HYDRO-specific action items and recommendations
        /// based on the current assessment results. The action items are tailored
        /// to the HYDRO framework requirements and maturity model.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/reports/getHydroActionItems")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetHydroActionItems()
        {
            int assessmentId = _token.AssessmentForUser();
            _context.FillEmptyMaturityQuestionsForAnalysis(assessmentId);

            var hmm = new HydroMaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);

            return Ok(hmm.GetHydroActions(assessmentId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/reports/getHydroActionItemsReport")]
        public IActionResult GetHydroActionItemsReport()
        {
            int assessmentId = _token.AssessmentForUser();
            _context.FillEmptyMaturityQuestionsForAnalysis(assessmentId);

            var hmm = new HydroMaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);

            return Ok(hmm.GetHydroActionsReport(assessmentId));
        }


        //--------------------------------
        // MVRA Controllers
        //--------------------------------


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/reports/mvradetail")]
        public IActionResult MvraDetail()
        {
            int assessmentId = _token.AssessmentForUser();

            _context.FillEmptyMaturityQuestionsForAnalysis(assessmentId);

            MvraSummary summary = new MvraSummary(_context);

            object o = new object();
            return Ok(o);
        }


        /// <summary>
        /// Returns the information for a report containing 
        /// questions with alternate justification.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/reports/getAltList")]
        public IActionResult GetAltList()
        {
            int assessmentId = _token.AssessmentForUser();

            _report.SetReportsAssessmentId(assessmentId);
            var data = new BasicReportData();
            data.QuestionsWithAltJust = _report.GetQuestionsWithAlternateJustification();
            data.MaturityQuestionsWithAlt = _report.GetAlternatesList();
            data.information = _report.GetInformation();


            // null out a few navigation properties to avoid circular references that blow up the JSON stringifier
            data.MaturityQuestionsWithAlt.ForEach(d =>
            {
                d.ANSWER.Assessment = null;
                d.Mat.Maturity_Model = null;
            });

            return Ok(data);
        }


        /// <summary>
        /// 
        /// </summary>
        [HttpGet]
        [Route("api/reports/observations")]
        public IActionResult GetObservations()
        {
            int assessmentId = _token.AssessmentForUser();

            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData data = new BasicReportData();
            data.information = _report.GetInformation();
            data.Individuals = _report.GetObservationIndividuals();
            return Ok(data);
        }


        /// <summary>
        /// Returns a file stream containing Observations in a CSV format.
        /// </summary>
        [HttpGet]
        [Route("api/reports/observations/excel")]
        public IActionResult ExportObservationsCsv()
        {
            _report.SetToken(_token);

            int assessmentId = _token.AssessmentForUser();
            string lang = _token.GetCurrentLanguage();

            var info = _context.INFORMATION.Where(x => x.Id == assessmentId).FirstOrDefault();

            // Generate the Excel file
            using (var memoryStream = new MemoryStream())
            {
                var otx = new ObservationsToExcel(_context, _report);
                otx.GenerateSpreadsheet(assessmentId, memoryStream);

                // Return the file as a downloadable attachment
                return File(
                    memoryStream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{info.Assessment_Name} - Observations.xlsx"
                );
            }
        }


        [HttpGet]
        [Route("api/reports/sitesummary")]
        public IActionResult GetSiteSummary()
        {
            int assessmentId = _token.AssessmentForUser();

            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData data = new BasicReportData();
            data.information = _report.GetInformation();

            data.genSalTable = _report.GetGenSals();
            data.salTable = _report.GetSals();
            data.nistTypes = _report.GetNistInfoTypes();
            data.nistSalTable = _report.GetNistSals();

            data.DocumentLibraryEntries = _report.GetDocumentLibrary();
            data.RankedQuestionsTable = _report.GetRankedQuestions();
            data.FinancialQuestionsTable = _report.GetFinancialQuestions();
            data.QuestionsWithComments = _report.GetQuestionsWithComments();
            data.QuestionsMarkedForReview = _report.GetQuestionsMarkedForReview();
            data.QuestionsWithAltJust = _report.GetQuestionsWithAlternateJustification();
            return Ok(data);
        }
        [HttpGet]
        [Route("api/reports/physicalsummary")]
        public IActionResult GetPhysicalSummary()
        {
            int assessmentId = _token.AssessmentForUser();

            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData data = new BasicReportData();
            data.information = _report.GetInformation();
            data.QuestionsWithSupplementals = _report.GetQuestionsWithSupplementals();
            data.RankedQuestionsTable = _report.GetRankedQuestions();
            data.QuestionsWithComments = _report.GetQuestionsWithComments();
            data.QuestionsMarkedForReview = _report.GetQuestionsMarkedForReview();
            data.QuestionsWithAltJust = _report.GetQuestionsWithAlternateJustification();
            return Ok(data);
        }


        [HttpGet]
        [Route("api/reports/detail")]
        public IActionResult GetDetail()
        {
            int assessmentId = _token.AssessmentForUser();

            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData data = new BasicReportData();
            data.information = _report.GetInformation();

            data.genSalTable = _report.GetGenSals();
            data.salTable = _report.GetSals();
            data.nistTypes = _report.GetNistInfoTypes();
            data.nistSalTable = _report.GetNistSals();

            data.DocumentLibraryEntries = _report.GetDocumentLibrary();
            data.RankedQuestionsTable = _report.GetRankedQuestions();
            data.QuestionsWithComments = _report.GetQuestionsWithComments();
            data.QuestionsMarkedForReview = _report.GetQuestionsMarkedForReview();
            data.QuestionsWithAltJust = _report.GetQuestionsWithAlternateJustification();
            data.StandardsQuestions = _report.GetQuestionsForEachStandard();
            data.ComponentQuestions = _report.GetComponentQuestions();
            return Ok(data);
        }


        /// <summary>
        /// Returns data needed for the Trend report.  
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/reports/trend-report")]
        public IActionResult GetTrendReport(int aggregationID)
        {
            AggregationReportData response = new AggregationReportData();
            response.SalList = new List<BasicReportData.OverallSALTable>();
            response.DocumentLibraryEntries = new List<DocumentLibraryEntry>();

            var assessmentList = _aggregation.GetAssessmentsForAggregation((int)aggregationID);

            var aggregation = _aggregation.GetAggregation((int)aggregationID);


            response.AggregationName = assessmentList.Aggregation.AggregationName;
            response.Information = new AggInformation()
            {
                Assessment_Date = aggregation.AggregationDate,
                Assessment_Name = aggregation.AggregationName,
                Assessor_Name = aggregation.AssessorName
            };

            foreach (var a in assessmentList.Assessments)
            {
                _report.SetReportsAssessmentId(a.AssessmentId);
                // Incorporate SAL values into response
                var salTable = _report.GetSals();

                var entry = new BasicReportData.OverallSALTable();
                response.SalList.Add(entry);
                entry.Alias = a.Alias;
                entry.OSV = salTable.OSV;
                entry.Q_CV = "";
                entry.Q_IV = "";
                entry.Q_AV = "";
                entry.LastSalDeterminationType = salTable.LastSalDeterminationType;

                if (salTable.LastSalDeterminationType != "GENERAL")
                {
                    entry.Q_CV = salTable.Q_CV;
                    entry.Q_IV = salTable.Q_IV;
                    entry.Q_AV = salTable.Q_AV;
                }


                // Document Library 
                var documentLibraryEntries = _report.GetDocumentLibrary();
                foreach (var docEntry in documentLibraryEntries)
                {
                    docEntry.Alias = a.Alias;
                    response.DocumentLibraryEntries.Add(docEntry);
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// Returns data needed for the Compare report.  
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/reports/compare-report")]
        public IActionResult GetCompareReport(int aggregationID)
        {
            AggregationReportData response = new AggregationReportData();
            response.SalList = new List<BasicReportData.OverallSALTable>();
            response.DocumentLibraryEntries = new List<DocumentLibraryEntry>();


            var assessmentList = _aggregation.GetAssessmentsForAggregation((int)aggregationID);
            Aggregation ag = _aggregation.GetAggregation((int)aggregationID);
            response.AggregationName = assessmentList.Aggregation.AggregationName;

            response.Information = new AggInformation()
            {
                Assessment_Name = ag.AggregationName,
                Assessment_Date = ag.AggregationDate,
                Assessor_Name = ag.AssessorName
            };

            foreach (var a in assessmentList.Assessments)
            {
                _report.SetReportsAssessmentId(a.AssessmentId);
                // Incorporate SAL values into response
                var salTable = _report.GetSals();

                var entry = new BasicReportData.OverallSALTable();
                response.SalList.Add(entry);
                entry.Alias = a.Alias;
                entry.OSV = salTable.OSV;
                entry.Q_CV = "";
                entry.Q_IV = "";
                entry.Q_AV = "";
                entry.LastSalDeterminationType = salTable.LastSalDeterminationType;

                if (salTable.LastSalDeterminationType != "GENERAL")
                {
                    entry.Q_CV = salTable.Q_CV;
                    entry.Q_IV = salTable.Q_IV;
                    entry.Q_AV = salTable.Q_AV;
                }


                // Document Library 
                var documentLibraryEntries = _report.GetDocumentLibrary();
                foreach (var docEntry in documentLibraryEntries)
                {
                    docEntry.Alias = a.Alias;
                    response.DocumentLibraryEntries.Add(docEntry);
                }
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("api/reports/getconfidentialtypes")]
        public IActionResult GetConfidentialTypes()
        {
            return Ok(_report.GetConfidentialTypes());
        }


        // <summary>
        /// Returns a Q or R indicating the assessment's application mode, Questions or Requirements.
        /// </summary>
        /// <param name="assessmentId"></param>
        /// <returns></returns>
        protected string GetApplicationMode(int assessmentId)
        {
            var mode = _context.STANDARD_SELECTION.Where(x => x.Assessment_Id == assessmentId).Select(x => x.Application_Mode).FirstOrDefault();

            if (mode == null)
            {
                // default to Questions mode
                mode = "Q";
                SetMode(mode);
            }

            return mode;

        }

        private void SetMode(string mode)
        {
            int assessmentId = _token.AssessmentForUser();
            _question.SetQuestionAssessmentId(assessmentId);
            _questionRequirement.SetApplicationMode(mode);
        }


        /// <summary>
        /// Returns questions and supplemental content
        /// for a set or maturity model.
        /// This report is not intended to be something
        /// that a CSET user would view.  The intent is
        /// for confirming new modules under development.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/reports/modulecontent")]
        public IActionResult ModuleContentReport([FromQuery] string set)
        {
            var lang = _token.GetCurrentLanguage();

            var report = new ModuleContentReport(_context, _questionRequirement, _galleryEditor);
            report.SetLanguage(lang);
            var resp = report.GetResponse(set);
            return Ok(resp);
        }

        /// <summary>
        /// Validates the required fields for the CISA Assessor Workflow in order to unlock reports and export.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/reports/CisaAssessorWorkflowValidateFields")]
        public IActionResult CisaAssessorWorkflowValidateFields()
        {
            var assessmentId = _token.AssessmentForUser();

            var iodDemoBusiness = new DemographicExtBusiness(_context);
            var demoBusiness = new DemographicBusiness(_context, _assessmentUtil);
            var cisServiceDemographicBusiness = new CisDemographicBusiness(_context, _assessmentUtil);

            Demographics demographics = demoBusiness.GetDemographics(assessmentId);
            DemographicExt iodDemograhics = iodDemoBusiness.GetDemographics(assessmentId);
            CisServiceDemographics cisServiceDemographics = cisServiceDemographicBusiness.GetServiceDemographics(assessmentId);
            CisServiceComposition cisServiceComposition = cisServiceDemographicBusiness.GetServiceComposition(assessmentId);

            CisaAssessorWorkflowFieldValidator validator = new CisaAssessorWorkflowFieldValidator(demographics, iodDemograhics, cisServiceDemographics, cisServiceComposition);
            return Ok(validator.ValidateFields());
        }


        [HttpGet]
        [Route("api/reports/getStandardAnsweredQuestions")]
        public async Task<IActionResult> GetStandardAnsweredQuestions()
        {
            int assessmentId = _token.AssessmentForUser();

            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData data = new BasicReportData();
            data.information = _report.GetInformation();

            data.StandardsQuestions = await _report.GetStandardQuestionAnswers(assessmentId);

            return Ok(data);
        }


        [HttpGet]
        [Route("api/reports/getStandardCommentsAndMfr")]
        public IActionResult GetStandardCommentsAndMfr()
        {
            int assessmentId = _token.AssessmentForUser();

            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData data = new BasicReportData();
            data.information = _report.GetInformation();

            data.QuestionsWithComments = _report.GetQuestionsWithComments();
            data.QuestionsMarkedForReview = _report.GetQuestionsMarkedForReview();
            return Ok(data);
        }


        [HttpGet]
        [Route("api/reports/getReviewedQuestions")]
        public IActionResult GetReviewedQuestions()
        {
            int assessmentId = _token.AssessmentForUser();

            _report.SetReportsAssessmentId(assessmentId);
            BasicReportData data = new BasicReportData();
            data.information = _report.GetInformation();

            data.QuestionsWithComments = _report.GetQuestionsWithComments();
            data.QuestionsMarkedForReview = _report.GetQuestionsReviewed();
            return Ok(data);
        }
    }
}
