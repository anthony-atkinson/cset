//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.Document;
using CSETWebCore.Interfaces.FileRepository;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Model.Document;
using CSETWebCore.Model.Question;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CSETWebCore.Interfaces.Maturity;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for file upload functionality in CSET assessments.
    /// Supports document uploads for questions, answers, and maturity model responses,
    /// with automatic answer creation and document management capabilities.
    /// </summary>
    [ApiController]
    public class FileUploadController : Controller
    {
        private readonly ITokenManager _tokenManager;
        private readonly CSETContext _context;
        private readonly IDocumentBusiness _documentManager;
        private readonly IFileRepository _fileRepo;
        private readonly IQuestionRequirementManager _answerManager;
        private readonly IMaturityBusiness _maturityBusiness;

        /// <summary>
        /// Initializes a new instance of the FileUploadController.
        /// </summary>
        /// <param name="tokenManager">Service for JWT token management</param>
        /// <param name="context">Database context</param>
        /// <param name="documentManager">Service for document management operations</param>
        /// <param name="fileRepo">Service for file repository operations</param>
        /// <param name="answerManager">Service for answer management operations</param>
        /// <param name="maturityBusiness">Service for maturity model operations</param>
        public FileUploadController(
            ITokenManager tokenManager,
            CSETContext context,
            IDocumentBusiness documentManager,
            IFileRepository fileRepo,
            IQuestionRequirementManager answerManager,
            IMaturityBusiness maturityBusiness
        )
        {
            _tokenManager = tokenManager;
            _context = context;
            _documentManager = documentManager;
            _fileRepo = fileRepo;
            _answerManager = answerManager;
            _maturityBusiness = maturityBusiness;
        }


        /// <summary>
        /// Uploads a file and associates it with a question or answer in the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with list of documents for the answer
        /// 400 Bad Request if upload parameters are invalid or upload fails
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint handles file uploads for assessment documentation. The endpoint
        /// accepts multipart form data containing the file and metadata about the question
        /// or answer it should be associated with.
        /// 
        /// Required form parameters:
        /// - questionId: ID of the question the document relates to
        /// - answerId: ID of the answer (optional, will be created if not provided)
        /// - title: Document title/name
        /// - questionType: Type of question ("Question", "Requirement", "Maturity", "Component")
        /// - file: The actual file to upload
        /// 
        /// The endpoint will:
        /// - Create an answer record if one doesn't exist
        /// - Store the uploaded file in the file repository
        /// - Create a document record linking the file to the answer
        /// - Return all documents associated with the answer
        /// 
        /// Supported file types and size limits are determined by the file repository
        /// configuration. Common supported formats include PDF, DOC, DOCX, XLS, XLSX,
        /// and image files.
        /// 
        /// Sample request:
        ///     POST /api/files/blob/create/
        ///     Content-Type: multipart/form-data
        ///     
        ///     Form data:
        ///     - questionId: 123
        ///     - title: "Security Policy Document"
        ///     - questionType: "Question"
        ///     - file: [binary file data]
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("/api/files/blob/create/")]
        [ProducesResponseType(typeof(List<Document>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Upload()
        {
            const string key_questionId = "questionId";
            const string key_answerId = "answerId";
            const string key_title = "title";
            const string key_questionType = "questionType";

            var assessmentId = _tokenManager.AssessmentForUser();
            _documentManager.SetUserAssessmentId(assessmentId);

            var keyDict = new Dictionary<string, string>();
            keyDict.Add(key_questionId, null);
            keyDict.Add(key_answerId, null);
            keyDict.Add(key_title, null);
            keyDict.Add(key_questionType, null);

            var loader = new FileUploadStream();
            FileUploadStreamResult result = null;

            try
            {
                result = await loader.ProcessUploadStream(HttpContext, keyDict);
            }
            catch
            {
                return StatusCode(400);
            }

            string questionType = result.FormNameValues[key_questionType];

            int questionId;
            if (!int.TryParse(result.FormNameValues[key_questionId], out questionId))
            {
                return StatusCode(400);
            }

            int answerId;
            if (!int.TryParse(result.FormNameValues[key_answerId], out answerId))
            {
                var answerObj = new ANSWER();

                // if no answerId was provided, try to find an answer for this assessment/question
                if (answerId == 0)
                {
                    answerObj = _context.ANSWER.FirstOrDefault(x =>
                        x.Assessment_Id == assessmentId && x.Question_Or_Requirement_Id == questionId);
                }

                if (answerObj == null)
                {
                    var answer = new Answer
                    {
                        QuestionId = questionId,
                        QuestionType = questionType
                    };

                    // 
                    if (questionType.ToLower() == "maturity")
                    {
                        var ans = _maturityBusiness.StoreAnswer(assessmentId, answer);
                        answerId = (int)ans.AnswerId;
                    }
                    else
                    {
                        _answerManager.InitializeManager(assessmentId);
                        answerId = _answerManager.StoreAnswer(answer);
                    }
                }
                else
                {
                    answerId = answerObj.Answer_Id;
                }
            }

            _documentManager.AddDocument(result.FormNameValues[key_title], answerId, result);

            // returns all documents for the answer to account for updating duplicate docs
            // not the most efficient, but there are lots of shenanigans involved in keeping
            // the frontend for this synced
            return Ok(_documentManager.GetDocumentsForAnswer(answerId));
        }
    }
}
