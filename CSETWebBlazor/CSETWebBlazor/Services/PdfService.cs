using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing;

namespace CSETWebBlazor.Services
{
    public class PdfService : IPdfService
    {
        private readonly IChartService _chartService;
        private readonly ILogger<PdfService> _logger;

        public PdfService(IChartService chartService, ILogger<PdfService> logger)
        {
            _chartService = chartService;
            _logger = logger;
        }

        public async Task<byte[]> GenerateBasicReportAsync(ReportData data)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Add title
                if (!string.IsNullOrEmpty(data.Title))
                {
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    var title = new Paragraph(data.Title, titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);
                    document.Add(new Paragraph(" ")); // Spacing
                }

                // Add subtitle
                if (!string.IsNullOrEmpty(data.Subtitle))
                {
                    var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var subtitle = new Paragraph(data.Subtitle, subtitleFont);
                    subtitle.Alignment = Element.ALIGN_CENTER;
                    document.Add(subtitle);
                    document.Add(new Paragraph(" ")); // Spacing
                }

                // Add generation date
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var dateText = $"Generated: {data.GeneratedDate:MM/dd/yyyy HH:mm:ss}";
                var dateParagraph = new Paragraph(dateText, dateFont);
                dateParagraph.Alignment = Element.ALIGN_RIGHT;
                document.Add(dateParagraph);
                document.Add(new Paragraph(" ")); // Spacing

                // Add sections
                foreach (var section in data.Sections)
                {
                    // Section title
                    var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    var sectionTitle = new Paragraph(section.Title, sectionFont);
                    document.Add(sectionTitle);
                    document.Add(new Paragraph(" ")); // Spacing

                    // Section content
                    if (!string.IsNullOrEmpty(section.Content))
                    {
                        var contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        var content = new Paragraph(section.Content, contentFont);
                        document.Add(content);
                        document.Add(new Paragraph(" ")); // Spacing
                    }

                    // Section tables
                    foreach (var table in section.Tables)
                    {
                        document.Add(await CreatePdfTableAsync(table));
                        document.Add(new Paragraph(" ")); // Spacing
                    }
                }

                // Add charts
                for (int i = 0; i < data.Charts.Count; i++)
                {
                    var chartImage = Image.GetInstance(data.Charts[i]);
                    chartImage.ScaleToFit(500, 300);
                    chartImage.Alignment = Element.ALIGN_CENTER;
                    document.Add(chartImage);
                    document.Add(new Paragraph(" ")); // Spacing
                }

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating basic PDF report");
                throw;
            }
        }

        public async Task<byte[]> GenerateAssessmentReportAsync(AssessmentReportData data)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Add header
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph("CSET Assessment Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph(" ")); // Spacing

                // Assessment details
                var detailsFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                document.Add(new Paragraph($"Assessment: {data.AssessmentName}", detailsFont));
                document.Add(new Paragraph($"Facility: {data.FacilityName}", detailsFont));
                document.Add(new Paragraph($"Assessment Date: {data.AssessmentDate:MM/dd/yyyy}", detailsFont));
                document.Add(new Paragraph($"Overall Score: {data.OverallScore:F1}%", detailsFont));
                document.Add(new Paragraph(" ")); // Spacing

                // Categories table
                var categoryTable = new PdfPTable(4);
                categoryTable.WidthPercentage = 100;

                // Headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                categoryTable.AddCell(new PdfPCell(new Phrase("Category", headerFont)));
                categoryTable.AddCell(new PdfPCell(new Phrase("Score", headerFont)));
                categoryTable.AddCell(new PdfPCell(new Phrase("Questions", headerFont)));
                categoryTable.AddCell(new PdfPCell(new Phrase("Answered", headerFont)));

                // Data
                var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var category in data.Categories)
                {
                    categoryTable.AddCell(new PdfPCell(new Phrase(category.Name, dataFont)));
                    categoryTable.AddCell(new PdfPCell(new Phrase($"{category.Score:F1}%", dataFont)));
                    categoryTable.AddCell(new PdfPCell(new Phrase(category.QuestionCount.ToString(), dataFont)));
                    categoryTable.AddCell(new PdfPCell(new Phrase(category.AnsweredCount.ToString(), dataFont)));
                }

                document.Add(categoryTable);
                document.Add(new Paragraph(" ")); // Spacing

                // Add sections from base report
                foreach (var section in data.Sections)
                {
                    var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    var sectionTitle = new Paragraph(section.Title, sectionFont);
                    document.Add(sectionTitle);
                    document.Add(new Paragraph(" ")); // Spacing

                    if (!string.IsNullOrEmpty(section.Content))
                    {
                        var contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        var content = new Paragraph(section.Content, contentFont);
                        document.Add(content);
                        document.Add(new Paragraph(" ")); // Spacing
                    }
                }

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating assessment PDF report");
                throw;
            }
        }

        public async Task<byte[]> GenerateComparisonReportAsync(ComparisonReportData data)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Add header
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph("CSET Assessment Comparison Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph(" ")); // Spacing

                // Comparison table
                var comparisonTable = new PdfPTable(3);
                comparisonTable.WidthPercentage = 100;

                // Headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                comparisonTable.AddCell(new PdfPCell(new Phrase("Assessment", headerFont)));
                comparisonTable.AddCell(new PdfPCell(new Phrase("Date", headerFont)));
                comparisonTable.AddCell(new PdfPCell(new Phrase("Score", headerFont)));

                // Data
                var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var comparison in data.Comparisons)
                {
                    comparisonTable.AddCell(new PdfPCell(new Phrase(comparison.AssessmentName, dataFont)));
                    comparisonTable.AddCell(new PdfPCell(new Phrase(comparison.AssessmentDate.ToString("MM/dd/yyyy"), dataFont)));
                    comparisonTable.AddCell(new PdfPCell(new Phrase($"{comparison.Score:F1}%", dataFont)));
                }

                document.Add(comparisonTable);
                document.Add(new Paragraph(" ")); // Spacing

                // Add sections from base report
                foreach (var section in data.Sections)
                {
                    var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    var sectionTitle = new Paragraph(section.Title, sectionFont);
                    document.Add(sectionTitle);
                    document.Add(new Paragraph(" ")); // Spacing

                    if (!string.IsNullOrEmpty(section.Content))
                    {
                        var contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        var content = new Paragraph(section.Content, contentFont);
                        document.Add(content);
                        document.Add(new Paragraph(" ")); // Spacing
                    }
                }

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating comparison PDF report");
                throw;
            }
        }

        public async Task<byte[]> GenerateTrendReportAsync(TrendReportData data)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Add header
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph("CSET Trend Analysis Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph(" ")); // Spacing

                // Trend data table
                var trendTable = new PdfPTable(3);
                trendTable.WidthPercentage = 100;

                // Headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                trendTable.AddCell(new PdfPCell(new Phrase("Date", headerFont)));
                trendTable.AddCell(new PdfPCell(new Phrase("Assessment", headerFont)));
                trendTable.AddCell(new PdfPCell(new Phrase("Score", headerFont)));

                // Data
                var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var trendPoint in data.TrendData)
                {
                    trendTable.AddCell(new PdfPCell(new Phrase(trendPoint.Date.ToString("MM/dd/yyyy"), dataFont)));
                    trendTable.AddCell(new PdfPCell(new Phrase(trendPoint.AssessmentName, dataFont)));
                    trendTable.AddCell(new PdfPCell(new Phrase($"{trendPoint.Score:F1}%", dataFont)));
                }

                document.Add(trendTable);
                document.Add(new Paragraph(" ")); // Spacing

                // Add sections from base report
                foreach (var section in data.Sections)
                {
                    var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    var sectionTitle = new Paragraph(section.Title, sectionFont);
                    document.Add(sectionTitle);
                    document.Add(new Paragraph(" ")); // Spacing

                    if (!string.IsNullOrEmpty(section.Content))
                    {
                        var contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        var content = new Paragraph(section.Content, contentFont);
                        document.Add(content);
                        document.Add(new Paragraph(" ")); // Spacing
                    }
                }

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating trend PDF report");
                throw;
            }
        }

        public async Task<byte[]> GenerateCustomReportAsync(CustomReportData data)
        {
            try
            {
                // For custom reports, we'll use the basic report structure
                // but allow for custom data injection
                var reportData = new ReportData
                {
                    Title = data.Title,
                    Subtitle = data.Subtitle,
                    GeneratedDate = data.GeneratedDate,
                    Sections = data.Sections,
                    Charts = data.Charts
                };

                return await GenerateBasicReportAsync(reportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom PDF report");
                throw;
            }
        }

        private async Task<PdfPTable> CreatePdfTableAsync(ReportTable table)
        {
            var pdfTable = new PdfPTable(table.Headers.Count);
            pdfTable.WidthPercentage = 100;

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            foreach (var header in table.Headers)
            {
                pdfTable.AddCell(new PdfPCell(new Phrase(header, headerFont)));
            }

            // Add data rows
            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            foreach (var row in table.Rows)
            {
                foreach (var cell in row)
                {
                    pdfTable.AddCell(new PdfPCell(new Phrase(cell, dataFont)));
                }
            }

            return pdfTable;
        }
    }
} 