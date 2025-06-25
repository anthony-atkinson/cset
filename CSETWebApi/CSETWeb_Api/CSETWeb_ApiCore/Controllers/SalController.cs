//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Sal;
using CSETWebCore.Business.Standards;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Model.Sal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.Business.Authorization;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Standards;

namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for SAL (Security Assurance Level) operations in CSET.
    /// This controller handles comprehensive SAL management including NIST SAL methodology,
    /// standard-specific SAL calculations, confidence/integrity/availability levels,
    /// and SAL determination type management for cybersecurity assessments.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class SalController : ControllerBase
    {
        private readonly CSETContext _context;
        private readonly ITokenManager _token;
        private readonly IAssessmentUtil _assessmentUtil;
        private readonly IStandardsBusiness _standard;
        private readonly IStandardSpecficLevelRepository _standardRepo;
        private readonly IAssessmentModeData _assessmentModeData;

        /// <summary>
        /// Initializes a new instance of the SalController.
        /// </summary>
        /// <param name="context">The database context for SAL data access</param>
        /// <param name="token">The token manager for user authentication and assessment context</param>
        /// <param name="standard">The standards business service for standards operations</param>
        /// <param name="assessmentUtil">The assessment utility service for assessment operations</param>
        /// <param name="standardRepo">The standard-specific level repository for level management</param>
        /// <param name="assessmentModeData">The assessment mode data service for mode operations</param>
        public SalController(CSETContext context, ITokenManager token,
            IStandardsBusiness standard, IAssessmentUtil assessmentUtil, IStandardSpecficLevelRepository standardRepo,
            IAssessmentModeData assessmentModeData)
        {
            _context = context;
            _token = token;
            _standard = standard;
            _assessmentUtil = assessmentUtil;
            _standardRepo = standardRepo;
            _assessmentModeData = assessmentModeData;
        }

        /// <summary>
        /// Retrieves SAL values for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with SAL values if successful
        /// 409 Conflict if SAL retrieval fails
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves comprehensive SAL information for the assessment:
        /// - Overall SAL level and methodology
        /// - Confidence, Integrity, and Availability levels
        /// - SAL determination type and override settings
        /// - Standard-specific SAL calculations
        /// 
        /// The SAL system includes:
        /// - Overall SAL level determination
        /// - CIA (Confidence, Integrity, Availability) levels
        /// - SAL methodology selection
        /// - Override capabilities
        /// - Standard-specific calculations
        /// 
        /// SAL features:
        /// - Multiple SAL determination methods
        /// - CIA level management
        /// - Standard-specific SAL calculations
        /// - Override and manual SAL setting
        /// - Assessment-specific SAL values
        /// 
        /// The response includes:
        /// - Overall SAL level
        /// - Confidence level (C)
        /// - Integrity level (I)
        /// - Availability level (A)
        /// - SAL methodology
        /// - Override settings
        /// 
        /// SAL levels supported:
        /// - Low: Basic security assurance
        /// - Moderate: Medium security assurance
        /// - High: High security assurance
        /// - Very High: Very high security assurance
        /// 
        /// Usage scenarios:
        /// - SAL level display and management
        /// - CIA level configuration
        /// - SAL methodology selection
        /// - Assessment SAL setup
        /// - SAL value verification
        /// 
        /// The SAL retrieval process:
        /// - Gets assessment-specific SAL data
        /// - Calculates standard-specific SAL values
        /// - Retrieves CIA level information
        /// - Returns comprehensive SAL data
        /// - Handles calculation errors gracefully
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/SAL")]
        [ProducesResponseType(200)]
        [ProducesResponseType(409)]
        [ProducesResponseType(401)]
        public IActionResult GetSalValues()
        {
            try
            {
                int assessmentId = _token.AssessmentForUser();

                var biz = new SalBusiness(_context, _assessmentModeData, _assessmentUtil, _standard, _standardRepo);
                return Ok(biz.GetSals(assessmentId));
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");

                return Conflict();
            }
        }

        /// <summary>
        /// Updates the SAL determination type for the assessment.
        /// </summary>
        /// <param name="newType">The new SAL determination type to set</param>
        /// <returns>
        /// 200 OK if type updated successfully
        /// 400 Bad Request if update fails
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint updates the SAL determination type:
        /// - Sets the Last_Sal_Determination_Type for the assessment
        /// - Persists the change to the database
        /// - Supports SAL methodology tracking
        /// - Enables SAL type management
        /// 
        /// The SAL determination type indicates:
        /// - Which SAL methodology was used
        /// - How the SAL was calculated
        /// - SAL determination approach
        /// - Assessment SAL history
        /// 
        /// SAL determination types include:
        /// - General: General SAL methodology
        /// - NIST: NIST SAL methodology
        /// - Standard: Standard-specific SAL
        /// - Manual: Manually set SAL
        /// - Override: Overridden SAL value
        /// 
        /// The update process:
        /// - Validates the assessment exists
        /// - Updates the determination type
        /// - Persists changes to database
        /// - Handles errors gracefully
        /// 
        /// Usage scenarios:
        /// - SAL methodology tracking
        /// - SAL type management
        /// - Assessment SAL history
        /// - SAL determination logging
        /// - Methodology selection
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/SAL/Type")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetType(String newType)
        {
            try
            {
                int asssessmentId = _token.AssessmentForUser();
                STANDARD_SELECTION sTANDARD_SELECTION = await _context.STANDARD_SELECTION.FindAsync(asssessmentId);
                if (sTANDARD_SELECTION != null)
                {
                    sTANDARD_SELECTION.Last_Sal_Determination_Type = newType;
                    await _context.SaveChangesAsync();
                }
                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /// <summary>
        /// Saves SAL values and recalculates the overall SAL level.
        /// </summary>
        /// <param name="tmpsal">The Sals object containing SAL values to save</param>
        /// <returns>
        /// 200 OK with updated SAL values if successful
        /// 400 Bad Request if model state is invalid
        /// 404 Not Found if assessment doesn't exist
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint saves comprehensive SAL data and recalculates levels:
        /// - Saves SAL values to the database
        /// - Recalculates overall SAL level
        /// - Updates CIA levels and other settings
        /// - Handles SAL overrides and manual settings
        /// 
        /// The SAL save process includes:
        /// - Validation of SAL data
        /// - Database persistence of SAL values
        /// - Level manager operations
        /// - Standard repository updates
        /// - SAL level recalculation
        /// 
        /// SAL calculation features:
        /// - Overall SAL level determination
        /// - CIA level management
        /// - Standard-specific calculations
        /// - Override handling
        /// - Assessment updates
        /// 
        /// The Sals object contains:
        /// - Selected SAL level
        /// - SAL override settings
        /// - CIA levels (Confidence, Integrity, Availability)
        /// - SAL methodology
        /// - Assessment-specific data
        /// 
        /// SAL calculation process:
        /// - Updates STANDARD_SELECTION record
        /// - Saves other levels via LevelManager
        /// - Initializes level management
        /// - Handles SAL overrides
        /// - Recalculates overall SAL
        /// 
        /// Usage scenarios:
        /// - SAL value updates
        /// - CIA level configuration
        /// - SAL methodology changes
        /// - Override management
        /// - Assessment SAL updates
        /// 
        /// The save process:
        /// - Validates input data
        /// - Updates database records
        /// - Recalculates SAL levels
        /// - Handles concurrency issues
        /// - Returns updated SAL data
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/SAL")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public IActionResult PostSAL(Sals tmpsal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int assessmentId = _token.AssessmentForUser();
            TinyMapper.Bind<Sals, STANDARD_SELECTION>();
            STANDARD_SELECTION sTANDARD_SELECTION = _context.STANDARD_SELECTION.Where(x => x.Assessment_Id == assessmentId).FirstOrDefault();
            if (sTANDARD_SELECTION != null)
            {
                sTANDARD_SELECTION = TinyMapper.Map<Sals, STANDARD_SELECTION>(tmpsal, sTANDARD_SELECTION);
            }
            else
            {
                sTANDARD_SELECTION = TinyMapper.Map<STANDARD_SELECTION>(tmpsal);
            }
            sTANDARD_SELECTION.Assessment_Id = assessmentId;

            _context.Entry(sTANDARD_SELECTION).State = EntityState.Modified;
            LevelManager lm = new LevelManager(assessmentId, _context);
            lm.SaveOtherLevels(assessmentId, tmpsal);
            lm.Init(sTANDARD_SELECTION);
            if (tmpsal.SelectedSALOverride)
            {
                lm.SaveSALLevel(tmpsal.Selected_Sal_Level);
            }

            try
            {
                _context.SaveChanges();

                StandardRepository sr = new StandardRepository(_standard, _assessmentModeData, _context, _assessmentUtil, _standardRepo);
                sr.InitializeStandardRepository(assessmentId);
                sr.Confidence_Level = tmpsal.CLevel;
                sr.Integrity_Level = tmpsal.ILevel;
                sr.Availability_Level = tmpsal.ALevel;

                // save the newly-calculated overall value
                if (!tmpsal.SelectedSALOverride)
                {
                    tmpsal.Selected_Sal_Level = sr.Selected_Sal_Level;
                    lm.SaveSALLevel(tmpsal.Selected_Sal_Level);
                }

                return Ok(tmpsal);
            }
            catch (DbUpdateConcurrencyException dbe)
            {
                if (!STANDARD_SELECTIONExists(assessmentId))
                {
                    return NotFound();
                }
                else
                {
                    NLog.LogManager.GetCurrentClassLogger().Error($"... {dbe}");

                    throw;
                }
            }
            catch (Exception e)
            {
                BadRequest(e);
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a new STANDARD_SELECTION record for the assessment.
        /// </summary>
        /// <param name="sTANDARD_SELECTION">The STANDARD_SELECTION object to create</param>
        /// <returns>
        /// 201 Created with the created record if successful
        /// 400 Bad Request if model state is invalid
        /// 409 Conflict if record already exists
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint creates a new STANDARD_SELECTION record:
        /// - Adds a new standard selection record
        /// - Handles duplicate record conflicts
        /// - Supports assessment standard setup
        /// - Enables standard selection management
        /// 
        /// The creation process:
        /// - Validates the STANDARD_SELECTION object
        /// - Adds the record to the database
        /// - Handles duplicate assessment conflicts
        /// - Returns the created record
        /// 
        /// Usage scenarios:
        /// - Initial assessment setup
        /// - Standard selection creation
        /// - Assessment configuration
        /// - New assessment initialization
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/Sal_what_is_this")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> PostSTANDARD_SELECTION(STANDARD_SELECTION sTANDARD_SELECTION)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.STANDARD_SELECTION.Add(sTANDARD_SELECTION);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");

                if (STANDARD_SELECTIONExists(sTANDARD_SELECTION.Assessment_Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = sTANDARD_SELECTION.Assessment_Id }, sTANDARD_SELECTION);
        }

        /// <summary>
        /// Retrieves NIST SAL data for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with NistModel containing NIST SAL data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves comprehensive NIST SAL information:
        /// - Information types and their SAL values
        /// - NIST SAL questions and answers
        /// - Special factors affecting SAL
        /// - NIST SAL methodology data
        /// 
        /// The NIST SAL system includes:
        /// - Information type categorization
        /// - NIST SAL question sets
        /// - Special factor considerations
        /// - NIST methodology calculations
        /// 
        /// NIST SAL features:
        /// - Information type-based SAL
        /// - Question-driven SAL determination
        /// - Special factor adjustments
        /// - NIST methodology compliance
        /// - Assessment-specific NIST data
        /// 
        /// The response includes:
        /// - Information types and SAL values
        /// - NIST questions and answers
        /// - Special factors and their impact
        /// - NIST methodology data
        /// 
        /// Usage scenarios:
        /// - NIST SAL configuration
        /// - Information type management
        /// - NIST question handling
        /// - Special factor consideration
        /// - NIST methodology implementation
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/SAL/NistData")]
        [ProducesResponseType(typeof(NistModel), 200)]
        [ProducesResponseType(401)]
        public NistModel GetNistData()
        {
            int assessmentId = _token.AssessmentForUser();
            NistSalBusiness nistSal = new NistSalBusiness(_context, _assessmentUtil, _token);
            NistModel rvalue = new NistModel()
            {
                models = nistSal.GetInformationTypes(assessmentId),
                questions = nistSal.GetNistQuestions(assessmentId),
                specialFactors = nistSal.GetSpecialFactors(assessmentId)
            };

            return rvalue;
        }

        /// <summary>
        /// Updates NIST SAL data for the assessment.
        /// </summary>
        /// <param name="updateValue">The NistSalModel containing NIST SAL updates</param>
        /// <returns>
        /// 200 OK with updated Sals object if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint updates NIST SAL values:
        /// - Updates information type SAL values
        /// - Recalculates overall NIST SAL
        /// - Persists NIST SAL changes
        /// - Returns updated SAL data
        /// 
        /// The NIST SAL update process:
        /// - Updates information type SAL values
        /// - Recalculates overall SAL level
        /// - Persists changes to database
        /// - Returns updated SAL information
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/SAL/NistData")]
        [ProducesResponseType(typeof(Sals), 200)]
        [ProducesResponseType(401)]
        public Sals PostNistData([FromBody] NistSalModel updateValue)
        {
            int assessmentId = _token.AssessmentForUser();
            NistSalBusiness nistSal = new NistSalBusiness(_context, _assessmentUtil, _token);
            return nistSal.UpdateSalValue(updateValue, assessmentId);
        }

        /// <summary>
        /// Saves NIST SAL questions and answers for the assessment.
        /// </summary>
        /// <param name="updateValue">The NistQuestionsAnswers containing question updates</param>
        /// <returns>
        /// 200 OK with updated Sals object if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint saves NIST SAL questions and answers:
        /// - Updates NIST question responses
        /// - Recalculates SAL based on answers
        /// - Persists question answer data
        /// - Returns updated SAL information
        /// 
        /// The NIST questions save process:
        /// - Updates question answer data
        /// - Recalculates SAL levels
        /// - Persists changes to database
        /// - Returns updated SAL data
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/SAL/NistDataQuestions")]
        [ProducesResponseType(typeof(Sals), 200)]
        [ProducesResponseType(401)]
        public Sals PostNistDataQuestions([FromBody] NistQuestionsAnswers updateValue)
        {
            int assessmentId = _token.AssessmentForUser();
            NistSalBusiness nistSal = new NistSalBusiness(_context, _assessmentUtil, _token);
            return nistSal.SaveNistQuestions(assessmentId, updateValue);
        }

        /// <summary>
        /// Saves NIST SAL special factors for the assessment.
        /// </summary>
        /// <param name="updateValue">The NistSpecialFactor containing special factor updates</param>
        /// <returns>
        /// 200 OK with updated Sals object if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint saves NIST SAL special factors:
        /// - Updates special factor values
        /// - Recalculates SAL based on factors
        /// - Persists special factor data
        /// - Returns updated SAL information
        /// 
        /// The NIST special factors save process:
        /// - Updates special factor data
        /// - Recalculates SAL levels
        /// - Persists changes to database
        /// - Returns updated SAL data
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/SAL/NistDataSpecialFactor")]
        [ProducesResponseType(typeof(Sals), 200)]
        [ProducesResponseType(401)]
        public Sals PostNistDataSpecialFactor([FromBody] NistSpecialFactor updateValue)
        {
            int assessmentId = _token.AssessmentForUser();
            NistSalBusiness nistSal = new NistSalBusiness(_context, _assessmentUtil, _token);
            return nistSal.SaveNistSpecialFactor(assessmentId, updateValue);
        }

        /// <summary>
        /// Checks if a STANDARD_SELECTION record exists for the specified assessment ID.
        /// </summary>
        /// <param name="id">The assessment ID to check for STANDARD_SELECTION records</param>
        /// <returns>True if STANDARD_SELECTION record exists, false otherwise</returns>
        /// <remarks>
        /// This private method checks for the existence of STANDARD_SELECTION records:
        /// - Queries the STANDARD_SELECTION table
        /// - Counts records for the assessment ID
        /// - Returns existence status
        /// - Supports data validation
        /// 
        /// The existence check:
        /// - Counts STANDARD_SELECTION records
        /// - Filters by assessment ID
        /// - Returns boolean result
        /// - Supports data validation
        /// </remarks>
        private bool STANDARD_SELECTIONExists(int id)
        {
            return _context.STANDARD_SELECTION.Count(e => e.Assessment_Id == id) > 0;
        }
    }
}
