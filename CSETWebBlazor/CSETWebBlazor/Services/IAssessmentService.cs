using CSETWebBlazor.Models;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Interface for assessment management services
    /// </summary>
    public interface IAssessmentService
    {
        // Assessment Management
        Task<List<AssessmentDetail>> GetAssessmentsAsync();
        Task<List<AssessmentCompletion>> GetAssessmentsCompletionAsync();
        Task<AssessmentDetail?> GetAssessmentDetailAsync();
        Task<AssessmentDetail?> LoadAssessmentAsync(int assessmentId);
        Task<AssessmentDetail> CreateAssessmentAsync(CreateAssessmentRequest request);
        Task<bool> UpdateAssessmentDetailsAsync(UpdateAssessmentRequest request);
        Task<bool> DeleteAssessmentAsync(int assessmentId);
        Task<bool> RefreshAssessmentAsync();
        Task<DateTime?> GetLastModifiedAsync();

        // Assessment Token Management
        Task<string> GetAssessmentTokenAsync(int assessmentId);
        Task<bool> DropAssessmentAsync();

        // Maturity Models
        Task<List<MaturityModel>> GetAllMaturityModelsAsync();
        Task<bool> UsesMaturityModelAsync(string modelName);
        Task<bool> UsesMaturityModelIdAsync(int modelId);
        Task<bool> SetModelAsync(string modelName);

        // Standards
        Task<bool> UsesStandardAsync(string setName);

        // Assessment Features
        Task<bool> HasFeatureAsync(string feature);
        Task<bool> ChangeFeatureAsync(string feature, bool state);
        Task<bool> HasDiagramAsync();

        // Assessment Contacts
        Task<AssessmentContactsResponse> GetAssessmentContactsAsync();
        Task<AssessmentContactsResponse> GetAssessmentContactsByIdAsync(int[] contactIds);
        Task<AssessmentContact> CreateContactAsync(CreateContactRequest request);
        Task<bool> UpdateContactAsync(AssessmentContact contact);
        Task<bool> RemoveContactAsync(int assessmentContactId);
        Task<bool> RemoveMyContactAsync(int assessmentId);
        Task<List<AssessmentContact>> SearchContactsAsync(ContactSearchRequest request);

        // Roles
        Task<List<Role>> GetRolesAsync();
        Task RefreshRolesAsync();

        // Organization Types
        Task<List<OrganizationType>> GetOrganizationTypesAsync();

        // Other Remarks
        Task<string> GetOtherRemarksAsync();
        Task<bool> SaveOtherRemarksAsync(string remarks);

        // Assessment Creator
        Task<AssessmentContact?> GetCreatorAsync();

        // Assessment Documents
        Task<List<string>> GetAssessmentDocumentsAsync();

        // Assessment Conversion
        Task<bool> ConvertAssessmentAsync(int originalId);
        Task<bool> CheckUpgradesAsync();

        // Assessment Settings
        Task<bool> SetAssessorSettingAsync(bool mode);
        Task<bool> IsDeletePermittedAsync();
        Task<bool> IsPciiAsync();
        Task<bool> PersistEncryptPreferenceAsync(bool status);
        Task<bool> GetEncryptPreferenceAsync();
        Task<bool> HasGlobalDocumentsAsync();

        // Assessment Answers
        Task<bool> UpdateAnswerAsync(Answer answer);

        // Assessment State
        Task<string> GetModeAsync();
        Task<int> GetCurrentAssessmentIdAsync();
        Task<bool> IsBrandNewAssessmentAsync();

        // Utility Methods
        Task<string> FormatLinebreaksAsync(string text);
        Task<bool> ClearFirstTimeAsync();

        // Events
        event Action<AssessmentDetail?> AssessmentChanged;
        event Action<string> AssessmentStateChanged;
    }
} 