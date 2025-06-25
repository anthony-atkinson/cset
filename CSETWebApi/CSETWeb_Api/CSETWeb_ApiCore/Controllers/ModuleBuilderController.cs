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
using CSETWebCore.Business.Authorization;
using CSETWebCore.Interfaces.ModuleBuilder;
using CSETWebCore.Model.Document;
using CSETWebCore.Model.Set;
using CSETWebCore.Helpers;

namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for custom module creation and management in CSET.
    /// This controller handles the creation, modification, and management of custom assessment modules,
    /// including questions, requirements, reference documents, and module structure.
    /// Requires authentication and authorization via CsetAuthorize attribute.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class ModuleBuilderController : ControllerBase
    {
        private readonly IModuleBuilderBusiness _module;

        /// <summary>
        /// Initializes a new instance of the ModuleBuilderController.
        /// </summary>
        /// <param name="module">Module builder business logic service</param>
        public ModuleBuilderController(IModuleBuilderBusiness module)
        {
            _module = module;
        }

        /// <summary>
        /// Returns a list of custom modules.
        /// 
        /// In the future, we may want to return all modules to allow the user
        /// to clone a 'stock' module.  Currently we cannot prevent the user
        /// from overwriting stock requirements or questions, so that functionality
        /// is turned off.
        /// </summary>
        /// <returns>
        /// 200 OK with list of custom sets if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns only custom modules created by users, not the stock/built-in modules.
        /// This prevents accidental modification of core CSET functionality.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetCustomSets")]
        public IActionResult GetCustomSetsList()
        {
            return Ok(_module.GetCustomSetList());
        }

        /// <summary>
        /// Returns a list of all modules including both custom and stock modules.
        /// </summary>
        /// <returns>
        /// 200 OK with list of all sets if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns all modules including stock modules that come with CSET.
        /// Use with caution as this includes modules that should not be modified.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetAllSets")]
        public IActionResult GetAllSetsList()
        {
            return Ok(_module.GetCustomSetList(true));
        }

        /// <summary>
        /// Returns a list of modules that are currently in use by assessments.
        /// </summary>
        /// <returns>
        /// 200 OK with list of sets in use if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns modules that are actively being used in assessments.
        /// These modules should be handled carefully as modifications may affect existing assessments.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetSetsInUse")]
        public IActionResult GetSetsInUseList()
        {
            return Ok(_module.GetSetsInUseList());
        }

        /// <summary>
        /// Returns a list of non-custom (stock) modules that match the specified name pattern.
        /// </summary>
        /// <param name="setName">Name pattern to search for in stock modules</param>
        /// <returns>
        /// 200 OK with list of non-custom sets if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Searches through stock modules for those matching the specified name pattern.
        /// These are the built-in modules that come with CSET.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetNonCustomSets")]
        public IActionResult GetNonCustomSetList(string setName)
        {
            return Ok(_module.GetNonCustomSetList(setName));
        }

        /// <summary>
        /// Sets the base sets for a custom module.
        /// </summary>
        /// <param name="setName">Name of the custom module</param>
        /// <param name="setNames">Array of base set names to associate with the custom module</param>
        /// <returns>
        /// 200 OK if base sets set successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Associates stock modules as base sets for a custom module.
        /// Base sets provide the foundation structure and questions for the custom module.
        /// If setName is null or empty, the operation is skipped.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/SetBaseSets")]
        public IActionResult SetBaseSets(string setName, string[] setNames)
        {
            if (String.IsNullOrWhiteSpace(setName))
                return Ok();

            _module.SetBaseSets(setName, setNames);
            return Ok();
        }

        /// <summary>
        /// Retrieves the base sets associated with a custom module.
        /// </summary>
        /// <param name="setName">Name of the custom module</param>
        /// <returns>
        /// 200 OK with list of base sets if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns the stock modules that serve as the foundation for the specified custom module.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetBaseSets")]
        public IActionResult GetBaseSets(string setName)
        {
            return Ok(_module.GetBaseSets(setName));
        }

        /// <summary>
        /// Retrieves detailed information about a specific module.
        /// </summary>
        /// <param name="setName">Name of the module to retrieve details for</param>
        /// <returns>
        /// 200 OK with module details if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns comprehensive information about the specified module including
        /// its structure, questions, requirements, and metadata.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetSetDetail")]
        public IActionResult GetSetDetail(string setName)
        {
            return Ok(_module.GetSetDetail(setName));
        }

        /// <summary>
        /// Saves the set information and returns the setname (key) of the record.
        /// </summary>
        /// <param name="setDetail">SetDetail object containing the module information to save</param>
        /// <returns>
        /// 200 OK with set name if saved successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Creates or updates a module with the provided information.
        /// Returns the set name which serves as the unique identifier for the module.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/UpdateSetDetail")]
        public IActionResult UpdateSetDetail([FromBody] SetDetail setDetail)
        {
            return Ok(new { SetName = _module.SaveSetDetail(setDetail) });
        }

        /// <summary>
        /// Creates a clone of an existing module.
        /// </summary>
        /// <param name="setName">Name of the module to clone</param>
        /// <returns>
        /// 200 OK with cloned set name if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Creates an exact copy of the specified module with a new name.
        /// The cloned module will have all the same questions, requirements, and structure as the original.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/CloneSet")]
        public IActionResult CloneSet(string setName)
        {
            return Ok(_module.CloneSet(setName));
        }

        /// <summary>
        /// Copies a base (stock) module to create a custom version.
        /// </summary>
        /// <param name="SourceSetName">Name of the source base module</param>
        /// <param name="DestinationSetName">Name for the new custom module</param>
        /// <returns>
        /// 200 OK if copy operation successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Creates a custom copy of a stock module, allowing modifications without affecting the original.
        /// This is useful for creating organization-specific versions of standard modules.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/CopyBaseToCustom")]
        public IActionResult CopyBaseToCustom(string SourceSetName, string DestinationSetName)
        {
            _module.AddCopyToSet(SourceSetName, DestinationSetName);
            return Ok();
        }

        /// <summary>
        /// Removes the copy-to-custom relationship for a module.
        /// </summary>
        /// <param name="setName">Name of the module to remove from copy relationship</param>
        /// <returns>
        /// 200 OK if relationship removed successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Removes the association between a custom module and its base module.
        /// This prevents automatic updates from the base module.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/BaseToCustomDelete")]
        public IActionResult BaseToCustomDelete(string setName)
        {
            _module.DeleteCopyToSet(setName);
            return Ok();
        }

        /// <summary>
        /// Deletes a custom module.
        /// </summary>
        /// <param name="setName">Name of the module to delete</param>
        /// <returns>
        /// 200 OK with deletion result if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Permanently removes a custom module from the system.
        /// This operation cannot be undone and will affect any assessments using this module.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/DeleteSet")]
        public IActionResult DeleteSet([FromBody] string setName)
        {
            return Ok(_module.DeleteSet(setName));
        }

        /// <summary>
        /// Retrieves all questions associated with a specific module.
        /// </summary>
        /// <param name="setName">Name of the module</param>
        /// <returns>
        /// 200 OK with question list response if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns all questions that are part of the specified module,
        /// including their categories, subcategories, and grouping information.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetQuestionsForSet")]
        public IActionResult GetQuestionsForSet(string setName)
        {
            QuestionListResponse response = _module.GetQuestionsForSet(setName);

            return Ok(response);
        }

        /// <summary>
        /// Returns a list of questions whose 'original_set_name' is the one specified,
        /// but are also being used in other sets.
        /// </summary>
        /// <param name="setName">Name of the original module</param>
        /// <returns>
        /// 200 OK with list of question IDs if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Identifies questions that originated from the specified module but are now
        /// being used in other modules. This helps track question dependencies and usage.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetMyQuestionsUsedByOtherSets")]
        public IActionResult GetMyQuestionsUsedByOtherSets(string setName)
        {
            List<int> response = _module.GetMyQuestionsUsedByOtherSets(setName);

            return Ok(response);
        }

        /// <summary>
        /// Checks if a question text already exists in the system.
        /// </summary>
        /// <param name="questionText">Text to check for existence</param>
        /// <returns>
        /// 200 OK with true if text exists, false otherwise
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Validates whether the provided question text already exists in any module.
        /// Returns true if the text exists, false if it's unique.
        /// Null or empty text is considered to exist (returns true).
        /// </remarks>
        [HttpPost]
        [Route("api/builder/ExistsQuestionText")]
        public IActionResult ExistsQuestionText([FromBody] string questionText)
        {
            // Don't let null be added as question text
            if (questionText == null)
            {
                return Ok(true);
            }

            return Ok(_module.ExistsQuestionText(questionText));
        }

        /// <summary>
        /// Creates a new custom question from the supplied text.
        /// </summary>
        /// <param name="request">SetQuestion object containing the question details</param>
        /// <returns>
        /// 200 OK if question created successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Creates a new custom question and adds it to the specified module.
        /// The question will be associated with the specified requirement and SAL levels.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/AddCustomQuestion")]
        public IActionResult AddCustomQuestion([FromBody] SetQuestion request)
        {
            _module.AddCustomQuestion(request);
            return Ok();
        }

        /// <summary>
        /// Adds 'base' or 'stock' questions to the Requirement or Set.
        /// </summary>
        /// <param name="request">AddQuestionsRequest object containing questions to add</param>
        /// <returns>
        /// 200 OK if questions added successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Adds existing stock questions to a custom module or requirement.
        /// Each question in the request will be added with the specified SAL levels.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/AddQuestions")]
        public IActionResult AddQuestion([FromBody] AddQuestionsRequest request)
        {
            foreach (QuestionAdd add in request.QuestionList)
            {
                SetQuestion r = new SetQuestion
                {
                    SetName = request.SetName,
                    RequirementID = request.RequirementID,
                    QuestionID = add.QuestionID,
                    SalLevels = add.SalLevels
                };
                _module.AddQuestion(r);
            }

            return Ok();
        }

        /// <summary>
        /// Removes a question from a module or requirement.
        /// </summary>
        /// <param name="request">SetQuestion object identifying the question to remove</param>
        /// <returns>
        /// 200 OK if question removed successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Removes the specified question from the module or requirement.
        /// This does not delete the question from the system, only removes it from the specified context.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/RemoveQuestion")]
        public IActionResult RemoveQuestion([FromBody] SetQuestion request)
        {
            _module.RemoveQuestion(request);
            return Ok();
        }

        /// <summary>
        /// Retrieves standard question categories.
        /// </summary>
        /// <returns>
        /// 200 OK with list of standard categories if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns the standard question categories used throughout CSET.
        /// These categories help organize questions by topic or domain.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetStandardCategories")]
        public IActionResult GetStandardCategories()
        {
            return Ok(_module.GetStandardCategories());
        }

        /// <summary>
        /// Retrieves categories, subcategories, and group headings for question organization.
        /// </summary>
        /// <returns>
        /// 200 OK with hierarchical structure if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns the complete hierarchical structure used for organizing questions,
        /// including categories, subcategories, and group headings.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetCategoriesSubcategoriesGroupHeadings")]
        public IActionResult GetCategoriesSubcategoriesGroupHeadings()
        {
            return Ok(_module.GetCategoriesSubcategoriesGroupHeadings());
        }

        /// <summary>
        /// Searches for questions based on specified criteria.
        /// </summary>
        /// <param name="searchParms">QuestionSearch object containing search criteria</param>
        /// <returns>
        /// 200 OK with search results if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Performs a search for questions based on the provided search parameters.
        /// Search criteria may include text, categories, subcategories, and other filters.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/SearchQuestions")]
        public IActionResult SearchQuestions([FromBody] QuestionSearch searchParms)
        {
            return Ok(_module.SearchQuestions(searchParms));
        }

        /// <summary>
        /// Sets the SAL (Security Assurance Level) for questions in a module.
        /// </summary>
        /// <param name="parms">SalParms object containing SAL level information</param>
        /// <returns>
        /// 200 OK if SAL levels set successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Updates the Security Assurance Level for questions in a module.
        /// SAL levels determine the rigor of security controls required.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/SetSalLevel")]
        public IActionResult SetSalLevel([FromBody] SalParms parms)
        {
            _module.SetSalLevel(parms);
            return Ok();
        }

        /// <summary>
        /// Updates the text of an existing question.
        /// </summary>
        /// <param name="parms">QuestionTextUpdateParms object containing the question ID and new text</param>
        /// <returns>
        /// 200 OK with update result if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Updates the question text for the specified question ID.
        /// This change will affect all modules that use this question.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/UpdateQuestionText")]
        public IActionResult UpdateQuestionText([FromBody] QuestionTextUpdateParms parms)
        {
            return Ok(_module.UpdateQuestionText(parms.QuestionID, parms.QuestionText));
        }

        /// <summary>
        /// Checks if a question is currently being used in any assessments.
        /// </summary>
        /// <param name="questionID">ID of the question to check</param>
        /// <returns>
        /// 200 OK with true if question is in use, false otherwise
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Determines whether the specified question is currently being used in any assessments.
        /// This helps prevent deletion of questions that are actively being used.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/IsQuestionInUse")]
        public IActionResult IsQuestionInUse(int questionID)
        {
            return Ok(_module.IsQuestionInUse(questionID));
        }

        /// <summary>
        /// Updates the text of a heading in a module.
        /// </summary>
        /// <param name="parms">HeadingUpdateParms object containing the heading ID and new text</param>
        /// <returns>
        /// 200 OK if heading updated successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Updates the text of a heading (category, subcategory, or group heading) in a module.
        /// Headings help organize the structure of questions within a module.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/UpdateHeadingText")]
        public IActionResult UpdateHeadingText([FromBody] HeadingUpdateParms parms)
        {
            _module.UpdateHeadingText(parms.PairID, parms.HeadingText);
            return Ok();
        }

        /// <summary>
        /// Retrieves the standard structure of a module.
        /// </summary>
        /// <param name="setName">Name of the module</param>
        /// <returns>
        /// 200 OK with module structure if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns the hierarchical structure of the specified module,
        /// including categories, subcategories, requirements, and questions.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetStandardStructure")]
        public IActionResult GetStandardStructure(string setName)
        {
            return Ok(_module.GetModuleStructure(setName));
        }

        /// <summary>
        /// Creates a new Requirement.  Returns the ID.
        /// </summary>
        /// <param name="parms">Requirement object containing the requirement details</param>
        /// <returns>
        /// 200 OK with new requirement ID if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Creates a new requirement in the specified module.
        /// Requirements serve as containers for related questions and provide structure to the assessment.
        /// Returns the ID of the newly created requirement.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/CreateRequirement")]
        public IActionResult CreateRequirement([FromBody] Requirement parms)
        {
            return Ok(_module.CreateRequirement(parms));
        }

        /// <summary>
        /// Returns the Requirement for the setname and requirement ID.
        /// </summary>
        /// <param name="setName">Name of the module</param>
        /// <param name="reqID">ID of the requirement</param>
        /// <returns>
        /// 200 OK with requirement details if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Retrieves detailed information about a specific requirement within a module.
        /// Includes the requirement text, associated questions, and metadata.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetRequirement")]
        public IActionResult GetRequirement(string setName, int reqID)
        {
            return Ok(_module.GetRequirement(setName, reqID));
        }

        /// <summary>
        /// Updates an existing Requirement.
        /// </summary>
        /// <param name="parms">Requirement object containing updated requirement details</param>
        /// <returns>
        /// 200 OK with update result if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Updates the properties of an existing requirement in a module.
        /// Changes may include requirement text, title, and other metadata.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/UpdateRequirement")]
        public IActionResult UpdateRequirement([FromBody] Requirement parms)
        {
            return Ok(_module.UpdateRequirement(parms));
        }

        /// <summary>
        /// Removes an existing Requirement from the Set.
        /// </summary>
        /// <param name="parms">Requirement object identifying the requirement to remove</param>
        /// <returns>
        /// 200 OK if requirement removed successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Removes a requirement from the specified module.
        /// This will also remove all questions associated with the requirement.
        /// The operation cannot be undone.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/RemoveRequirement")]
        public IActionResult RemoveRequirement([FromBody] Requirement parms)
        {
            _module.RemoveRequirement(parms);
            return Ok();
        }

        /// <summary>
        /// Returns a list of reference docs attached to the set after applying the filter.
        /// </summary>
        /// <param name="setName">Name of the module</param>
        /// <param name="filter">Filter string to apply to document names</param>
        /// <returns>
        /// 200 OK with filtered reference documents if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns reference documents associated with the module that match the specified filter.
        /// The filter is applied to document names to narrow down the results.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetReferenceDocs")]
        public IActionResult GetReferenceDocs(string setName, string filter)
        {
            return Ok(_module.GetReferenceDocs(setName, filter));
        }

        /// <summary>
        /// Returns the list of reference docs attached to the set.
        /// </summary>
        /// <param name="setName">Name of the module</param>
        /// <returns>
        /// 200 OK with all reference documents if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns all reference documents associated with the specified module.
        /// These documents provide supporting information for the module's requirements and questions.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetReferenceDocsForSet")]
        public IActionResult GetReferenceDocs(string setName)
        {
            return Ok(_module.GetReferenceDocsForSet(setName));
        }

        /// <summary>
        /// Retrieves detailed information about a specific reference document.
        /// </summary>
        /// <param name="id">ID of the reference document</param>
        /// <returns>
        /// 200 OK with reference document details if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns comprehensive information about a specific reference document,
        /// including its title, description, file information, and usage details.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/GetReferenceDocDetail")]
        public IActionResult GetReferenceDocDetail(int id)
        {
            return Ok(_module.GetReferenceDocDetail(id));
        }

        /// <summary>
        /// Updates the details of a reference document.
        /// </summary>
        /// <param name="doc">ReferenceDoc object containing updated document details</param>
        /// <returns>
        /// 200 OK if document updated successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Updates the properties of an existing reference document.
        /// Changes may include title, description, and other metadata.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/UpdateReferenceDocDetail")]
        public IActionResult UpdateReferenceDocDetail([FromBody] ReferenceDoc doc)
        {
            _module.UpdateReferenceDocDetail(doc);
            return Ok();
        }

        /// <summary>
        /// Refreshes the list of GEN_FILEs associated with a SET.
        /// The entire list of applicable files must be sent.
        /// </summary>
        /// <param name="parms">SetFileSelection object containing file selection parameters</param>
        /// <returns>
        /// 200 OK if file selection updated successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Updates the association between a module and its supporting files.
        /// The entire list of files must be provided as this operation replaces the existing selection.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/SelectSetFile")]
        public IActionResult SelectSetFiles(SetFileSelection parms)
        {
            _module.SelectSetFile(parms);
            return Ok();
        }

        /// <summary>
        /// Adds or deletes the source or resource doc/bookmark from the requirement.
        /// The 'isSourceRef' argument specifies whether the document is a 'source' document.
        /// The 'add' argument specifies whether the document is being added or removed from the requirement.
        /// </summary>
        /// <param name="reqId">ID of the requirement</param>
        /// <param name="docId">ID of the document</param>
        /// <param name="isSourceRef">Whether the document is a source reference</param>
        /// <param name="bookmark">Bookmark within the document</param>
        /// <param name="add">Whether to add (true) or remove (false) the document</param>
        /// <returns>
        /// 200 OK with operation result if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Manages the association between requirements and reference documents.
        /// Source references are primary documents, while resource references are supporting materials.
        /// Bookmarks allow linking to specific sections within documents.
        /// </remarks>
        [HttpGet]
        [Route("api/builder/AddDeleteRefDocToRequirement")]
        public IActionResult AddDeleteRefDocToRequirement(int reqId, int docId, bool isSourceRef, string bookmark, bool add)
        {
            return Ok(_module.AddDeleteRefDocToRequirement(reqId, docId, isSourceRef, bookmark, add));
        }

        /// <summary>
        /// Uploads a reference document to the system.
        /// </summary>
        /// <returns>
        /// 200 OK with document ID if upload successful
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if upload fails
        /// </returns>
        /// <remarks>
        /// Handles file upload for reference documents.
        /// Creates a GEN_FILE entry and a SET_FILES entry in the database.
        /// The uploaded file becomes available for association with modules and requirements.
        /// </remarks>
        [HttpPost]
        [Route("api/builder/UploadReferenceDoc")]
        public async Task<int> UploadReferenceDoc()
        {
            try
            {
                FileUploadStream fileUploader = new FileUploadStream();
                Dictionary<string, string> formValues = new Dictionary<string, string>();
                formValues.Add("title", null);
                formValues.Add("setName", null);

                FileUploadStreamResult streamResult = await fileUploader.ProcessUploadStream(HttpContext.Request.HttpContext, formValues);

                // Create a GEN_FILE entry, and a SET_FILES entry.
                return _module.RecordDocInDB(streamResult);

            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");

                throw;
            }
        }
    }
}
