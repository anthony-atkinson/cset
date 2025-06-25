//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.GalleryParser;
using CSETWebCore.Business.Standards;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Maturity;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Interfaces.Standards;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for gallery state management in CSET.
    /// This controller handles gallery board structure retrieval and layout item management,
    /// supporting the dynamic gallery interface that displays assessment tools, standards,
    /// and maturity models in an organized card-based layout.
    /// </summary>
    [ApiController]
    public class GalleryStateController : ControllerBase
    {
        private readonly CSETContext _context;
        private ITokenManager _tokenManager;
        private IGalleryState _stateManager;

        /// <summary>
        /// Initializes a new instance of the GalleryStateController.
        /// </summary>
        /// <param name="context">The database context for gallery data access</param>
        /// <param name="tokenManager">The token manager for user authentication and language preferences</param>
        /// <param name="parser">The gallery state parser for board structure generation</param>
        public GalleryStateController(CSETContext context,
            ITokenManager tokenManager,
            IGalleryState parser
           )
        {
            _context = context;
            _tokenManager = tokenManager;
            _stateManager = parser;
        }

        /// <summary>
        /// Retrieves the gallery board structure for a specific layout.
        /// </summary>
        /// <param name="Layout_Name">The name of the layout to retrieve (e.g., "CSET", "ACET", "RRA")</param>
        /// <returns>
        /// 200 OK with GalleryBoardData containing the complete board structure
        /// 400 Bad Request if the layout cannot be retrieved
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves the complete gallery board structure for a specific layout:
        /// - Gallery groups organized by rows
        /// - Gallery items within each group
        /// - Localized titles and descriptions
        /// - Visibility settings and configuration
        /// 
        /// The gallery board structure includes:
        /// - Layout name and configuration
        /// - Rows containing gallery groups
        /// - Groups containing gallery items
        /// - Item properties and metadata
        /// 
        /// Gallery features:
        /// - Dynamic layout generation
        /// - Multi-language support
        /// - Visibility filtering
        /// - Configuration management
        /// - Assessment tool organization
        /// 
        /// The response includes:
        /// - Layout name and structure
        /// - Gallery groups with titles
        /// - Gallery items with properties
        /// - Localized content based on user language
        /// - Visibility and configuration settings
        /// 
        /// Layout types include:
        /// - CSET (standard layout)
        /// - ACET (assurance layout)
        /// - RRA (risk assessment layout)
        /// - Custom layouts
        /// - Module-specific layouts
        /// 
        /// Gallery items represent:
        /// - Assessment standards
        /// - Maturity models
        /// - Assessment tools
        /// - Report generators
        /// - Configuration options
        /// 
        /// Usage scenarios:
        /// - Initial gallery page load
        /// - Layout switching
        /// - Dynamic content generation
        /// - Assessment tool discovery
        /// - Interface customization
        /// 
        /// The gallery supports:
        /// - Multi-language interfaces
        /// - Role-based visibility
        /// - Dynamic content loading
        /// - Configuration-driven layouts
        /// - Assessment workflow integration
        /// 
        /// No authentication required - this is a public gallery endpoint.
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/getboard")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult GetBoard(string Layout_Name)
        {
            try
            {
                return Ok(_stateManager.GetGalleryBoard(Layout_Name));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Retrieves all layout items for a specific gallery layout.
        /// </summary>
        /// <param name="Layout_Name">The name of the layout to retrieve items for</param>
        /// <returns>
        /// 200 OK with List of GalleryItem containing all layout items
        /// 400 Bad Request if the layout items cannot be retrieved
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all gallery items for a specific layout:
        /// - Direct database query for layout items
        /// - Gallery item properties and metadata
        /// - Group associations and ordering
        /// - Item configuration and settings
        /// 
        /// The query process includes:
        /// - Gallery rows and group details
        /// - Gallery groups and items
        /// - Layout-specific filtering
        /// - Ordered item retrieval
        /// 
        /// Layout item features:
        /// - Item properties and metadata
        /// - Group associations
        /// - Configuration settings
        /// - Visibility status
        /// - Ordering information
        /// 
        /// The response includes:
        /// - Gallery item objects
        /// - Item titles and descriptions
        /// - Configuration setup data
        /// - Group associations
        /// - Item metadata
        /// 
        /// Query structure:
        /// - Joins gallery rows, group details, groups, and items
        /// - Filters by layout name
        /// - Orders by group ID and item GUID
        /// - Returns complete item objects
        /// 
        /// Gallery items contain:
        /// - Item GUID and title
        /// - Description and configuration
        /// - Icon information
        /// - Visibility settings
        /// - Group associations
        /// 
        /// Usage scenarios:
        /// - Layout item enumeration
        /// - Item property inspection
        /// - Configuration analysis
        /// - Layout debugging
        /// - Item management operations
        /// 
        /// This endpoint provides:
        /// - Direct database access to gallery items
        /// - Complete item property information
        /// - Layout-specific item filtering
        /// - Ordered item retrieval
        /// - Item metadata access
        /// 
        /// No authentication required - this is a public gallery endpoint.
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/getLayoutItems")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult GetLayoutItems(string Layout_Name)
        {
            try
            {
                var query = from r in _context.GALLERY_ROWS
                            join g in _context.GALLERY_GROUP_DETAILS
                                on r.Group_Id equals g.Group_Id
                            join n in _context.GALLERY_GROUP
                                on g.Group_Id equals n.Group_Id
                            join i in _context.GALLERY_ITEM
                                on g.Gallery_Item_Guid equals i.Gallery_Item_Guid
                            where r.Layout_Name == Layout_Name
                                && r.Group_Id == g.Group_Id
                                && g.Group_Id == n.Group_Id
                                && g.Gallery_Item_Guid == i.Gallery_Item_Guid
                            orderby g.Group_Id, g.Gallery_Item_Guid ascending
                            select new GalleryItem(i, n.Group_Id)
                            {
                            };

                var responseList = query.ToList();
                return Ok(responseList);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
