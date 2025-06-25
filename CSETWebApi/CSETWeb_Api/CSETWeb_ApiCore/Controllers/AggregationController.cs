//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 

using System;
using CSETWebCore.Business.Aggregation;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Aggregation;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using CSETWebCore.Business.Authorization;


namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for assessment aggregation functionality in CSET.
    /// This controller handles the creation, management, and analysis of assessment aggregations,
    /// allowing users to combine multiple assessments for comparative analysis and reporting.
    /// Note: This controller is marked as obsolete and may be removed in future versions.
    /// Requires authentication and authorization via CsetAuthorize attribute.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    [Obsolete("This controller is no longer used")]

    public class AggregationController : ControllerBase
    {
        private readonly ITokenManager _token;
        private readonly CSETContext _context;

        /// <summary>
        /// Initializes a new instance of the AggregationController.
        /// </summary>
        /// <param name="token">Token manager for authentication and authorization</param>
        /// <param name="context">Database context for aggregation operations</param>
        public AggregationController(ITokenManager token, CSETContext context)
        {
            _token = token;
            _context = context;
        }

        /// <summary>
        /// Returns a list of aggregations that the current user is allowed to see.
        /// The user must be authorized to view all assessments involved in the aggregation.
        /// </summary>
        /// <param name="mode">Mode parameter for filtering aggregations</param>
        /// <returns>
        /// 200 OK with list of aggregations if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Retrieves aggregations based on the current user's permissions and the specified mode.
        /// The user must have access to all assessments included in each aggregation.
        /// The mode parameter can be used to filter aggregations by different criteria.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/getaggregations")]
        public IActionResult GetAggregations([FromQuery] string mode)
        {
            // Get the current userid to set as the Assessment creator and first attached user
            var currentUserId = _token.GetCurrentUserId();

            var manager = new AggregationBusiness(_context, _token);
            return Ok(manager.GetAggregations(mode, (int)currentUserId));
        }

        /// <summary>
        /// Creates a new aggregation.
        /// </summary>
        /// <param name="mode">Mode parameter for the new aggregation</param>
        /// <returns>
        /// 200 OK with new aggregation details if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Creates a new aggregation with the specified mode.
        /// The aggregation will be associated with the current user as the creator.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/create")]
        public IActionResult CreateAggregation([FromQuery] string mode)
        {
            var manager = new AggregationBusiness(_context, _token);
            return Ok(manager.CreateAggregation(mode));
        }

        /// <summary>
        /// Retrieves a specific aggregation by ID.
        /// </summary>
        /// <returns>
        /// 200 OK with aggregation details if successful
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if aggregation ID not found in token
        /// </returns>
        /// <remarks>
        /// Retrieves the aggregation specified by the aggregation ID in the user's token.
        /// Returns null if no aggregation ID is found in the token payload.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/get")]
        public IActionResult GetAggregation()
        {
            var aggregationID = _token.PayloadInt("aggreg");
            if (aggregationID == null)
            {
                return null;
            }

            var manager = new AggregationBusiness(_context, _token);
            return Ok(manager.GetAggregation((int)aggregationID));
        }

        /// <summary>
        /// Updates an existing aggregation.
        /// </summary>
        /// <param name="aggregation">Aggregation object containing updated information</param>
        /// <returns>
        /// 200 OK if update successful
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if aggregation ID not found in token
        /// </returns>
        /// <remarks>
        /// Updates the aggregation specified by the aggregation ID in the user's token.
        /// Saves the provided aggregation information to the database.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/update")] 
        public IActionResult UpdateAggregation([FromBody] Aggregation aggregation)
        {
            var aggregationID = _token.PayloadInt("aggreg");
            if (aggregationID == null)
            {
                return Ok();
            }

            var manager = new AggregationBusiness(_context, _token);
            manager.SaveAggregationInformation(aggregation.AggregationId, aggregation);
            return Ok();
        }

        /// <summary>
        /// Deletes an aggregation.
        /// </summary>
        /// <param name="aggregationId">ID of the aggregation to delete</param>
        /// <returns>
        /// 200 OK if deletion successful
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if aggregation ID not found in token
        /// </returns>
        /// <remarks>
        /// Deletes the specified aggregation from the system.
        /// The aggregation ID is validated against the token payload for security.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/delete")]
        public IActionResult DeleteAggregation([FromQuery] int aggregationId)
        {
            var aggregationID = _token.PayloadInt("aggreg");
            if (aggregationID == null)
            {
                return Ok();
            }
            var manager = new AggregationBusiness(_context, _token);
            manager.DeleteAggregation(aggregationId);
            return Ok();
        }

        /// <summary>
        /// Retrieves assessments associated with an aggregation.
        /// </summary>
        /// <returns>
        /// 200 OK with list of assessments if successful
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if aggregation ID not found in token
        /// </returns>
        /// <remarks>
        /// Returns all assessments that are part of the aggregation specified in the user's token.
        /// This includes both selected and available assessments for the aggregation.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/getassessments")]
        public IActionResult GetAssessmentsForAggregation()
        {
            var aggregationID = _token.PayloadInt("aggreg");
            if (aggregationID == null)
            {
                return Ok();
            }

            var manager = new AggregationBusiness(_context, _token);
            return Ok(manager.GetAssessmentsForAggregation((int)aggregationID));
        }

        /// <summary>
        /// Saves the selection state of an assessment in an aggregation.
        /// </summary>
        /// <param name="request">AssessmentSelection object containing assessment ID and selection state</param>
        /// <returns>
        /// 200 OK with selection result if successful
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if aggregation ID not found in token
        /// </returns>
        /// <remarks>
        /// Updates whether an assessment is selected for inclusion in the aggregation analysis.
        /// The selection state determines if the assessment data is included in aggregated reports.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/saveassessmentselection")]
        public IActionResult SaveAssessmentSelection([FromBody] AssessmentSelection request)
        {
            var aggregationID = _token.PayloadInt("aggreg");
            if (aggregationID == null)
            {
                return Ok();
            }

            var aggreg = new AggregationBusiness(_context, _token);
            return Ok(aggreg.SaveAssessmentSelection((int)aggregationID, request.AssessmentId, request.Selected));
        }

        /// <summary>
        /// Saves an alias for an assessment in an aggregation.
        /// </summary>
        /// <param name="req">AliasSaveRequest object containing assessment and alias information</param>
        /// <returns>
        /// 200 OK with new alias if successful
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if aggregation ID not found in token
        /// </returns>
        /// <remarks>
        /// Creates or updates an alias for an assessment within an aggregation.
        /// Aliases help identify assessments in aggregated reports and analysis.
        /// Returns the new alias information after saving.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/saveassessmentalias")]
        public IActionResult SaveAssessmentAlias([FromBody] AliasSaveRequest req)
        {
            var aggregationID = _token.PayloadInt("aggreg");
            if (aggregationID == null)
            {
                return Ok();
            }

            var aggreg = new AggregationBusiness(_context, _token);
            var newAlias = aggreg.SaveAssessmentAlias((int)aggregationID, req.aliasAssessment.AssessmentId, req.aliasAssessment.Alias, req.assessmentList);

            return Ok(newAlias);
        }

        /// <summary>
        /// Retrieves commonly missed questions across assessments in an aggregation.
        /// </summary>
        /// <returns>
        /// 200 OK with list of missed questions if successful
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if aggregation ID not found in token
        /// </returns>
        /// <remarks>
        /// Analyzes all assessments in the aggregation to identify questions that are commonly missed.
        /// This helps identify areas where organizations typically struggle with compliance.
        /// Returns an empty list if no aggregation ID is found in the token.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/missedquestions")]
        public IActionResult GetCommonlyMissedQuestions()
        {
            var aggregationID = _token.PayloadInt("aggreg");
            if (aggregationID == null)
            {
                return Ok(new List<MissedQuestion>());
            }

            var manager = new AggregationBusiness(_context, _token);
            return Ok(manager.GetCommonlyMissedQuestions((int)aggregationID));
        }

        /// <summary>
        /// Retrieves commonly missed maturity questions across assessments in an aggregation.
        /// </summary>
        /// <returns>
        /// 200 OK with missed maturity questions response if successful
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if aggregation ID not found in token
        /// </returns>
        /// <remarks>
        /// Analyzes maturity model questions across all assessments in the aggregation.
        /// Identifies maturity questions that are commonly missed or scored low.
        /// Returns an empty response if no aggregation ID is found in the token.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/maturity/missedquestions")]
        public IActionResult GetCommonlyMissedMaturityQuestions()
        {
            var aggregationID = _token.PayloadInt("aggreg");
            if (aggregationID == null)
            {
                return Ok(new MissedQuestionResponse());
            }

            var manager = new AggregationMaturityBusiness(_context);
            return Ok(manager.GetCommonlyMissedQuestions((int)aggregationID));
        }

        //////////////////////////////////////////
        /// Merge
        //////////////////////////////////////////

        /// <summary>
        /// Retrieves answers for aggregation merge functionality.
        /// </summary>
        /// <returns>
        /// 200 OK with answers if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Retrieves answer data for use in aggregation merge operations.
        /// This endpoint supports the merging of assessment answers across multiple assessments.
        /// Currently returns an empty response as the functionality is not fully implemented.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/getanswers")]
        public IActionResult GetAnswers()
        {
            var aggreg = new AggregationBusiness(_context, _token);
            // return aggreg.GetAnswers(new List<int>() { 4, 5 });

            return Ok();
        }

        /// <summary>
        /// Sets a single answer text into the COMBINED_ANSWER table.
        /// </summary>
        /// <param name="answerId">ID of the answer to set</param>
        /// <param name="answerText">Text content for the answer</param>
        /// <returns>
        /// 200 OK if answer set successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Updates or creates a merged answer in the COMBINED_ANSWER table.
        /// This is used for merging answers from multiple assessments into a single aggregated view.
        /// Currently returns OK as the functionality is not fully implemented.
        /// </remarks>
        [HttpPost]
        [Route("api/aggregation/setmergeanswer")]
        public IActionResult SetMergeAnswer([FromQuery] int answerId, [FromQuery] string answerText)
        {
            var aggreg = new AggregationBusiness(_context, _token);
            // aggreg.SetMergeAnswer(answerId, answerText);

            return Ok();
        }
    }
}
