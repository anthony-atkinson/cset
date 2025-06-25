//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Model.AssessmentIO;
using CSETWebCore.DataLayer.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using NJsonSchema.NewtonsoftJson.Generation;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for database schema operations in CSET.
    /// This controller handles JSON schema generation for external standards,
    /// supporting schema validation and data structure definition for assessment
    /// import/export operations. Generates dynamic schemas based on database content.
    /// </summary>
    [ApiController]
    public class SchemaController : ControllerBase
    {
        private readonly CSETContext _context;

        /// <summary>
        /// Initializes a new instance of the SchemaController.
        /// </summary>
        /// <param name="context">The database context for data access operations</param>
        public SchemaController(CSETContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates a JSON schema for external standards based on database content.
        /// </summary>
        /// <returns>
        /// 200 OK with JSON schema string for ExternalStandard type
        /// 400 Bad Request if schema generation fails
        /// </returns>
        /// <remarks>
        /// This endpoint generates a dynamic JSON schema for external standards:
        /// - Schema based on ExternalStandard type definition
        /// - Dynamic category enumeration from database
        /// - Set name validation and enumeration
        /// - Requirement structure validation
        /// 
        /// The schema generation process includes:
        /// - JSON schema generation with inheritance flattening
        /// - Database context injection for dynamic content
        /// - Category enumeration from SETS_CATEGORY table
        /// - Set name validation from SETS table
        /// - Requirement structure validation
        /// 
        /// Schema features:
        /// - Dynamic category enumeration
        /// - Set name validation and enumeration
        /// - Requirement structure validation
        /// - Inheritance hierarchy flattening
        /// - Database-driven schema generation
        /// 
        /// The schema supports:
        /// - External standard validation
        /// - Assessment import/export operations
        /// - Data structure validation
        /// - Schema-driven development
        /// - API documentation generation
        /// 
        /// Generated schema includes:
        /// - ExternalStandard type definition
        /// - Category enumeration from database
        /// - Set name validation rules
        /// - Requirement structure validation
        /// - Property validation rules
        /// 
        /// Schema validation features:
        /// - Required field validation
        /// - String length constraints
        /// - Enumeration value validation
        /// - Nested object validation
        /// - Array structure validation
        /// 
        /// This endpoint is used for:
        /// - External standard import validation
        /// - API documentation generation
        /// - Client-side validation
        /// - Data structure definition
        /// - Schema-driven development tools
        /// 
        /// No authentication required - this is a public schema endpoint.
        /// </remarks>
        [HttpGet]
        [Route("api/Schema")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult Get()
        {
            try
            {
                var settings = new NewtonsoftJsonSchemaGeneratorSettings() { FlattenInheritanceHierarchy = true };

                StandardSchemaProcessor.dbContext = _context;
                var schema = NJsonSchema.JsonSchema.FromType<ExternalStandard>(settings);
                var schemaJson = schema.ToJson();
                return Ok(schemaJson);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
