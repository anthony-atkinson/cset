using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Xml;
using System.Text.Json;

namespace CSETWebBlazor.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateObjectAsync<T>(T obj)
        {
            var result = new ValidationResult();
            var validationContext = new ValidationContext(obj);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            var isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);

            result.IsValid = isValid;

            foreach (var validationResult in validationResults)
            {
                result.Errors.Add(new ValidationError
                {
                    PropertyName = validationResult.MemberNames.FirstOrDefault() ?? "",
                    ErrorMessage = validationResult.ErrorMessage ?? "",
                    ErrorCode = "ValidationError"
                });
            }

            return await Task.FromResult(result);
        }

        public async Task<ValidationResult> ValidatePropertyAsync<T>(T obj, string propertyName)
        {
            var result = new ValidationResult();
            var property = typeof(T).GetProperty(propertyName);
            
            if (property == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    PropertyName = propertyName,
                    ErrorMessage = "Property not found",
                    ErrorCode = "PropertyNotFound"
                });
                return await Task.FromResult(result);
            }

            var value = property.GetValue(obj);
            var validationContext = new ValidationContext(obj) { MemberName = propertyName };
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            var isValid = Validator.TryValidateProperty(value, validationContext, validationResults);

            result.IsValid = isValid;

            foreach (var validationResult in validationResults)
            {
                result.Errors.Add(new ValidationError
                {
                    PropertyName = propertyName,
                    ErrorMessage = validationResult.ErrorMessage ?? "",
                    ErrorCode = "ValidationError"
                });
            }

            return await Task.FromResult(result);
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return await Task.FromResult(false);

            try
            {
                var emailAttribute = new EmailAddressAttribute();
                return await Task.FromResult(emailAttribute.IsValid(email));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email: {Email}", email);
                return await Task.FromResult(false);
            }
        }

        public async Task<PasswordValidationResult> ValidatePasswordAsync(string password)
        {
            var result = new PasswordValidationResult();
            var requirements = new List<string>();
            var missingRequirements = new List<string>();
            var strength = 0;

            // Check minimum length
            if (password.Length >= 8)
            {
                requirements.Add("At least 8 characters");
                strength += 20;
            }
            else
            {
                missingRequirements.Add("At least 8 characters");
            }

            // Check for uppercase letters
            if (password.Any(char.IsUpper))
            {
                requirements.Add("At least one uppercase letter");
                strength += 20;
            }
            else
            {
                missingRequirements.Add("At least one uppercase letter");
            }

            // Check for lowercase letters
            if (password.Any(char.IsLower))
            {
                requirements.Add("At least one lowercase letter");
                strength += 20;
            }
            else
            {
                missingRequirements.Add("At least one lowercase letter");
            }

            // Check for numbers
            if (password.Any(char.IsDigit))
            {
                requirements.Add("At least one number");
                strength += 20;
            }
            else
            {
                missingRequirements.Add("At least one number");
            }

            // Check for special characters
            if (password.Any(c => !char.IsLetterOrDigit(c)))
            {
                requirements.Add("At least one special character");
                strength += 20;
            }
            else
            {
                missingRequirements.Add("At least one special character");
            }

            result.Requirements = requirements;
            result.MissingRequirements = missingRequirements;
            result.Strength = strength;
            result.IsValid = missingRequirements.Count == 0;

            return await Task.FromResult(result);
        }

        public async Task<ValidationResult> ValidateFileUploadAsync(Stream fileStream, string fileName, long maxSize)
        {
            var result = new ValidationResult();

            // Check file size
            if (fileStream.Length > maxSize)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    PropertyName = "FileSize",
                    ErrorMessage = $"File size exceeds maximum allowed size of {maxSize / (1024 * 1024)}MB",
                    ErrorCode = "FileSizeExceeded"
                });
            }

            // Check file extension
            var allowedExtensions = new[] { ".json", ".xml", ".csv", ".xlsx", ".pdf" };
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    PropertyName = "FileType",
                    ErrorMessage = $"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}",
                    ErrorCode = "InvalidFileType"
                });
            }

            // Check for malicious content (basic check)
            if (fileExtension == ".json" || fileExtension == ".xml")
            {
                try
                {
                    fileStream.Position = 0;
                    using var reader = new StreamReader(fileStream);
                    var content = await reader.ReadToEndAsync();
                    
                    if (fileExtension == ".json")
                    {
                        JsonDocument.Parse(content);
                    }
                    else if (fileExtension == ".xml")
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(content);
                    }
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError
                    {
                        PropertyName = "FileContent",
                        ErrorMessage = $"Invalid {fileExtension.ToUpperInvariant()} content: {ex.Message}",
                        ErrorCode = "InvalidFileContent"
                    });
                }
            }

            if (result.Errors.Count == 0)
            {
                result.IsValid = true;
            }

            return result;
        }

        public async Task<ValidationResult> ValidateJsonDataAsync(string jsonData, Type targetType)
        {
            var result = new ValidationResult();

            try
            {
                var jsonDoc = JsonDocument.Parse(jsonData);
                
                // Basic validation - in a real application, you might want to deserialize
                // and validate the object against the target type
                result.IsValid = true;
            }
            catch (JsonException ex)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    PropertyName = "JsonData",
                    ErrorMessage = $"Invalid JSON format: {ex.Message}",
                    ErrorCode = "InvalidJsonFormat"
                });
            }

            return await Task.FromResult(result);
        }

        public async Task<ValidationResult> ValidateXmlDataAsync(string xmlData, string schemaPath)
        {
            var result = new ValidationResult();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);

                // If schema path is provided, validate against schema
                if (!string.IsNullOrEmpty(schemaPath) && File.Exists(schemaPath))
                {
                    xmlDoc.Schemas.Add(null, schemaPath);
                    xmlDoc.Validate(null);
                }

                result.IsValid = true;
            }
            catch (XmlException ex)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    PropertyName = "XmlData",
                    ErrorMessage = $"Invalid XML format: {ex.Message}",
                    ErrorCode = "InvalidXmlFormat"
                });
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    PropertyName = "XmlData",
                    ErrorMessage = $"XML validation error: {ex.Message}",
                    ErrorCode = "XmlValidationError"
                });
            }

            return await Task.FromResult(result);
        }
    }
} 