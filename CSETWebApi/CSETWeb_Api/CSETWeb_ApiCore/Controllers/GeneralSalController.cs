//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Sal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CSETWebCore.Business;
using CSETWebCore.Business.Authorization;
using CSETWebCore.Business.Sal;

namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for General SAL (Security Assurance Level) operations in CSET.
    /// This controller handles General SAL slider management, weight calculations, and
    /// SAL level determination for cybersecurity assessments. Supports the General SAL
    /// methodology for determining security assurance levels based on weighted criteria.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class GeneralSalController : ControllerBase
    {
        private readonly CSETContext _context;
        private readonly ITokenManager _token;
        private readonly IAssessmentUtil _assessmentUtil;

        /// <summary>
        /// Initializes a new instance of the GeneralSalController.
        /// </summary>
        /// <param name="context">The database context for SAL data access</param>
        /// <param name="token">The token manager for user authentication and assessment context</param>
        /// <param name="assessmentUtil">The assessment utility service for assessment operations</param>
        public GeneralSalController(CSETContext context, ITokenManager token, IAssessmentUtil assessmentUtil)
        {
            _context = context;
            _token = token;
            _assessmentUtil = assessmentUtil;
        }

        /// <summary>
        /// Retrieves General SAL descriptions, weights, and current slider values for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with List of GenSalPairs containing SAL descriptions and weights
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves General SAL configuration and current values:
        /// - SAL descriptions and metadata
        /// - Weight values and display options
        /// - Current slider positions for the assessment
        /// - On-site and off-site SAL pairs
        /// 
        /// The General SAL system includes:
        /// - SAL descriptions and ordering
        /// - Weight values and calculations
        /// - Slider-based value selection
        /// - On-site and off-site considerations
        /// 
        /// SAL features:
        /// - Configurable SAL descriptions
        /// - Weight-based calculations
        /// - Slider value management
        /// - Assessment-specific values
        /// - On-site/off-site pairing
        /// 
        /// The response includes:
        /// - List of GenSalPairs (on-site/off-site pairs)
        /// - SAL descriptions and metadata
        /// - Weight values and display options
        /// - Current slider positions
        /// - Assessment-specific values
        /// 
        /// SAL levels supported:
        /// - Low (1-254 weight)
        /// - Moderate (255-999 weight)
        /// - High (1000-19999 weight)
        /// - Very High (20000+ weight)
        /// 
        /// Usage scenarios:
        /// - General SAL configuration display
        /// - Slider value management
        /// - SAL level calculation
        /// - Assessment SAL setup
        /// - Weight-based SAL determination
        /// 
        /// The General SAL methodology:
        /// - Uses weighted criteria for SAL determination
        /// - Supports slider-based value selection
        /// - Calculates overall SAL based on weights
        /// - Provides on-site and off-site considerations
        /// - Enables flexible SAL configuration
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/GeneralSal/Descriptions")]
        [ProducesResponseType(typeof(List<GenSalPairs>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetGeneralSalDescriptionsWeights()
        {
            int assessmentid = _token.AssessmentForUser();

            TinyMapper.Bind<GENERAL_SAL_DESCRIPTIONS, GeneralSalDescriptionsWeights>();
            TinyMapper.Bind<GEN_SAL_WEIGHTS, GenSalWeights>();

            List<GenSalPairs> result = new List<GenSalPairs>();

            var sliders = from d in _context.GENERAL_SAL_DESCRIPTIONS
                          from g in _context.GENERAL_SAL.Where(g => g.Assessment_Id == assessmentid && g.Sal_Name == d.Sal_Name).DefaultIfEmpty()
                          orderby d.Sal_Order
                          select new GenSalCategory
                          {
                              d = d,
                              SliderValue = (int?)g.Slider_Value
                          };

            bool first = true;
            GenSalPairs pair = null;

            foreach (var slider in sliders.ToList())
            {
                GeneralSalDescriptionsWeights s = TinyMapper.Map<GeneralSalDescriptionsWeights>(slider.d);
                if (first)
                {
                    pair = new GenSalPairs();
                    pair.OnSite = s;
                    result.Add(pair);
                }
                else
                {
                    pair.OffSite = s;
                }
                first = !first;

                s.values = new List<string>();
                s.Slider_Value = slider.SliderValue ?? 0;
                foreach (GEN_SAL_WEIGHTS w in _context.GEN_SAL_WEIGHTS.Where(x => String.Equals(x.Sal_Name, slider.d.Sal_Name)))
                {
                    s.GEN_SAL_WEIGHTS.Add(TinyMapper.Map<GenSalWeights>(w));
                    s.values.Add(" " + w.Display + " ");
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Saves a General SAL weight and recalculates the overall SAL level.
        /// </summary>
        /// <param name="ws">The SaveWeight object containing the slider name and value to save</param>
        /// <returns>
        /// 200 OK with the calculated SAL level if weight saved successfully
        /// 400 Bad Request if the model state is invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint saves a General SAL weight and recalculates the overall SAL:
        /// - Saves the slider value to the database
        /// - Recalculates the overall SAL level
        /// - Updates the assessment with the new SAL
        /// - Returns the calculated SAL level
        /// 
        /// The save process includes:
        /// - Validation of the SaveWeight object
        /// - Database persistence of slider values
        /// - SAL level recalculation
        /// - Assessment update and touch
        /// 
        /// SAL calculation features:
        /// - Weight-based SAL determination
        /// - Threshold-based level assignment
        /// - Assessment-specific calculations
        /// - Database persistence
        /// - Assessment update tracking
        /// 
        /// The SaveWeight object contains:
        /// - Slider name for identification
        /// - Slider value to save
        /// - Assessment ID for context
        /// - Weight calculation data
        /// 
        /// SAL level calculation:
        /// - Sums all selected weight values
        /// - Applies threshold-based logic
        /// - Determines overall SAL level
        /// - Updates assessment record
        /// - Persists to database
        /// 
        /// Usage scenarios:
        /// - General SAL weight updates
        /// - SAL level recalculation
        /// - Assessment SAL management
        /// - Weight-based SAL determination
        /// - Real-time SAL updates
        /// 
        /// The calculation process:
        /// - Retrieves current weights from database
        /// - Calculates total weight value
        /// - Applies SAL threshold logic
        /// - Updates assessment SAL level
        /// - Returns calculated SAL value
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/GeneralSal/SaveWeight")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult PostSaveWeight(SaveWeight ws)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int assessmentid = _token.AssessmentForUser();
            ws.assessmentid = assessmentid;

            GeneralSalBusiness salManager = new GeneralSalBusiness(_context, _token, _assessmentUtil);
            string salvalue = salManager.SaveWeightAndCalculate(ws);

            return Ok(salvalue);
        }

        /// <summary>
        /// Retrieves the current saved SAL value for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with the current SAL level if successful
        /// 400 Bad Request if the model state is invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves the current saved SAL value:
        /// - Gets the saved SAL level from the database
        /// - Returns the current assessment SAL
        /// - Provides default value if not set
        /// - Supports SAL value display
        /// 
        /// The SAL retrieval process:
        /// - Queries the assessment's saved SAL level
        /// - Returns the current SAL value
        /// - Handles missing SAL records
        /// - Provides default SAL level
        /// 
        /// SAL value features:
        /// - Current assessment SAL level
        /// - Saved SAL value retrieval
        /// - Default value handling
        /// - Assessment-specific SAL
        /// - SAL level display support
        /// 
        /// The response includes:
        /// - Current SAL level (Low, Moderate, High, Very High)
        /// - Assessment-specific SAL value
        /// - Saved SAL determination
        /// - Default SAL handling
        /// 
        /// SAL level values:
        /// - Low: Default SAL level
        /// - Moderate: Medium security assurance
        /// - High: High security assurance
        /// - Very High: Very high security assurance
        /// 
        /// Usage scenarios:
        /// - Current SAL level display
        /// - SAL value verification
        /// - Assessment SAL retrieval
        /// - SAL level checking
        /// - Default SAL handling
        /// 
        /// The retrieval process:
        /// - Queries STANDARD_SELECTION table
        /// - Gets Selected_Sal_Level value
        /// - Returns current SAL level
        /// - Handles missing records gracefully
        /// - Provides default "Low" value
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/GeneralSal/Value")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult GetValue()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GeneralSalBusiness salManager = new GeneralSalBusiness(_context, _token, _assessmentUtil);

            int assessmentId = _token.AssessmentForUser();
            string salvalue = salManager.GetSavedSALValue(assessmentId);
            return Ok(salvalue);
        }

        /// <summary>
        /// Checks if General SAL records exist for the specified assessment ID.
        /// </summary>
        /// <param name="id">The assessment ID to check for General SAL records</param>
        /// <returns>True if General SAL records exist, false otherwise</returns>
        /// <remarks>
        /// This private method checks for the existence of General SAL records:
        /// - Queries the GENERAL_SAL table
        /// - Counts records for the assessment ID
        /// - Returns existence status
        /// - Supports SAL record validation
        /// 
        /// The existence check:
        /// - Counts GENERAL_SAL records
        /// - Filters by assessment ID
        /// - Returns boolean result
        /// - Supports data validation
        /// </remarks>
        private bool GENERAL_SALExists(int id)
        {
            return _context.GENERAL_SAL.Count(e => e.Assessment_Id == id) > 0;
        }
    }

    /// <summary>
    /// Internal class for organizing General SAL category data with slider values.
    /// </summary>
    public class GenSalCategory
    {
        /// <summary>
        /// The General SAL description data
        /// </summary>
        public GENERAL_SAL_DESCRIPTIONS d;
        
        /// <summary>
        /// The current slider value for this SAL category
        /// </summary>
        public int? SliderValue;
    }
}
