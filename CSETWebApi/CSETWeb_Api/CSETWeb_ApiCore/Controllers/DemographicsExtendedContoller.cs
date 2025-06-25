//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
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
using Nelibur.ObjectMapper;

using Microsoft.AspNetCore.Authorization;
using CSETWebCore.Business.Demographic;

namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for extended demographic functionality in CSET.
    /// This controller handles comprehensive demographic data collection, geographic information,
    /// sector and subsector management, and extended demographic options for cybersecurity assessments.
    /// Supports geographic selections, employee ranges, customer ranges, and organizational structure data.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class DemographicsExtendedController : ControllerBase
    {
        private readonly ITokenManager _token;
        private readonly IAssessmentBusiness _assessment;
        private readonly IAssessmentUtil _assessmentUtil;
        private readonly IDemographicBusiness _demographic;
        private CSETContext _context;

        /// <summary>
        /// Initializes a new instance of the DemographicsExtendedController.
        /// </summary>
        /// <param name="token">The token manager for user authentication and assessment context</param>
        /// <param name="assessment">The assessment business service for assessment operations</param>
        /// <param name="assessmentUtil">The assessment utility service for assessment operations</param>
        /// <param name="demographic">The demographic business service for demographic operations</param>
        /// <param name="context">The database context for data access operations</param>
        public DemographicsExtendedController(ITokenManager token, IAssessmentBusiness assessment, IAssessmentUtil assessmentUtil,
            IDemographicBusiness demographic, CSETContext context)
        {
            _token = token;
            _assessment = assessment;
            _assessmentUtil = assessmentUtil;
            _demographic = demographic;
            _context = context;
        }

        /// <summary>
        /// Gets the extended demographic answers for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with ExtendedDemographic containing extended demographic data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves comprehensive extended demographic information:
        /// - Customer support information
        /// - Cyber risk service details
        /// - CIO and CISO existence and status
        /// - Employee counts and geographic scope
        /// - Cyber training program information
        /// - Sector and subsector classifications
        /// 
        /// The response includes:
        /// - Complete extended demographic structure
        /// - Organizational leadership information
        /// - Employee and customer data
        /// - Geographic scope and training details
        /// - Sector and industry classifications
        /// 
        /// Extended demographic features:
        /// - Comprehensive organizational profiling
        /// - Leadership structure identification
        /// - Employee and customer metrics
        /// - Geographic scope analysis
        /// - Training program assessment
        /// 
        /// The demographic data supports:
        /// - Assessment customization and tailoring
        /// - Organizational structure analysis
        /// - Geographic and demographic benchmarking
        /// - Training and leadership assessment
        /// - Sector-specific analysis and reporting
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/ext")]
        [ProducesResponseType(typeof(ExtendedDemographic), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetExtendedDemographics()
        {
            int assessmentId = _token.AssessmentForUser();

            var demoBiz = new DemographicBusiness(_context, _assessmentUtil);
            var resp = demoBiz.GetExtendedDemographics(assessmentId);

            return Ok(resp);
        }

        /// <summary>
        /// Gets the persisted Region / County / Metro selections.
        /// </summary>
        /// <returns>
        /// 200 OK with GeographicSelections containing geographic data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves geographic selections for the assessment:
        /// - Region selections with state information
        /// - County FIPS codes and selections
        /// - Metro area FIPS codes and selections
        /// - Geographic scope and coverage data
        /// 
        /// The response includes:
        /// - Region codes and state information
        /// - County FIPS codes for selected counties
        /// - Metro FIPS codes for selected metro areas
        /// - Geographic scope and coverage details
        /// 
        /// Geographic features:
        /// - Multi-level geographic selection
        /// - Region, county, and metro coverage
        /// - FIPS code standardization
        /// - Geographic scope identification
        /// 
        /// The geographic data supports:
        /// - Geographic scope analysis
        /// - Regional benchmarking and comparison
        /// - Geographic risk assessment
        /// - Regional compliance tracking
        /// - Geographic reporting and analytics
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/ext/geographics")]
        [ProducesResponseType(typeof(GeographicSelections), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetGeographics()
        {
            int assessmentId = _token.AssessmentForUser();

            var resp = new GeographicSelections();

            _context.REGION_ANSWERS.Where(x => x.Assessment_Id == assessmentId).ToList().ForEach(x =>
            {
                resp.Regions.Add(new GeoRegion() { RegionCode = x.RegionCode, State = x.State });
            });

            _context.COUNTY_ANSWERS.Where(x => x.Assessment_Id == assessmentId).ToList().ForEach(x =>
            {
                resp.CountyFips.Add(x.County_FIPS);
            });

            _context.METRO_ANSWERS.Where(x => x.Assessment_Id == assessmentId).ToList().ForEach(x =>
            {
                resp.MetroFips.Add(x.Metro_FIPS);
            });

            return Ok(resp);
        }

        /// <summary>
        /// Returns Cyber Florida sector list.
        /// </summary>
        /// <returns>
        /// 200 OK with list of sectors
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides Cyber Florida sector options:
        /// - Complete sector list for Cyber Florida
        /// - Sorted sector names and identifiers
        /// - Sector selection options
        /// - Cyber Florida specific sectors
        /// 
        /// The response includes:
        /// - List of available sectors
        /// - Sector IDs and names
        /// - Sorted sector organization
        /// - Cyber Florida specific data
        /// 
        /// Sector features:
        /// - Cyber Florida specific sectors
        /// - Sorted sector organization
        /// - Sector selection support
        /// - Assessment customization
        /// 
        /// The sector data supports:
        /// - Cyber Florida assessment customization
        /// - Sector-specific analysis and reporting
        /// - Assessment tailoring and configuration
        /// - Sector-based benchmarking
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/ext/sectors")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetSectors()
        {
            var list = _context.EXT_SECTOR.OrderBy(x => x.SectorName).ToList();
            return Ok(list);
        }

        /// <summary>
        /// Retrieves subsectors for a specific sector ID.
        /// </summary>
        /// <param name="sectorId">The sector ID to get subsectors for</param>
        /// <returns>
        /// 200 OK with list of subsectors for the specified sector
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides subsector options for sector selection:
        /// - Subsector list based on parent sector
        /// - Hierarchical sector organization
        /// - Dynamic subsector filtering
        /// - Sector-specific subsector options
        /// 
        /// The response includes:
        /// - List of available subsectors
        /// - Subsector IDs and names
        /// - Sector-specific organization
        /// - Hierarchical relationship data
        /// 
        /// Subsector features:
        /// - Dynamic sector-based filtering
        /// - Hierarchical sector organization
        /// - Sector-specific subsector options
        /// - Assessment customization support
        /// 
        /// The subsector data supports:
        /// - Sector-specific assessment tailoring
        /// - Industry classification and analysis
        /// - Assessment customization
        /// - Sector-based reporting and analytics
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/ext/subsector/{sectorId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetSubsector(int sectorId)
        {
            var list = _context.EXT_SUB_SECTOR.Where(x => x.SectorId == sectorId).OrderBy(x => x.SubSectorName).ToList();
            return Ok(list);
        }

        /// <summary>
        /// Returns region list for a specific state.
        /// </summary>
        /// <param name="state">The state code to get regions for</param>
        /// <returns>
        /// 200 OK with list of regions and counties for the specified state
        /// </returns>
        /// <remarks>
        /// This endpoint provides region and county data for state selection:
        /// - Region list for specified state
        /// - County data within each region
        /// - FIPS codes and geographic information
        /// - State-specific geographic organization
        /// 
        /// The response includes:
        /// - List of regions for the state
        /// - County data within each region
        /// - FIPS codes and geographic identifiers
        /// - State-specific organization
        /// 
        /// Geographic features:
        /// - State-specific region organization
        /// - County-region relationships
        /// - FIPS code standardization
        /// - Geographic hierarchy support
        /// 
        /// The geographic data supports:
        /// - State-specific assessment customization
        /// - Regional analysis and reporting
        /// - Geographic scope identification
        /// - Regional benchmarking
        /// 
        /// This endpoint is marked as AllowAnonymous for public access.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet]
        [Route("api/demographics/ext/regions/{state}")]
        [ProducesResponseType(200)]
        public IActionResult GetRegionList(string state)
        {
            var list = new List<Model.Demographic.StateRegion>();
            TinyMapper.Bind<STATE_REGION, Model.Demographic.StateRegion>();

            var dbCounties = _context.COUNTIES.Where(x => x.State == state).OrderBy(x => x.CountyName).ToList();

            var dbRegions = _context.STATE_REGION.Where(x => x.State == state).OrderBy(x => x.RegionName).ToList();
            dbRegions.ForEach(x =>
            {
                var r = TinyMapper.Map<Model.Demographic.StateRegion>(x);

                foreach (var c in dbCounties.Where(x => x.RegionCode == r.RegionCode).ToList())
                {
                    r.Counties.Add(new County()
                    {
                        FIPS = c.County_FIPS,
                        Name = c.CountyName,
                        State = c.State
                    });
                }

                list.Add(r);
            });

            return Ok(list);
        }

        /// <summary>
        /// Returns county list for a specific state.
        /// </summary>
        /// <param name="state">The state code to get counties for</param>
        /// <returns>
        /// 200 OK with list of counties for the specified state
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides county data for state selection:
        /// - County list for specified state
        /// - County names and FIPS codes
        /// - State-specific county organization
        /// - Geographic county data
        /// 
        /// The response includes:
        /// - List of counties for the state
        /// - County names and identifiers
        /// - FIPS codes and geographic data
        /// - State-specific organization
        /// 
        /// County features:
        /// - State-specific county organization
        /// - FIPS code standardization
        /// - Geographic county data
        /// - County selection support
        /// 
        /// The county data supports:
        /// - State-specific assessment customization
        /// - County-level analysis and reporting
        /// - Geographic scope identification
        /// - County-based benchmarking
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/ext/counties/{state}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetCountyList(string state)
        {
            var list = _context.COUNTIES.Where(x => x.State == state).ToList();
            return Ok(list);
        }

        /// <summary>
        /// Returns all known metro areas for Florida "12-*".
        /// TODO: Make this smarter to know the Florida FIPS (12) so that
        /// the query can be run for any state.
        /// </summary>
        /// <param name="state">The state code to get metro areas for</param>
        /// <returns>
        /// 200 OK with list of metro areas for the specified state
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides metro area data for state selection:
        /// - Metro area list for specified state
        /// - Metro FIPS codes and geographic information
        /// - County-metro area relationships
        /// - State-specific metro organization
        /// 
        /// The response includes:
        /// - List of metro areas for the state
        /// - Metro FIPS codes and identifiers
        /// - County-metro area relationships
        /// - State-specific organization
        /// 
        /// Metro features:
        /// - State-specific metro organization
        /// - FIPS code standardization
        /// - County-metro relationships
        /// - Metro area selection support
        /// 
        /// The metro data supports:
        /// - State-specific assessment customization
        /// - Metro-level analysis and reporting
        /// - Geographic scope identification
        /// - Metro-based benchmarking
        /// 
        /// Note: Currently optimized for Florida (FIPS 12) but designed for expansion.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/ext/metros/{state}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetMetroList(string state)
        {
            var list = _context.METRO_AREA
                .Include(x => x.COUNTY_METRO_AREA)
                .Where(x => x.Metro_FIPS.StartsWith("12-"))
                .ToList();

            return Ok(list);
        }

        /// <summary>
        /// Returns employee range options for demographic selection.
        /// </summary>
        /// <returns>
        /// 200 OK with list of employee range options
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides employee range options:
        /// - Predefined employee count ranges
        /// - Standardized employee size categories
        /// - Employee range selection options
        /// - Demographic size classification
        /// 
        /// The response includes:
        /// - List of employee range options
        /// - Range IDs and descriptive values
        /// - Standardized size categories
        /// - Employee count classifications
        /// 
        /// Employee range features:
        /// - Standardized size categories
        /// - Employee count classifications
        /// - Range selection support
        /// - Demographic size analysis
        /// 
        /// The employee range data supports:
        /// - Organization size classification
        /// - Employee-based assessment customization
        /// - Size-based benchmarking
        /// - Demographic analysis and reporting
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/ext/employees")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetEmployeeRanges()
        {
            var list = new List<ListItem>();

            list.Add(new ListItem() { Id = 1, Value = "< 100" });
            list.Add(new ListItem() { Id = 2, Value = "101 - 250" });
            list.Add(new ListItem() { Id = 3, Value = "251 - 500" });
            list.Add(new ListItem() { Id = 4, Value = "501 - 1,000" });
            list.Add(new ListItem() { Id = 5, Value = "1,001 - 5,000" });
            list.Add(new ListItem() { Id = 6, Value = "5,001 - 10,000" });
            list.Add(new ListItem() { Id = 7, Value = "10,000 - 25,000" });
            list.Add(new ListItem() { Id = 8, Value = "> 25,000" });

            return Ok(list);
        }

        [HttpGet]
        [Route("api/demographics/ext/customers")]
        public IActionResult GetCustomerRanges()
        {
            var list = new List<ListItem>();

            list.Add(new ListItem() { Id = 1, Value = "< 100" });
            list.Add(new ListItem() { Id = 2, Value = "101 - 500" });
            list.Add(new ListItem() { Id = 3, Value = "501 - 2,500" });
            list.Add(new ListItem() { Id = 4, Value = "2,501 - 10,000" });
            list.Add(new ListItem() { Id = 5, Value = "10,001 - 50,000" });
            list.Add(new ListItem() { Id = 6, Value = "50,001 - 250,000" });
            list.Add(new ListItem() { Id = 7, Value = "250,001 - 1,000,000" });
            list.Add(new ListItem() { Id = 8, Value = "1,100,001 - 5,000,000" });
            list.Add(new ListItem() { Id = 9, Value = "> 5,000,001" });

            return Ok(list);
        }

        [HttpGet]
        [Route("api/demographics/ext/geoscope")]
        public IActionResult GetGeographicScope()
        {
            var list = new List<ListItem>();

            list.Add(new ListItem() { Id = 1, Value = "No Impact" });
            list.Add(new ListItem() { Id = 2, Value = "Local (Municipality or Single County)" });
            list.Add(new ListItem() { Id = 3, Value = "Area (More than one county and less than 50% of the state)" });
            list.Add(new ListItem() { Id = 4, Value = "Statewide" });

            return Ok(list);
        }

        [HttpGet]
        [Route("api/demographics/ext/cio")]
        public IActionResult GetCioOptions()
        {
            var list = new List<ListItem>();

            list.Add(new ListItem() { Id = 1, Value = "Yes, Full-Time" });
            list.Add(new ListItem() { Id = 2, Value = "Yes, Part-Time" });
            list.Add(new ListItem() { Id = 3, Value = "No" });

            return Ok(list);
        }


        [HttpGet]
        [Route("api/demographics/ext/ciso")]
        public IActionResult GetCisoOptions()
        {
            var list = new List<ListItem>();

            list.Add(new ListItem() { Id = 1, Value = "Yes, Full-Time" });
            list.Add(new ListItem() { Id = 2, Value = "Yes, Part-Time" });
            list.Add(new ListItem() { Id = 3, Value = "No" });

            return Ok(list);
        }


        [HttpGet]
        [Route("api/demographics/ext/training")]
        public IActionResult GetTrainingOptions()
        {
            var list = new List<ListItem>();

            list.Add(new ListItem() { Id = 1, Value = "Yes" });
            list.Add(new ListItem() { Id = 2, Value = "No" });

            return Ok(list);
        }


        /// <summary>
        /// Persists extended demographics.
        /// </summary>
        /// <param name="demographics"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/demographics/ext")]
        public IActionResult PostExtended([FromBody] ExtendedDemographic demographics)
        {
            demographics.AssessmentId = _token.AssessmentForUser();

            return Ok(_demographic.SaveDemographics(demographics));
        }


        /// <summary>
        /// Persists the selected region/county/metro areas.
        /// </summary>
        [HttpPost]
        [Route("api/demographics/ext/geographics")]
        public IActionResult PostGeographicLocations([FromBody] GeographicSelections geographics)
        {
            int assessmentId = _token.AssessmentForUser();

            // clean out all existing selections
            var ra = _context.REGION_ANSWERS.Where(x => x.Assessment_Id == assessmentId).ToList();
            _context.REGION_ANSWERS.RemoveRange(ra);
            var ca = _context.COUNTY_ANSWERS.Where(x => x.Assessment_Id == assessmentId).ToList();
            _context.COUNTY_ANSWERS.RemoveRange(ca);
            var ma = _context.METRO_ANSWERS.Where(x => x.Assessment_Id == assessmentId).ToList();
            _context.METRO_ANSWERS.RemoveRange(ma);
            _context.SaveChanges();

            // region_answers
            geographics.Regions.ForEach(r =>
            {
                _context.REGION_ANSWERS.Add(new REGION_ANSWERS()
                {
                    Assessment_Id = assessmentId,
                    State = r.State,
                    RegionCode = r.RegionCode
                });
            });

            // county_answers
            geographics.CountyFips.ForEach(c =>
            {
                _context.COUNTY_ANSWERS.Add(new COUNTY_ANSWERS()
                {
                    Assessment_Id = assessmentId,
                    County_FIPS = c
                });
            });

            // metro_answers
            geographics.MetroFips.ForEach(m =>
            {
                _context.METRO_ANSWERS.Add(new METRO_ANSWERS()
                {
                    Assessment_Id = assessmentId,
                    Metro_FIPS = m
                });
            });

            _context.SaveChanges();

            return Ok();
        }


        /// <summary>
        /// Returns a true if all extended demographics have been answered,
        /// otherwise a false is returned.
        /// </summary>
        [HttpGet]
        [Route("api/demographics/ext/demoanswered")]
        public IActionResult AreExtendedDemographicsAnswered()
        {
            int assessmentId = _token.AssessmentForUser();

            var demo = _context.DEMOGRAPHIC_ANSWERS.Where(x => x.Assessment_Id == assessmentId).FirstOrDefault();

            if (demo == null)
            {
                demo = new DEMOGRAPHIC_ANSWERS();
            }

            if (demo.SectorId == null
                || demo.SubSectorId == null
                || demo.Employees == null
                || demo.CustomersSupported == null
                || demo.CIOExists == null
                || demo.CISOExists == null
                || demo.CyberTrainingProgramExists == null
                || demo.GeographicScope == null)
            {
                return Ok(false);
            }

            var regions = _context.REGION_ANSWERS.Where(x => x.Assessment_Id == assessmentId).Count();
            var counties = _context.COUNTY_ANSWERS.Where(x => x.Assessment_Id == assessmentId).Count();
            var metros = _context.METRO_ANSWERS.Where(x => x.Assessment_Id == assessmentId).Count();

            if (regions == 0 && counties == 0 && metros == 0)
            {
                return Ok(false);
            }


            return Ok(true);
        }
    }
}
