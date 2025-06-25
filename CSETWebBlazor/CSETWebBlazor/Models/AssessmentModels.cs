using System.ComponentModel.DataAnnotations;

namespace CSETWebBlazor.Models
{
    /// <summary>
    /// Represents a user role in an assessment
    /// </summary>
    public class Role
    {
        public int AssessmentRoleId { get; set; }
        public string AssessmentRole { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a maturity model
    /// </summary>
    public class MaturityModel
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string ModelTitle { get; set; } = string.Empty;
        public string QuestionsAlias { get; set; } = string.Empty;
        public string AnswerOptions { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string ModelDescription { get; set; } = string.Empty;
        public string ModelCategory { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents detailed assessment information
    /// </summary>
    public class AssessmentDetail
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string AssessmentDate { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public string CreatorEmail { get; set; } = string.Empty;
        public string CreatorPhone { get; set; } = string.Empty;
        public string CreatorTitle { get; set; } = string.Empty;
        public string CreatorOrganization { get; set; } = string.Empty;
        public string AssessmentDescription { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public string AssessmentVersion { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Workflow { get; set; } = string.Empty;
        public string GalleryItemGuid { get; set; } = string.Empty;
        public string CustomSetName { get; set; } = string.Empty;
        public List<string> AssessmentFeatures { get; set; } = new List<string>();
        public List<MaturityModel> MaturityModels { get; set; } = new List<MaturityModel>();
        public List<string> Standards { get; set; } = new List<string>();
        public bool HasDiagram { get; set; }
        public bool IsEncrypted { get; set; }
        public string ApplicationMode { get; set; } = string.Empty;
        public int UserRoleId { get; set; }
        public string UserRole { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents assessment contact information
    /// </summary>
    public class AssessmentContact
    {
        public int AssessmentContactId { get; set; }
        public int AssessmentId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public int AssessmentRoleId { get; set; }
        public string AssessmentRole { get; set; } = string.Empty;
        public bool IsPrimaryContact { get; set; }
        public bool IsInvited { get; set; }
        public DateTime? InviteDate { get; set; }
    }

    /// <summary>
    /// Response model for assessment contacts
    /// </summary>
    public class AssessmentContactsResponse
    {
        public List<AssessmentContact> Contacts { get; set; } = new List<AssessmentContact>();
        public List<Role> AvailableRoles { get; set; } = new List<Role>();
    }

    /// <summary>
    /// Represents an answer to an assessment question
    /// </summary>
    public class Answer
    {
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string MarkForReview { get; set; } = string.Empty;
        public string Reviewed { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;
        public DateTime? AnswerDate { get; set; }
        public string AnsweredBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents organization type information
    /// </summary>
    public class OrganizationType
    {
        public int OrganizationTypeId { get; set; }
        public string OrganizationTypeName { get; set; } = string.Empty;
        public string OrganizationTypeDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents assessment completion information
    /// </summary>
    public class AssessmentCompletion
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public int UnansweredQuestions { get; set; }
        public double CompletionPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Workflow { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for creating a new assessment
    /// </summary>
    public class CreateAssessmentRequest
    {
        [Required]
        public string AssessmentName { get; set; } = string.Empty;
        
        public string AssessmentDescription { get; set; } = string.Empty;
        
        [Required]
        public string Workflow { get; set; } = string.Empty;
        
        public string GalleryItemGuid { get; set; } = string.Empty;
        
        public string CustomSetName { get; set; } = string.Empty;
        
        public List<string> AssessmentFeatures { get; set; } = new List<string>();
        
        public List<int> MaturityModelIds { get; set; } = new List<int>();
        
        public List<string> Standards { get; set; } = new List<string>();
    }

    /// <summary>
    /// Request model for updating assessment details
    /// </summary>
    public class UpdateAssessmentRequest
    {
        [Required]
        public int AssessmentId { get; set; }
        
        public string AssessmentName { get; set; } = string.Empty;
        
        public string AssessmentDescription { get; set; } = string.Empty;
        
        public string AssessmentDate { get; set; } = string.Empty;
        
        public List<string> AssessmentFeatures { get; set; } = new List<string>();
        
        public List<int> MaturityModelIds { get; set; } = new List<int>();
        
        public List<string> Standards { get; set; } = new List<string>();
    }

    /// <summary>
    /// Request model for searching contacts
    /// </summary>
    public class ContactSearchRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for creating a contact
    /// </summary>
    public class CreateContactRequest
    {
        [Required]
        public int AssessmentId { get; set; }
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        
        public string Organization { get; set; } = string.Empty;
        
        [Required]
        public int AssessmentRoleId { get; set; }
        
        public bool IsPrimaryContact { get; set; }
    }
} 