using ScottPlot;
using System.Drawing;

namespace CSETWebBlazor.Services
{
    public class ChartService : IChartService
    {
        public async Task<byte[]> GenerateLineChartAsync(ChartData data, ChartOptions options)
        {
            return await Task.Run(() =>
            {
                var plot = new Plot(options.Width, options.Height);
                
                // Add title
                if (!string.IsNullOrEmpty(options.Title))
                {
                    plot.Title(options.Title);
                }

                // Add datasets
                for (int i = 0; i < data.Datasets.Count; i++)
                {
                    var dataset = data.Datasets[i];
                    var scatter = plot.AddScatter(
                        Enumerable.Range(0, dataset.Data.Count).Select(x => (double)x).ToArray(),
                        dataset.Data.ToArray(),
                        label: dataset.Label,
                        color: ColorTranslator.FromHtml(dataset.BorderColor)
                    );
                    
                    if (dataset.Fill)
                    {
                        scatter.Fill = true;
                        scatter.FillColor = ColorTranslator.FromHtml(dataset.BackgroundColor);
                    }
                }

                // Configure axes
                if (!string.IsNullOrEmpty(options.XAxisLabel))
                    plot.XAxis.Label(options.XAxisLabel);
                if (!string.IsNullOrEmpty(options.YAxisLabel))
                    plot.YAxis.Label(options.YAxisLabel);

                // Show legend if requested
                if (options.ShowLegend && data.Datasets.Count > 1)
                    plot.Legend();

                // Show grid if requested
                if (options.ShowGrid)
                {
                    plot.Grid(true);
                }

                return plot.GetImageBytes();
            });
        }

        public async Task<byte[]> GenerateBarChartAsync(ChartData data, ChartOptions options)
        {
            return await Task.Run(() =>
            {
                var plot = new Plot(options.Width, options.Height);
                
                if (!string.IsNullOrEmpty(options.Title))
                {
                    plot.Title(options.Title);
                }

                // Add bar plots for each dataset
                for (int i = 0; i < data.Datasets.Count; i++)
                {
                    var dataset = data.Datasets[i];
                    var barPlot = plot.AddBar(
                        dataset.Data.ToArray(),
                        label: dataset.Label,
                        color: ColorTranslator.FromHtml(dataset.BackgroundColor)
                    );
                }

                // Configure axes
                if (!string.IsNullOrEmpty(options.XAxisLabel))
                    plot.XAxis.Label(options.XAxisLabel);
                if (!string.IsNullOrEmpty(options.YAxisLabel))
                    plot.YAxis.Label(options.YAxisLabel);

                if (options.ShowLegend && data.Datasets.Count > 1)
                    plot.Legend();

                if (options.ShowGrid)
                {
                    plot.Grid(true);
                }

                return plot.GetImageBytes();
            });
        }

        public async Task<byte[]> GeneratePieChartAsync(ChartData data, ChartOptions options)
        {
            return await Task.Run(() =>
            {
                var plot = new Plot(options.Width, options.Height);
                
                if (!string.IsNullOrEmpty(options.Title))
                {
                    plot.Title(options.Title);
                }

                // For pie chart, we use the first dataset
                if (data.Datasets.Count > 0)
                {
                    var dataset = data.Datasets[0];
                    var piePlot = plot.AddPie(dataset.Data.ToArray());
                    
                    // Set colors for each slice
                    for (int i = 0; i < dataset.Data.Count; i++)
                    {
                        piePlot.SliceColors[i] = ColorTranslator.FromHtml(dataset.BackgroundColor);
                    }
                }

                return plot.GetImageBytes();
            });
        }

        public async Task<byte[]> GenerateDoughnutChartAsync(ChartData data, ChartOptions options)
        {
            return await Task.Run(() =>
            {
                var plot = new Plot(options.Width, options.Height);
                
                if (!string.IsNullOrEmpty(options.Title))
                {
                    plot.Title(options.Title);
                }

                // For doughnut chart, we use the first dataset
                if (data.Datasets.Count > 0)
                {
                    var dataset = data.Datasets[0];
                    var piePlot = plot.AddPie(dataset.Data.ToArray());
                    
                    // Set colors for each slice
                    for (int i = 0; i < dataset.Data.Count; i++)
                    {
                        piePlot.SliceColors[i] = ColorTranslator.FromHtml(dataset.BackgroundColor);
                    }
                    
                    // Make it a doughnut by setting inner radius
                    piePlot.InnerRadius = 0.3;
                }

                return plot.GetImageBytes();
            });
        }

        public async Task<byte[]> GenerateScatterPlotAsync(ChartData data, ChartOptions options)
        {
            return await Task.Run(() =>
            {
                var plot = new Plot(options.Width, options.Height);
                
                if (!string.IsNullOrEmpty(options.Title))
                {
                    plot.Title(options.Title);
                }

                // For scatter plot, we need X and Y coordinates
                // Assuming the first dataset contains X values and second contains Y values
                if (data.Datasets.Count >= 2)
                {
                    var xData = data.Datasets[0].Data;
                    var yData = data.Datasets[1].Data;
                    
                    var scatter = plot.AddScatter(
                        xData.ToArray(),
                        yData.ToArray(),
                        label: data.Datasets[0].Label,
                        color: ColorTranslator.FromHtml(data.Datasets[0].BorderColor)
                    );
                }

                // Configure axes
                if (!string.IsNullOrEmpty(options.XAxisLabel))
                    plot.XAxis.Label(options.XAxisLabel);
                if (!string.IsNullOrEmpty(options.YAxisLabel))
                    plot.YAxis.Label(options.YAxisLabel);

                if (options.ShowGrid)
                {
                    plot.Grid(true);
                }

                return plot.GetImageBytes();
            });
        }
    }
} 