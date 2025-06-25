using System.ComponentModel.DataAnnotations;

namespace CSETWebBlazor.Services
{
    public interface IValidationService
    {
        /// <summary>
        /// Validates an object using Data Annotations
        /// </summary>
        Task<ValidationResult> ValidateObjectAsync<T>(T obj);
        
        /// <summary>
        /// Validates a specific property
        /// </summary>
        Task<ValidationResult> ValidatePropertyAsync<T>(T obj, string propertyName);
        
        /// <summary>
        /// Validates email format
        /// </summary>
        Task<bool> ValidateEmailAsync(string email);
        
        /// <summary>
        /// Validates password strength
        /// </summary>
        Task<PasswordValidationResult> ValidatePasswordAsync(string password);
        
        /// <summary>
        /// Validates file upload
        /// </summary>
        Task<ValidationResult> ValidateFileUploadAsync(Stream fileStream, string fileName, long maxSize);
        
        /// <summary>
        /// Validates JSON data
        /// </summary>
        Task<ValidationResult> ValidateJsonDataAsync(string jsonData, Type targetType);
        
        /// <summary>
        /// Validates XML data
        /// </summary>
        Task<ValidationResult> ValidateXmlDataAsync(string xmlData, string schemaPath);
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
        public List<ValidationWarning> Warnings { get; set; } = new();
    }

    public class ValidationError
    {
        public string PropertyName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }

    public class ValidationWarning
    {
        public string PropertyName { get; set; } = string.Empty;
        public string WarningMessage { get; set; } = string.Empty;
        public string WarningCode { get; set; } = string.Empty;
    }

    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public int Strength { get; set; } // 0-100
        public List<string> Requirements { get; set; } = new();
        public List<string> MissingRequirements { get; set; } = new();
    }
} 