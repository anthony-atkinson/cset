using System.Text.Json;
using System.Xml;
using System.Text.RegularExpressions;

namespace CSETWebBlazor.Services
{
    public class CodeEditorService : ICodeEditorService
    {
        private readonly ILogger<CodeEditorService> _logger;

        public CodeEditorService(ILogger<CodeEditorService> logger)
        {
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateJsonAsync(string code)
        {
            var result = new ValidationResult();
            
            try
            {
                await Task.Run(() =>
                {
                    JsonDocument.Parse(code);
                });
                
                result.IsValid = true;
            }
            catch (JsonException ex)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Line = GetLineNumber(code, ex.BytePositionInLine),
                    Column = GetColumnNumber(code, ex.BytePositionInLine),
                    Message = ex.Message,
                    Severity = "Error"
                });
            }
            
            return result;
        }

        public async Task<ValidationResult> ValidateXmlAsync(string code)
        {
            var result = new ValidationResult();
            
            try
            {
                await Task.Run(() =>
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(code);
                });
                
                result.IsValid = true;
            }
            catch (XmlException ex)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Line = ex.LineNumber,
                    Column = ex.LinePosition,
                    Message = ex.Message,
                    Severity = "Error"
                });
            }
            
            return result;
        }

        public async Task<string> FormatJsonAsync(string code)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(code);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                return await Task.FromResult(JsonSerializer.Serialize(jsonDoc, options));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error formatting JSON");
                return code; // Return original if formatting fails
            }
        }

        public async Task<string> FormatXmlAsync(string code)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(code);
                
                using var stringWriter = new StringWriter();
                using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\n"
                });
                
                xmlDoc.Save(xmlWriter);
                return await Task.FromResult(stringWriter.ToString());
            }
            catch (XmlException ex)
            {
                _logger.LogError(ex, "Error formatting XML");
                return code; // Return original if formatting fails
            }
        }

        public async Task<List<SyntaxRule>> GetSyntaxRulesAsync(string language)
        {
            return await Task.FromResult(language.ToLower() switch
            {
                "json" => GetJsonSyntaxRules(),
                "xml" => GetXmlSyntaxRules(),
                _ => new List<SyntaxRule>()
            });
        }

        public async Task<List<HighlightedToken>> HighlightSyntaxAsync(string code, string language)
        {
            return await Task.FromResult(language.ToLower() switch
            {
                "json" => HighlightJson(code),
                "xml" => HighlightXml(code),
                _ => new List<HighlightedToken>()
            });
        }

        private List<SyntaxRule> GetJsonSyntaxRules()
        {
            return new List<SyntaxRule>
            {
                new() { Pattern = @"\b(true|false|null)\b", TokenType = "keyword", Color = "#0000FF" },
                new() { Pattern = @"\b\d+\.?\d*\b", TokenType = "number", Color = "#FF0000" },
                new() { Pattern = @"""([^""\\]|\\.)*""", TokenType = "string", Color = "#008000" },
                new() { Pattern = @"[{}\[\]]", TokenType = "punctuation", Color = "#000000" },
                new() { Pattern = @":", TokenType = "operator", Color = "#000000" },
                new() { Pattern = @",", TokenType = "punctuation", Color = "#000000" }
            };
        }

        private List<SyntaxRule> GetXmlSyntaxRules()
        {
            return new List<SyntaxRule>
            {
                new() { Pattern = @"<\?xml[^>]*\?>", TokenType = "xml-declaration", Color = "#808080" },
                new() { Pattern = @"<!--.*?-->", TokenType = "comment", Color = "#008000" },
                new() { Pattern = @"<[^>]+>", TokenType = "tag", Color = "#0000FF" },
                new() { Pattern = @"""([^""\\]|\\.)*""", TokenType = "string", Color = "#FF0000" },
                new() { Pattern = @"\b\w+\b", TokenType = "identifier", Color = "#000000" }
            };
        }

        private List<HighlightedToken> HighlightJson(string code)
        {
            var tokens = new List<HighlightedToken>();
            var rules = GetJsonSyntaxRules();
            
            foreach (var rule in rules)
            {
                var regex = new Regex(rule.Pattern);
                var matches = regex.Matches(code);
                
                foreach (Match match in matches)
                {
                    tokens.Add(new HighlightedToken
                    {
                        StartIndex = match.Index,
                        EndIndex = match.Index + match.Length,
                        TokenType = rule.TokenType,
                        Color = rule.Color
                    });
                }
            }
            
            return tokens.OrderBy(t => t.StartIndex).ToList();
        }

        private List<HighlightedToken> HighlightXml(string code)
        {
            var tokens = new List<HighlightedToken>();
            var rules = GetXmlSyntaxRules();
            
            foreach (var rule in rules)
            {
                var regex = new Regex(rule.Pattern, RegexOptions.Singleline);
                var matches = regex.Matches(code);
                
                foreach (Match match in matches)
                {
                    tokens.Add(new HighlightedToken
                    {
                        StartIndex = match.Index,
                        EndIndex = match.Index + match.Length,
                        TokenType = rule.TokenType,
                        Color = rule.Color
                    });
                }
            }
            
            return tokens.OrderBy(t => t.StartIndex).ToList();
        }

        private int GetLineNumber(string code, long bytePosition)
        {
            if (bytePosition < 0) return 1;
            
            var lines = code.Substring(0, (int)bytePosition).Split('\n');
            return lines.Length;
        }

        private int GetColumnNumber(string code, long bytePosition)
        {
            if (bytePosition < 0) return 1;
            
            var lines = code.Substring(0, (int)bytePosition).Split('\n');
            var lastLine = lines.LastOrDefault() ?? "";
            return lastLine.Length + 1;
        }
    }
} 