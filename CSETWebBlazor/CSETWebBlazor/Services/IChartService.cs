using System.Drawing;

namespace CSETWebBlazor.Services
{
    public interface IChartService
    {
        /// <summary>
        /// Generates a line chart as a byte array (PNG format)
        /// </summary>
        Task<byte[]> GenerateLineChartAsync(ChartData data, ChartOptions options);
        
        /// <summary>
        /// Generates a bar chart as a byte array (PNG format)
        /// </summary>
        Task<byte[]> GenerateBarChartAsync(ChartData data, ChartOptions options);
        
        /// <summary>
        /// Generates a pie chart as a byte array (PNG format)
        /// </summary>
        Task<byte[]> GeneratePieChartAsync(ChartData data, ChartOptions options);
        
        /// <summary>
        /// Generates a doughnut chart as a byte array (PNG format)
        /// </summary>
        Task<byte[]> GenerateDoughnutChartAsync(ChartData data, ChartOptions options);
        
        /// <summary>
        /// Generates a scatter plot as a byte array (PNG format)
        /// </summary>
        Task<byte[]> GenerateScatterPlotAsync(ChartData data, ChartOptions options);
    }

    public class ChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<ChartDataset> Datasets { get; set; } = new();
    }

    public class ChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<double> Data { get; set; } = new();
        public string BackgroundColor { get; set; } = "#007bff";
        public string BorderColor { get; set; } = "#007bff";
        public double BorderWidth { get; set; } = 2;
        public bool Fill { get; set; } = false;
    }

    public class ChartOptions
    {
        public string Title { get; set; } = string.Empty;
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
        public bool ShowLegend { get; set; } = true;
        public bool ShowGrid { get; set; } = true;
        public string XAxisLabel { get; set; } = string.Empty;
        public string YAxisLabel { get; set; } = string.Empty;
    }
} 