//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using NUnit.Framework;
using System.Threading.Tasks;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.ModuleBuilder;

namespace CSETWebCore.PlaywrightTests.Tests.ModuleBuilder
{
    [TestFixture]
    [TestCategory("E2E")]
    [TestCategory("ModuleBuilder")]
    public class ModuleBuilderTests : PlaywrightTestBase
    {
        private ModuleBuilderPage _moduleBuilderPage;

        [SetUp]
        public async Task Setup()
        {
            _moduleBuilderPage = new ModuleBuilderPage(Page);
            await LoginAsDefaultUser();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_NavigateToModuleList_DisplaysModuleList()
        {
            // Navigate to module builder
            await _moduleBuilderPage.NavigateToModuleList();

            // Verify module list page is displayed
            await _moduleBuilderPage.VerifyModuleListPageDisplayed();
            await _moduleBuilderPage.VerifyCreateModuleButtonPresent();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_CreateNewModule_SuccessfulCreation()
        {
            // Navigate to module builder
            await _moduleBuilderPage.NavigateToModuleList();

            // Create a new module
            var moduleName = "Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Verify module creation
            await _moduleBuilderPage.VerifyModuleCreated(moduleName);
            await _moduleBuilderPage.VerifyModuleDetailPageDisplayed();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_ModuleDetails_EditModuleInformation()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Edit Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Edit module details
            var newFullName = "Updated Full Name";
            var newShortName = "UpdatedShort";
            var newDescription = "Updated module description";
            
            await _moduleBuilderPage.EditModuleDetails(newFullName, newShortName, newDescription);

            // Verify changes are saved
            await _moduleBuilderPage.VerifyModuleDetailsUpdated(newFullName, newShortName, newDescription);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_CloneModule_SuccessfulCloning()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var originalModuleName = "Original Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(originalModuleName);

            // Clone the module
            await _moduleBuilderPage.CloneModule(originalModuleName);

            // Verify module is cloned
            await _moduleBuilderPage.VerifyModuleCloned(originalModuleName);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_DeleteModule_SuccessfulDeletion()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Delete Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Delete the module
            await _moduleBuilderPage.DeleteModule(moduleName);

            // Verify module is deleted
            await _moduleBuilderPage.VerifyModuleDeleted(moduleName);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Requirements_AddNewRequirement()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Requirements Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Navigate to requirements
            await _moduleBuilderPage.NavigateToRequirements();

            // Add a new requirement
            var requirementTitle = "Test Requirement";
            var requirementText = "This is a test requirement for validation";
            var category = "Test Category";
            var subcategory = "Test Subcategory";
            
            await _moduleBuilderPage.AddRequirement(requirementTitle, requirementText, category, subcategory);

            // Verify requirement is added
            await _moduleBuilderPage.VerifyRequirementAdded(requirementTitle);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Requirements_EditExistingRequirement()
        {
            // Navigate to module builder and create a module with a requirement
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Edit Requirements Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            await _moduleBuilderPage.NavigateToRequirements();
            
            var requirementTitle = "Original Requirement";
            var requirementText = "Original requirement text";
            var category = "Test Category";
            var subcategory = "Test Subcategory";
            
            await _moduleBuilderPage.AddRequirement(requirementTitle, requirementText, category, subcategory);

            // Edit the requirement
            var updatedTitle = "Updated Requirement";
            var updatedText = "Updated requirement text";
            
            await _moduleBuilderPage.EditRequirement(requirementTitle, updatedTitle, updatedText);

            // Verify requirement is updated
            await _moduleBuilderPage.VerifyRequirementUpdated(updatedTitle);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Requirements_DeleteRequirement()
        {
            // Navigate to module builder and create a module with a requirement
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Delete Requirements Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            await _moduleBuilderPage.NavigateToRequirements();
            
            var requirementTitle = "Delete Test Requirement";
            var requirementText = "Requirement to be deleted";
            var category = "Test Category";
            var subcategory = "Test Subcategory";
            
            await _moduleBuilderPage.AddRequirement(requirementTitle, requirementText, category, subcategory);

            // Delete the requirement
            await _moduleBuilderPage.DeleteRequirement(requirementTitle);

            // Verify requirement is deleted
            await _moduleBuilderPage.VerifyRequirementDeleted(requirementTitle);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Questions_AddNewQuestion()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Questions Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Navigate to questions
            await _moduleBuilderPage.NavigateToQuestions();

            // Add a new question
            var questionText = "This is a test question for validation?";
            var category = "Test Category";
            var subcategory = "Test Subcategory";
            
            await _moduleBuilderPage.AddQuestion(questionText, category, subcategory);

            // Verify question is added
            await _moduleBuilderPage.VerifyQuestionAdded(questionText);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Questions_EditExistingQuestion()
        {
            // Navigate to module builder and create a module with a question
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Edit Questions Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            await _moduleBuilderPage.NavigateToQuestions();
            
            var questionText = "Original test question?";
            var category = "Test Category";
            var subcategory = "Test Subcategory";
            
            await _moduleBuilderPage.AddQuestion(questionText, category, subcategory);

            // Edit the question
            var updatedQuestionText = "Updated test question?";
            
            await _moduleBuilderPage.EditQuestion(questionText, updatedQuestionText);

            // Verify question is updated
            await _moduleBuilderPage.VerifyQuestionUpdated(updatedQuestionText);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Questions_DeleteQuestion()
        {
            // Navigate to module builder and create a module with a question
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Delete Questions Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            await _moduleBuilderPage.NavigateToQuestions();
            
            var questionText = "Question to be deleted?";
            var category = "Test Category";
            var subcategory = "Test Subcategory";
            
            await _moduleBuilderPage.AddQuestion(questionText, category, subcategory);

            // Delete the question
            await _moduleBuilderPage.DeleteQuestion(questionText);

            // Verify question is deleted
            await _moduleBuilderPage.VerifyQuestionDeleted(questionText);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_CloneFromExisting_SelectAndCloneModules()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Clone From Existing Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Clone from existing modules
            await _moduleBuilderPage.CloneFromExistingModules();

            // Verify clone dialog is displayed
            await _moduleBuilderPage.VerifyCloneDialogDisplayed();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Documents_ManageModuleDocuments()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Documents Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Navigate to documents
            await _moduleBuilderPage.NavigateToDocuments();

            // Verify documents page is displayed
            await _moduleBuilderPage.VerifyDocumentsPageDisplayed();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Validation_EmptyModuleName_ShowsError()
        {
            // Navigate to module builder
            await _moduleBuilderPage.NavigateToModuleList();

            // Try to create module with empty name
            await _moduleBuilderPage.AttemptCreateModuleWithEmptyName();

            // Verify error message is displayed
            await _moduleBuilderPage.VerifyModuleNameRequiredError();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Validation_DuplicateModuleName_ShowsError()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Duplicate Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Try to create another module with the same name
            await _moduleBuilderPage.AttemptCreateModuleWithDuplicateName(moduleName);

            // Verify error message is displayed
            await _moduleBuilderPage.VerifyDuplicateModuleNameError();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Validation_InvalidShortName_ShowsError()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Invalid Short Name Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Try to set invalid short name
            await _moduleBuilderPage.AttemptSetInvalidShortName("Invalid Short Name With Spaces");

            // Verify error message is displayed
            await _moduleBuilderPage.VerifyInvalidShortNameError();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Categories_ManageCategoriesAndSubcategories()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Categories Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Navigate to requirements
            await _moduleBuilderPage.NavigateToRequirements();

            // Add requirement with new category and subcategory
            var requirementTitle = "Category Test Requirement";
            var requirementText = "Test requirement for category management";
            var newCategory = "New Test Category";
            var newSubcategory = "New Test Subcategory";
            
            await _moduleBuilderPage.AddRequirement(requirementTitle, requirementText, newCategory, newSubcategory);

            // Verify category and subcategory are created
            await _moduleBuilderPage.VerifyCategoryCreated(newCategory);
            await _moduleBuilderPage.VerifySubcategoryCreated(newSubcategory);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Search_SearchRequirements()
        {
            // Navigate to module builder and create a module with requirements
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Search Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            await _moduleBuilderPage.NavigateToRequirements();
            
            var requirementTitle = "Searchable Requirement";
            var requirementText = "This requirement should be searchable";
            var category = "Search Category";
            var subcategory = "Search Subcategory";
            
            await _moduleBuilderPage.AddRequirement(requirementTitle, requirementText, category, subcategory);

            // Search for the requirement
            await _moduleBuilderPage.SearchRequirements("Searchable");

            // Verify search results
            await _moduleBuilderPage.VerifySearchResultsContain(requirementTitle);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Search_SearchQuestions()
        {
            // Navigate to module builder and create a module with questions
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Question Search Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            await _moduleBuilderPage.NavigateToQuestions();
            
            var questionText = "Searchable question for testing?";
            var category = "Search Category";
            var subcategory = "Search Subcategory";
            
            await _moduleBuilderPage.AddQuestion(questionText, category, subcategory);

            // Search for the question
            await _moduleBuilderPage.SearchQuestions("Searchable");

            // Verify search results
            await _moduleBuilderPage.VerifyQuestionSearchResultsContain(questionText);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Filter_FilterByCategory()
        {
            // Navigate to module builder and create a module with requirements in different categories
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Filter Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            await _moduleBuilderPage.NavigateToRequirements();
            
            var requirement1Title = "Category A Requirement";
            var requirement1Text = "Requirement in category A";
            var categoryA = "Category A";
            var subcategoryA = "Subcategory A";
            
            var requirement2Title = "Category B Requirement";
            var requirement2Text = "Requirement in category B";
            var categoryB = "Category B";
            var subcategoryB = "Subcategory B";
            
            await _moduleBuilderPage.AddRequirement(requirement1Title, requirement1Text, categoryA, subcategoryA);
            await _moduleBuilderPage.AddRequirement(requirement2Title, requirement2Text, categoryB, subcategoryB);

            // Filter by category A
            await _moduleBuilderPage.FilterByCategory(categoryA);

            // Verify filtered results
            await _moduleBuilderPage.VerifyFilteredResultsContain(requirement1Title);
            await _moduleBuilderPage.VerifyFilteredResultsDoNotContain(requirement2Title);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Sort_SortRequirementsByTitle()
        {
            // Navigate to module builder and create a module with requirements
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Sort Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            await _moduleBuilderPage.NavigateToRequirements();
            
            var requirement1Title = "Zebra Requirement";
            var requirement1Text = "Zebra requirement text";
            var requirement2Title = "Alpha Requirement";
            var requirement2Text = "Alpha requirement text";
            var category = "Sort Category";
            var subcategory = "Sort Subcategory";
            
            await _moduleBuilderPage.AddRequirement(requirement1Title, requirement1Text, category, subcategory);
            await _moduleBuilderPage.AddRequirement(requirement2Title, requirement2Text, category, subcategory);

            // Sort by title ascending
            await _moduleBuilderPage.SortRequirementsByTitle("ascending");

            // Verify sort order
            await _moduleBuilderPage.VerifyRequirementsSortedByTitle(requirement2Title, requirement1Title);
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_BulkOperations_BulkDeleteRequirements()
        {
            // Navigate to module builder and create a module with multiple requirements
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Bulk Operations Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            await _moduleBuilderPage.NavigateToRequirements();
            
            var requirements = new[]
            {
                ("Bulk Delete 1", "First requirement for bulk delete"),
                ("Bulk Delete 2", "Second requirement for bulk delete"),
                ("Bulk Delete 3", "Third requirement for bulk delete")
            };
            
            foreach (var (title, text) in requirements)
            {
                await _moduleBuilderPage.AddRequirement(title, text, "Bulk Category", "Bulk Subcategory");
            }

            // Select multiple requirements for bulk delete
            await _moduleBuilderPage.SelectMultipleRequirements(new[] { "Bulk Delete 1", "Bulk Delete 2" });
            await _moduleBuilderPage.BulkDeleteRequirements();

            // Verify requirements are deleted
            await _moduleBuilderPage.VerifyRequirementsDeleted(new[] { "Bulk Delete 1", "Bulk Delete 2" });
            await _moduleBuilderPage.VerifyRequirementExists("Bulk Delete 3");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Export_ExportModule()
        {
            // Navigate to module builder and create a module
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Export Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);

            // Export the module
            await _moduleBuilderPage.ExportModule();

            // Verify export functionality
            await _moduleBuilderPage.VerifyModuleExportStarted();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Import_ImportModule()
        {
            // Navigate to module builder
            await _moduleBuilderPage.NavigateToModuleList();

            // Import a module
            await _moduleBuilderPage.ImportModule();

            // Verify import functionality
            await _moduleBuilderPage.VerifyModuleImportDialogDisplayed();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Accessibility_SupportsScreenReaders()
        {
            // Navigate to module builder
            await _moduleBuilderPage.NavigateToModuleList();

            // Verify accessibility features
            await _moduleBuilderPage.VerifyAriaLabelsPresent();
            await _moduleBuilderPage.VerifyKeyboardNavigation();
            await _moduleBuilderPage.VerifyScreenReaderSupport();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Performance_ModuleCreationPerformance()
        {
            // Navigate to module builder
            await _moduleBuilderPage.NavigateToModuleList();

            // Measure module creation performance
            var moduleName = "Performance Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.MeasureModuleCreationPerformance(moduleName);

            // Verify performance metrics
            await _moduleBuilderPage.VerifyModuleCreationPerformance();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_ErrorHandling_NetworkFailure_HandlesGracefully()
        {
            // Navigate to module builder
            await _moduleBuilderPage.NavigateToModuleList();

            // Simulate network failure during module creation
            await _moduleBuilderPage.SimulateNetworkFailure();

            // Verify error handling
            await _moduleBuilderPage.VerifyNetworkFailureHandled();
            await _moduleBuilderPage.VerifyRetryOptionAvailable();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_DataIntegrity_ModuleDataPersistence()
        {
            // Navigate to module builder and create a module with data
            await _moduleBuilderPage.NavigateToModuleList();
            var moduleName = "Data Integrity Test Module " + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            await _moduleBuilderPage.CreateNewModule(moduleName);
            
            // Add requirements and questions
            await _moduleBuilderPage.NavigateToRequirements();
            await _moduleBuilderPage.AddRequirement("Test Requirement", "Test requirement text", "Test Category", "Test Subcategory");
            
            await _moduleBuilderPage.NavigateToQuestions();
            await _moduleBuilderPage.AddQuestion("Test question?", "Test Category", "Test Subcategory");

            // Navigate away and back
            await _moduleBuilderPage.NavigateToModuleList();
            await _moduleBuilderPage.NavigateToModuleDetail(moduleName);

            // Verify data persistence
            await _moduleBuilderPage.VerifyModuleDataPersisted();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task ModuleBuilder_Concurrency_MultipleUsers_HandlesConcurrentAccess()
        {
            // Navigate to module builder
            await _moduleBuilderPage.NavigateToModuleList();

            // Simulate concurrent access
            await _moduleBuilderPage.SimulateConcurrentAccess();

            // Verify concurrency handling
            await _moduleBuilderPage.VerifyConcurrencyHandled();
        }
    }
} 