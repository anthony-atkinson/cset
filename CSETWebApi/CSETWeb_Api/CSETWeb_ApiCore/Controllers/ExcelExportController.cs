//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.DataLayer.Model;
using CSETWebCore.ExportCSV;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Maturity;
using CSETWebCore.Interfaces.ReportEngine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for Excel export functionality in CSET.
    /// This controller handles the export of assessment data to Excel spreadsheet format,
    /// including question answers, maturity model data, and network diagram information.
    /// Exports are formatted with one row per answer for detailed analysis.
    /// </summary>
    public class ExcelExportController : ControllerBase
    {
        private readonly ITokenManager _token;
        private readonly IDataHandling _data;
        private readonly IMaturityBusiness _maturity;
        private readonly IHttpContextAccessor _http;
        private CSETContext _context;
        private ExcelExporter _exporter;

        private string excelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private string excelExtension = ".xlsx";

        /// <summary>
        /// Initializes a new instance of the ExcelExportController.
        /// </summary>
        /// <param name="token">Token manager for authentication and authorization</param>
        /// <param name="data">Data handling service for export operations</param>
        /// <param name="maturity">Maturity business logic service</param>
        /// <param name="http">HTTP context accessor for request information</param>
        /// <param name="context">Database context for assessment operations</param>
        public ExcelExportController(ITokenManager token, IDataHandling data, IMaturityBusiness maturity,
            IHttpContextAccessor http, CSETContext context)
        {
            _token = token;
            _data = data;
            _maturity = maturity;
            _http = http;
            _context = context;
            _exporter = new ExcelExporter(_context, _data, _http, _token);
        }

        /// <summary>
        /// Exports an assessment into a spreadsheet with 1 row per answer.
        /// </summary>
        /// <returns>
        /// 200 OK with Excel file download if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Exports the current assessment to Excel format with comprehensive data.
        /// Fills empty maturity questions, regular questions, and network diagram questions
        /// to ensure complete data export. The exported file contains one row per answer
        /// for detailed analysis and reporting. The filename includes the application name
        /// and assessment name for easy identification.
        /// </remarks>
        [HttpGet]
        [Route("api/assessment/export/excel")]
        public IActionResult GetExcelExport()
        {
            var currentUserId = _token.GetUserId();
            int assessmentId = _token.AssessmentForUser();
            string appName = _token.Payload(Constants.Constants.Token_Scope);

            _context.FillEmptyMaturityQuestionsForAnalysis(assessmentId);
            _context.FillEmptyQuestionsForAnalysis(assessmentId);
            _context.FillNetworkDiagramQuestions(assessmentId);

            var stream = _exporter.ExportToCSV(assessmentId);
            stream.Flush();
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            return File(stream, excelContentType, GetFilename(assessmentId, appName));
        }

        /// <summary>
        /// Generates a filename for the Excel export based on assessment information.
        /// </summary>
        /// <param name="assessmentId">ID of the assessment being exported</param>
        /// <param name="appName">Application name from the token scope</param>
        /// <returns>Formatted filename for the Excel export</returns>
        /// <remarks>
        /// Creates a descriptive filename for the Excel export file.
        /// If an assessment name is available, it uses the format "{appName} Export - {assessmentName}.xlsx".
        /// Otherwise, it uses the default format "ExcelExport.xlsx".
        /// The filename helps users identify the exported file and its contents.
        /// </remarks>
        private string GetFilename(int assessmentId, string appName)
        {
            string filename = $"ExcelExport{excelExtension}";

            var assessmentName = _context.INFORMATION.Where(x => x.Id == assessmentId).FirstOrDefault()?.Assessment_Name;
            if (!string.IsNullOrEmpty(assessmentName))
            {
                filename = $"{appName} Export - {assessmentName}{excelExtension}";
            }

            return filename;
        }
    }
}
