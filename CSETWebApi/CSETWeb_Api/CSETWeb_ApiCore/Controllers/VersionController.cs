using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.User;
using CSETWebCore.Interfaces.Version;
using CSETWebCore.Business.Version;
using Microsoft.AspNetCore.Mvc;
using NLog;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for version information functionality in CSET.
    /// This controller handles the retrieval of CSET version information
    /// for system identification and compatibility checking. Provides
    /// version numbers and system information for client applications.
    /// </summary>
    public class VersionController : Controller
    {
        private readonly IVersionBusiness _versionBusiness;
        private readonly CSETContext _context;
        //IVersionBusiness versionBusiness

        /// <summary>
        /// Initializes a new instance of the VersionController.
        /// </summary>
        /// <param name="context">Database context for version operations</param>
        /// <param name="versionBusiness">Version business logic service</param>
        public VersionController(CSETContext context, IVersionBusiness versionBusiness)
        {
            _context = context;

            _versionBusiness = versionBusiness;

        }

        /// <summary>
        /// Retrieves the current CSET version number.
        /// </summary>
        /// <returns>
        /// 200 OK with version number if successful
        /// 200 OK with error message if version retrieval fails
        /// </returns>
        /// <remarks>
        /// Returns the current version number of the CSET application.
        /// Used by client applications to check version compatibility
        /// and display version information to users. Logs errors to
        /// database for debugging purposes. Returns error message as
        /// string if version retrieval fails.
        /// </remarks>
        [HttpGet]
        [Route("api/version")]
        public IActionResult GetCsetVersion()
        {
            try
            {
                var v = _versionBusiness.GetVersionNumber();
                return Ok(v);
            }
            catch (Exception exc)
            {
                var logToDb = LogManager.GetCurrentClassLogger();
                logToDb.Error(exc.ToString());
                return Ok(exc.ToString());
            }
        }
    }
}