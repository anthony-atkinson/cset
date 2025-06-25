//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Api.Models;
using CSETWebCore.Business.RepositoryLibrary;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.Common;
using CSETWebCore.Interfaces.ResourceLibrary;
using CSETWebCore.Model.ResourceLibrary;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;


namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for resource library functionality in CSET.
    /// This controller handles document retrieval, search operations, and resource management
    /// for the CSET resource library. Supports both local and cloud-based document storage,
    /// document search capabilities, and FlowDoc conversion to HTML. Provides access to
    /// reference documents, procurement language, and recommendations.
    /// </summary>
    [ApiController]
    public class ResourceLibraryController : ControllerBase
    {
        private CSETContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IHtmlFromXamlConverter _html;
        private readonly IFlowDocManager _flow;

        /// <summary>
        /// Initializes a new instance of the ResourceLibraryController.
        /// </summary>
        /// <param name="context">Database context for resource operations</param>
        /// <param name="environment">Web hosting environment for file operations</param>
        /// <param name="configuration">Application configuration for library settings</param>
        /// <param name="html">HTML converter service for document conversion</param>
        /// <param name="flow">Flow document manager for FlowDoc operations</param>
        public ResourceLibraryController(CSETContext context, IWebHostEnvironment environment, IConfiguration configuration,
            IHtmlFromXamlConverter html, IFlowDocManager flow)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
            _html = html;
            _flow = flow;
        }


        /// <summary>
        /// Retrieves a reference document by file ID with fallback to cloud library.
        /// </summary>
        /// <param name="fileId">ID or filename of the document to retrieve</param>
        /// <returns>
        /// 200 OK with document content if successful
        /// 404 Not Found if document doesn't exist
        /// 500 Internal Server Error if cloud library is unavailable
        /// </returns>
        /// <remarks>
        /// This endpoint provides comprehensive document retrieval functionality:
        /// - Searches local GEN_FILE table by filename or file ID
        /// - Returns binary data from database if available
        /// - Falls back to physical file location if not in database
        /// - Queries cloud resource library server as final fallback
        /// - Caches cloud documents locally for future access
        /// 
        /// Document retrieval process:
        /// 1. Check local GEN_FILE table for document
        /// 2. Return binary data if found in database
        /// 3. Look for physical file if not in database
        /// 4. Query cloud library server if local file not found
        /// 5. Cache cloud document locally for future use
        /// 
        /// Cloud library integration:
        /// - Configurable host and port settings
        /// - Automatic document caching after cloud retrieval
        /// - Error handling for network issues
        /// - Content type preservation from cloud source
        /// 
        /// Document types supported:
        /// - Reference documents
        /// - Technical specifications
        /// - Guidelines and procedures
        /// - Any file type supported by the library
        /// 
        /// Performance considerations:
        /// - Local caching reduces cloud requests
        /// - Binary storage in database for fast access
        /// - Automatic fallback ensures document availability
        /// </remarks>
        [HttpGet]
        [Route("api/library/doc/{fileId}")]
        public IActionResult GetReferenceDocument(string fileId)
        {
            var refDocManager = new ReferenceDocumentManager(_context, _environment, _configuration);
            var fileResp = refDocManager.FindLocalReferenceDocument(fileId);

            if (fileResp == null)
            {
                // Fallback to cloud CSET Library

                var libHost = _configuration.GetValue<string>("Library:Host");
                var libPort = _configuration.GetValue<string>("Library:Port");
                if (libPort != null)
                {
                    libPort = $":{libPort}";
                }

                if (libHost == null)
                {
                    // No fallback available
                    return GetErrorResponseFile("NoHostDefined.html");
                }


                HttpClient req = new HttpClient();
                HttpResponseMessage libResponse;

                var url = $"http://{libHost}{libPort}/api/library/doc/{fileId}";

                try
                {
                    libResponse = req.GetAsync(url).Result;
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error($"Error making doc request to '{url}': {ex.Message}");

                    return GetErrorResponseFile("DocumentNotFound.html");
                }


                if (!libResponse.IsSuccessStatusCode)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn($"Reference document '{fileId}' was requested but does not exist.");
                    return StatusCode((int)libResponse.StatusCode);
                }


                NLog.LogManager.GetCurrentClassLogger().Info($"Reference document '{fileId}' was downloaded from the cloud library.");


                var stream = libResponse.Content.ReadAsStream();


                // Save document content to the local GEN_FILE record so that we never have to pull from the cloud again
                if (int.TryParse(fileId, out int genFileId))
                {
                    refDocManager.SaveDataBuffer(genFileId, stream);
                }


                // repackage the stream and return it
                stream.Position = 0;
                return File(stream, libResponse.Content.Headers.ContentType.ToString());
            }


            // In case we want download statistics
            NLog.LogManager.GetCurrentClassLogger().Info($"Reference document '{fileResp.Id}' was downloaded.");


            var contentDisposition = new ContentDispositionHeaderValue("inline");
            contentDisposition.FileName = fileResp.FileName;
            Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

            return File(fileResp.Stream, fileResp.ContentType);
        }



        /// <summary>
        /// Returns an error response file from the app_data directory.
        /// </summary>
        /// <param name="fileName">Name of the error response file to return</param>
        /// <returns>FileContentResult containing the error response HTML</returns>
        /// <remarks>
        /// Internal method used to return standardized error response files.
        /// Loads HTML error pages from the app_data/ResponseFile directory.
        /// Used when document retrieval fails or cloud library is unavailable.
        /// </remarks>
        private FileContentResult GetErrorResponseFile(string fileName)
        {
            var rh = new ResourceHelper();
            var bytes = rh.GetCopiedResourceAsBytes(Path.Combine("app_data", "ResponseFile", fileName));

            return File(bytes, "text/html");
        }


        /// <summary>
        /// Searches for documents in the resource library based on specified criteria.
        /// </summary>
        /// <param name="searchRequest">SearchRequest object containing search terms and criteria</param>
        /// <returns>
        /// 200 OK with list of ResourceNode objects matching the search criteria
        /// 200 OK with empty list if no search term provided
        /// </returns>
        /// <remarks>
        /// Performs text-based search across the resource library documents.
        /// Returns documents that match the specified search term.
        /// Uses the SearchDocs service to perform the actual search operation.
        /// Returns empty list if search term is null, empty, or whitespace.
        /// Search results include document metadata and relevance information.
        /// </remarks>
        [HttpPost]
        [Route("api/library/search")]
        public IActionResult GetDetails([FromBody] SearchRequest searchRequest)
        {
            if (String.IsNullOrWhiteSpace(searchRequest.term))
                return Ok(new List<ResourceNode>());

            CSETGlobalProperties props = new CSETGlobalProperties(_context);
            SearchDocs search = new SearchDocs(props, new ResourceLibraryRepository(_context, props));
            return Ok(search.Search(searchRequest));
        }


        /// <summary>
        /// Retrieves the complete tree structure of the resource library.
        /// </summary>
        /// <returns>List of SimpleNode objects representing the resource library hierarchy</returns>
        /// <remarks>
        /// Returns a hierarchical tree structure containing all resource library document metadata.
        /// The tree provides multiple categorization views of the available documents.
        /// Used for navigation and browsing the resource library contents.
        /// Tree structure includes categories, subcategories, and document nodes.
        /// </remarks>
        [HttpGet]
        [Route("api/library/tree")]
        public List<SimpleNode> GetTree()
        {
            IResourceLibraryRepository resource = new ResourceLibraryRepository(_context, new CSETGlobalProperties(_context));
            return resource.GetTreeNodes();
        }


        /// <summary>
        /// Converts a FlowDoc (procurement language or recommendations) to HTML format.
        /// </summary>
        /// <param name="type">Type of FlowDoc (procurement language or recommendations)</param>
        /// <param name="id">ID of the specific FlowDoc to convert</param>
        /// <returns>HTML string representation of the FlowDoc</returns>
        /// <remarks>
        /// Converts FlowDoc content from the database to HTML format for web display.
        /// Supports both procurement language and recommendations document types.
        /// Uses the FlowDoc manager service to perform the conversion.
        /// Returns HTML that can be embedded in web pages or applications.
        /// </remarks>
        [HttpGet]
        [Route("api/library/flowdoc")]
        public string GetFlowDoc(string type, int id)
        {
            // pull the flowdoc from the database
            string html = _flow.GetFlowDoc(type, id);

            return html;
        }


        /// <summary>
        /// Returns the complete resource library listing.
        /// </summary>
        /// <returns>
        /// 200 OK with resource library listing if successful
        /// </returns>
        /// <remarks>
        /// Provides a comprehensive listing of all resources in the library.
        /// Used for displaying the complete resource library contents.
        /// Returns structured data suitable for library browsing interfaces.
        /// </remarks>
        [HttpGet]
        [Route("api/library/list")]
        public IActionResult ShowResourceLibrary()
        {
            IResourceLibraryRepository resource = new ResourceLibraryRepository(_context, new CSETGlobalProperties(_context));
            return Ok(resource.GetResourceLibrary());
        }


        /// <summary>
        /// Checks if local documents are available in the resource library.
        /// </summary>
        /// <returns>
        /// 200 OK with boolean indicating local document availability
        /// </returns>
        /// <remarks>
        /// Determines whether the resource library has local documents available.
        /// Used to check if offline access to documents is possible.
        /// Returns true if local documents exist, false otherwise.
        /// Helps applications determine whether cloud fallback is needed.
        /// </remarks>
        [HttpGet]
        [Route("api/HasLocalDocuments")]
        public IActionResult HasLocalDocuments()
        {
            IResourceLibraryRepository resource = new ResourceLibraryRepository(_context, new CSETGlobalProperties(_context));
            return Ok(resource.HasLocalDocuments());
        }
    }
}
