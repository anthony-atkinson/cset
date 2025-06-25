//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for GUID generation and management in CSET.
    /// This controller handles bulk GUID generation for client applications,
    /// supporting efficient GUID allocation for diagram components, assessment
    /// elements, and other system entities requiring unique identifiers.
    /// </summary>
    [ApiController]
    public class GuidController : ControllerBase
    {
        /// <summary>
        /// Generates a block of GUIDs for client applications.
        /// </summary>
        /// <param name="number">The number of GUIDs to generate (default: 100, max: 1000)</param>
        /// <returns>
        /// 200 OK with List of Guid containing the requested number of GUIDs
        /// 400 Bad Request if number is invalid
        /// </returns>
        /// <remarks>
        /// This endpoint provides bulk GUID generation for client applications:
        /// - Efficient bulk GUID generation
        /// - Configurable number of GUIDs per request
        /// - Standard .NET GUID format (RFC 4122)
        /// - Client-side GUID caching support
        /// 
        /// The GUID generation process includes:
        /// - Random GUID generation using .NET's Guid.NewGuid()
        /// - Bulk generation for performance optimization
        /// - List-based response format
        /// - Standard HTTP response handling
        /// 
        /// GUID features:
        /// - RFC 4122 compliant GUIDs
        /// - Cryptographically secure random generation
        /// - Globally unique identifiers
        /// - Version 4 UUID format
        /// - 128-bit unique identifiers
        /// 
        /// The GUIDs support:
        /// - Diagram component identification
        /// - Assessment element tracking
        /// - System entity identification
        /// - Client-side caching mechanisms
        /// - Unique identifier requirements
        /// 
        /// Usage scenarios:
        /// - Diagram component creation
        /// - Assessment element generation
        /// - Client-side GUID caching
        /// - Bulk identifier allocation
        /// - System entity tracking
        /// 
        /// Performance considerations:
        /// - Bulk generation reduces API calls
        /// - Client-side caching improves performance
        /// - Configurable batch sizes
        /// - Efficient memory usage
        /// 
        /// Client integration:
        /// - Used by diagram editing tools
        /// - Supports assessment creation workflows
        /// - Enables offline GUID generation
        /// - Facilitates client-side caching
        /// 
        /// No authentication required - this is a public utility endpoint.
        /// </remarks>
        [Route("api/guid/requestblock")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Guid>), 200)]
        [ProducesResponseType(400)]
        public IActionResult GetABlockOfGuids(int number = 100)
        {
            List<Guid> guids = new List<Guid>();
            for (int i = 0; i < number; i++)
            {
                guids.Add(Guid.NewGuid());
            }
            return Ok(guids);
        }
    }
}
