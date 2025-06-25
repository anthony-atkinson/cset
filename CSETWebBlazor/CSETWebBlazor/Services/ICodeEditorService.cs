namespace CSETWebBlazor.Services
{
    public interface ICodeEditorService
    {
        /// <summary>
        /// Validates JSON code
        /// </summary>
        Task<ValidationResult> ValidateJsonAsync(string code);
        
        /// <summary>
        /// Validates XML code
        /// </summary>
        Task<ValidationResult> ValidateXmlAsync(string code);
        
        /// <summary>
        /// Formats JSON code
        /// </summary>
        Task<string> FormatJsonAsync(string code);
        
        /// <summary>
        /// Formats XML code
        /// </summary>
        Task<string> FormatXmlAsync(string code);
        
        /// <summary>
        /// Gets syntax highlighting rules for a language
        /// </summary>
        Task<List<SyntaxRule>> GetSyntaxRulesAsync(string language);
        
        /// <summary>
        /// Performs syntax highlighting on code
        /// </summary>
        Task<List<HighlightedToken>> HighlightSyntaxAsync(string code, string language);
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
        public List<ValidationWarning> Warnings { get; set; } = new();
    }

    public class ValidationError
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error";
    }

    public class ValidationWarning
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
    }

    public class SyntaxRule
    {
        public string Pattern { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
    }

    public class HighlightedToken
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string TokenType { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
    }
} 