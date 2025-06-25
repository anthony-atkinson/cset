//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 

using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.Business.Authorization;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Assessment;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Assessment;
using CSETWebCore.Model.Demographic;
using Microsoft.EntityFrameworkCore;
using CSETWebCore.Helpers;


namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing assessment demographic information in CSET.
    /// Supports demographic data collection, organization types, sectors, industries,
    /// asset values, and geographic information for cybersecurity assessments.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class DemographicsController : ControllerBase
    {
        private readonly ITokenManager _token;
        private readonly IAssessmentBusiness _assessment;
        private readonly IDemographicBusiness _demographic;
        private CSETContext _context;

        private readonly TranslationOverlay _overlay;


        /// <summary>
        /// Initializes a new instance of the DemographicsController.
        /// </summary>
        /// <param name="token">Service for JWT token management</param>
        /// <param name="assessment">Service for assessment operations</param>
        /// <param name="demographic">Service for demographic operations</param>
        /// <param name="context">Database context</param>
        public DemographicsController(ITokenManager token, IAssessmentBusiness assessment,
            IDemographicBusiness demographic, CSETContext context)
        {
            _token = token;
            _assessment = assessment;
            _demographic = demographic;
            _context = context;

            _overlay = new TranslationOverlay();
        }


        /// <summary>
        /// Retrieves demographic information for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with assessment demographics data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves the complete demographic information for the current assessment,
        /// including organization details, sector information, asset values, and geographic data.
        /// 
        /// The response includes:
        /// - Organization name and type
        /// - Sector and industry classification
        /// - Asset values and criticality
        /// - Geographic location information
        /// - Organization size and characteristics
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics")]
        [ProducesResponseType(typeof(Demographics), 200)]
        [ProducesResponseType(401)]
        public IActionResult Get()
        {
            int assessmentId = _token.AssessmentForUser();
            return Ok(_demographic.GetDemographics(assessmentId));
        }


        /// <summary>
        /// Saves demographic information for the current assessment.
        /// </summary>
        /// <param name="demographics">Demographic data to save</param>
        /// <returns>
        /// 200 OK with saved demographics data
        /// 400 Bad Request if demographic data is invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint saves or updates the demographic information for the current assessment.
        /// The assessment ID is automatically set from the current user's context.
        /// 
        /// The request should include:
        /// - Organization information
        /// - Sector and industry classification
        /// - Asset values and criticality
        /// - Geographic location data
        /// - Organization size and characteristics
        /// 
        /// Sample request:
        ///     POST /api/demographics
        ///     {
        ///         "OrganizationName": "Example Corp",
        ///         "OrganizationType": "Private",
        ///         "SectorId": 1,
        ///         "IndustryId": 5,
        ///         "AssetValue": "High",
        ///         "Size": "Large"
        ///     }
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/demographics")]
        [ProducesResponseType(typeof(Demographics), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult Post([FromBody] Demographics demographics)
        {
            demographics.AssessmentId = _token.AssessmentForUser();
            return Ok(_demographic.SaveDemographics(demographics));
        }


        /// <summary>
        /// Retrieves a list of available organization types.
        /// </summary>
        /// <returns>
        /// 200 OK with list of organization types
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides a list of all available organization types that can be
        /// assigned to assessments. These types help categorize organizations for
        /// reporting and analysis purposes.
        /// 
        /// Common organization types include:
        /// - Private Sector
        /// - Public Sector
        /// - Government
        /// - Non-Profit
        /// - Educational
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/getOrganizationTypes")]
        [ProducesResponseType(typeof(List<OrganizationType>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetOrganizationTypes()
        {
            return Ok(_assessment.GetOrganizationTypes());
        }


        /// <summary>
        /// Retrieves sectors applicable to the current scope (base or IOD).
        /// </summary>
        /// <returns>
        /// 200 OK with list of applicable sectors
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves sectors based on the current application scope.
        /// For IOD (Infrastructure and Operational Dependencies) scope, it returns
        /// NIPP (National Infrastructure Protection Plan) sectors. For base scope,
        /// it returns classic CISA sectors.
        /// 
        /// The response includes:
        /// - Sector ID and name
        /// - Scope-appropriate sector list
        /// - Translated names based on user language preference
        /// 
        /// Sectors are used to classify organizations by their critical infrastructure
        /// sector for cybersecurity assessment and reporting purposes.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/Demographics/Sectors")]
        [ProducesResponseType(typeof(List<Sector>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetSECTORs()
        {
            string scope = _token.Payload("scope");


            var list = await _context.SECTOR.ToListAsync<SECTOR>();


            // For now, based on the scope/skin, show either the
            // classic CISA sectors or the NIPP sectors (for IOD).
            // If NIPP becomes the preferred for all, code will
            // be added to convert CISA to NIPP on the fly.
            if (scope == "IOD")
            {
                list.RemoveAll(x => !x.Is_NIPP);
            }
            else
            {
                list.RemoveAll(x => x.Is_NIPP);
            }

            var otherItems = list.Where(x => x.SectorName.Equals("other", System.StringComparison.CurrentCultureIgnoreCase)).ToList();
            foreach (var o in otherItems)
            {
                list.Remove(o);
                list.Add(o);
            }


            // translate if not running in english.  
            var lang = _token.GetCurrentLanguage();
            if (lang != "en")
            {
                list.ForEach(x =>
                {
                    var val = _overlay.GetValue("SECTOR", x.SectorId.ToString(), lang)?.Value;
                    if (val != null)
                    {
                        x.SectorName = val;
                    }
                });
            }


            return Ok(list.Select(s => new Sector { SectorId = s.SectorId, SectorName = s.SectorName }).ToList());
        }


        /// <summary>
        /// Retrieves all sector-industry relationships.
        /// </summary>
        /// <returns>
        /// 200 OK with all sector-industry mappings
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all sector-industry relationships in the system.
        /// This data is used to understand which industries belong to which sectors
        /// for assessment classification and reporting.
        /// 
        /// The response includes:
        /// - All sector-industry mappings
        /// - Industry IDs and names
        /// - Sector associations
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/Demographics/Sectors_Industry")]
        [ProducesResponseType(typeof(IQueryable<SECTOR_INDUSTRY>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetSECTOR_INDUSTRY()
        {
            var list = _context.SECTOR_INDUSTRY;
            return Ok(list);
        }


        /// <summary>
        /// Retrieves industries for a specific sector.
        /// </summary>
        /// <param name="id">Sector ID to get industries for</param>
        /// <returns>
        /// 200 OK with list of industries for the specified sector
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all industries that belong to a specific sector.
        /// Industries are ordered alphabetically with "Other" options moved to the end.
        /// 
        /// The response includes:
        /// - Industry ID and name
        /// - Associated sector ID
        /// - Translated names based on user language preference
        /// - Ordered list with "Other" options at the end
        /// 
        /// Sample request:
        ///     GET /api/Demographics/Sectors_Industry/1
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/Demographics/Sectors_Industry/{id}")]
        [ProducesResponseType(typeof(List<Industry>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetSECTOR_INDUSTRY(int id)
        {
            var list = await _context.SECTOR_INDUSTRY.Where(x => x.SectorId == id)
                .OrderBy(a => a.IndustryName).ToListAsync<SECTOR_INDUSTRY>();

            var otherItems = list.Where(x => x.Is_Other).ToList();
            foreach (var o in otherItems)
            {
                list.Remove(o);
                list.Add(o);
            }


            // translate if not running in english.  
            var lang = _token.GetCurrentLanguage();
            if (lang != "en")
            {
                list.ForEach(x =>
                {
                    var val = _overlay.GetValue("SECTOR_INDUSTRY", x.IndustryId.ToString(), lang)?.Value;
                    if (val != null)
                    {
                        x.IndustryName = val;
                    }
                });
            }

            return Ok(list.Select(x => new Industry() { IndustryId = x.IndustryId, IndustryName = x.IndustryName, SectorId = x.SectorId }).ToList());
        }


        /// <summary>
        /// Retrieves available asset values for demographic classification.
        /// </summary>
        /// <returns>
        /// 200 OK with list of asset values
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves the available asset values that can be assigned to
        /// organizations for assessment purposes. Asset values help determine the
        /// criticality and importance of the organization's assets.
        /// 
        /// The response includes:
        /// - Asset value descriptions
        /// - Ordered by value priority
        /// - Used for risk assessment and prioritization
        /// 
        /// Common asset values include:
        /// - High Value
        /// - Medium Value
        /// - Low Value
        /// - Critical
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/Demographics/AssetValues")]
        [ProducesResponseType(typeof(List<DemographicsAssetValue>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAssetValues()
        {
            List<DEMOGRAPHICS_ASSET_VALUES> assetValues = await _context.DEMOGRAPHICS_ASSET_VALUES
                .ToListAsync();
            return Ok(assetValues.OrderBy(a => a.ValueOrder).Select(a => new DemographicsAssetValue() { AssetValue = a.AssetValue, DemographicsAssetId = a.DemographicsAssetId }).ToList());
        }


        /// <summary>
        /// Retrieves states and provinces for geographic classification.
        /// </summary>
        /// <returns>
        /// 200 OK with list of states and provinces
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all available states and provinces for geographic
        /// classification of assessments. The list is ordered alphabetically by
        /// display name and includes international locations.
        /// 
        /// The response includes:
        /// - State/province ID and display name
        /// - ISO country codes
        /// - Translated names based on user language preference
        /// - Ordered alphabetically
        /// 
        /// This data is used for:
        /// - Geographic risk assessment
        /// - Regional compliance requirements
        /// - Location-based reporting
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/Demographics/StatesAndProvinces")]
        [ProducesResponseType(typeof(List<StateAndProvince>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetStatesAndProvinces()
        {
            List<STATES_AND_PROVINCES> statesAndProvinces = await _context.STATES_AND_PROVINCES.ToListAsync();

            // translate if not running in english
            var lang = _token.GetCurrentLanguage();
            if (lang != "en")
            {
                statesAndProvinces.ForEach(x =>
                {
                    var val = _overlay.GetValue("STATES_AND_PROVINCES", x.STATES_AND_PROVINCES_ID.ToString(), lang)?.Value;
                    if (val != null)
                    {
                        x.Display_Name = val;
                    }
                });
            }

            return Ok(statesAndProvinces.OrderBy(s => s.Display_Name).Select(s => new StateAndProvince() 
                { 
                    StateAndProvinceId = s.STATES_AND_PROVINCES_ID, 
                    ISOCode = s.ISO_code, 
                    CountryCode = s.Country_Code, 
                    DisplayName = s.Display_Name 
                }).ToList());
        }


        /// <summary>
        /// Retrieves organization size classifications.
        /// </summary>
        /// <returns>
        /// 200 OK with list of organization size options
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves the available organization size classifications
        /// that can be assigned to assessments. Size classifications help determine
        /// appropriate security controls and compliance requirements.
        /// 
        /// The response includes:
        /// - Size ID and description
        /// - Ordered by value priority
        /// - Translated descriptions based on user language preference
        /// 
        /// Common size classifications include:
        /// - Small (1-50 employees)
        /// - Medium (51-250 employees)
        /// - Large (251+ employees)
        /// - Enterprise (1000+ employees)
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/Demographics/Size")]
        [ProducesResponseType(typeof(List<AssessmentSize>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetSize()
        {
            List<DEMOGRAPHICS_SIZE> assetValues = await _context.DEMOGRAPHICS_SIZE.ToListAsync();


            // translate if not running in english
            var lang = _token.GetCurrentLanguage();
            if (lang != "en")
            {
                assetValues.ForEach(x =>
                {
                    var val = _overlay.GetValue("DEMOGRAPHICS_SIZE", x.DemographicId.ToString(), lang)?.Value;
                    if (val != null)
                    {
                        x.Description = val;
                    }
                });
            }


            return Ok(assetValues.OrderBy(a => a.ValueOrder).Select(s => new AssessmentSize() { DemographicId = s.DemographicId, Description = s.Description, Size = s.Size }).ToList());
        }
    }
}
