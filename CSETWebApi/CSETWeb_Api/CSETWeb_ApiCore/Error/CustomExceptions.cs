//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;

namespace CSETWebCore.Api.Error
{
    /// <summary>
    /// Base exception for CSET application errors
    /// </summary>
    public class CSETException : Exception
    {
        /// <summary>
        /// Error code for categorization
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// HTTP status code for the error
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// User ID associated with the error
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Constructor with message
        /// </summary>
        public CSETException(string message, string errorCode = "CSET_ERROR", int statusCode = 500) 
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Constructor with message and inner exception
        /// </summary>
        public CSETException(string message, Exception innerException, string errorCode = "CSET_ERROR", int statusCode = 500) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Exception for assessment-related errors
    /// </summary>
    public class AssessmentException : CSETException
    {
        /// <summary>
        /// Assessment ID associated with the error
        /// </summary>
        public int? AssessmentId { get; }

        public AssessmentException(string message, int? assessmentId = null, string errorCode = "ASSESSMENT_ERROR", int statusCode = 400) 
            : base(message, errorCode, statusCode)
        {
            AssessmentId = assessmentId;
        }

        public AssessmentException(string message, Exception innerException, int? assessmentId = null, string errorCode = "ASSESSMENT_ERROR", int statusCode = 400) 
            : base(message, innerException, errorCode, statusCode)
        {
            AssessmentId = assessmentId;
        }
    }

    /// <summary>
    /// Exception for authentication and authorization errors
    /// </summary>
    public class AuthenticationException : CSETException
    {
        public AuthenticationException(string message, string errorCode = "AUTH_ERROR", int statusCode = 401) 
            : base(message, errorCode, statusCode)
        {
        }

        public AuthenticationException(string message, Exception innerException, string errorCode = "AUTH_ERROR", int statusCode = 401) 
            : base(message, innerException, errorCode, statusCode)
        {
        }
    }

    /// <summary>
    /// Exception for data validation errors
    /// </summary>
    public class ValidationException : CSETException
    {
        /// <summary>
        /// Validation errors dictionary
        /// </summary>
        public Dictionary<string, string[]> ValidationErrors { get; }

        public ValidationException(string message, Dictionary<string, string[]> validationErrors = null, string errorCode = "VALIDATION_ERROR", int statusCode = 400) 
            : base(message, errorCode, statusCode)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        }

        public ValidationException(string message, Exception innerException, Dictionary<string, string[]> validationErrors = null, string errorCode = "VALIDATION_ERROR", int statusCode = 400) 
            : base(message, innerException, errorCode, statusCode)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        }
    }

    /// <summary>
    /// Exception for business logic errors
    /// </summary>
    public class BusinessLogicException : CSETException
    {
        /// <summary>
        /// Business rule that was violated
        /// </summary>
        public string BusinessRule { get; }

        public BusinessLogicException(string message, string businessRule = null, string errorCode = "BUSINESS_LOGIC_ERROR", int statusCode = 400) 
            : base(message, errorCode, statusCode)
        {
            BusinessRule = businessRule;
        }

        public BusinessLogicException(string message, Exception innerException, string businessRule = null, string errorCode = "BUSINESS_LOGIC_ERROR", int statusCode = 400) 
            : base(message, innerException, errorCode, statusCode)
        {
            BusinessRule = businessRule;
        }
    }

    /// <summary>
    /// Exception for file operation errors
    /// </summary>
    public class FileOperationException : CSETException
    {
        /// <summary>
        /// File path associated with the error
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// File operation that failed
        /// </summary>
        public string Operation { get; }

        public FileOperationException(string message, string filePath = null, string operation = null, string errorCode = "FILE_OPERATION_ERROR", int statusCode = 500) 
            : base(message, errorCode, statusCode)
        {
            FilePath = filePath;
            Operation = operation;
        }

        public FileOperationException(string message, Exception innerException, string filePath = null, string operation = null, string errorCode = "FILE_OPERATION_ERROR", int statusCode = 500) 
            : base(message, innerException, errorCode, statusCode)
        {
            FilePath = filePath;
            Operation = operation;
        }
    }

    /// <summary>
    /// Exception for import/export operation errors
    /// </summary>
    public class ImportExportException : CSETException
    {
        /// <summary>
        /// Import/export format
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// Operation type (import/export)
        /// </summary>
        public string OperationType { get; }

        public ImportExportException(string message, string format = null, string operationType = null, string errorCode = "IMPORT_EXPORT_ERROR", int statusCode = 400) 
            : base(message, errorCode, statusCode)
        {
            Format = format;
            OperationType = operationType;
        }

        public ImportExportException(string message, Exception innerException, string format = null, string operationType = null, string errorCode = "IMPORT_EXPORT_ERROR", int statusCode = 400) 
            : base(message, innerException, errorCode, statusCode)
        {
            Format = format;
            OperationType = operationType;
        }
    }
} 