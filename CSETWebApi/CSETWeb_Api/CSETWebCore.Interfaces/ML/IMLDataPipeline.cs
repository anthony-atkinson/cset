//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Model.ML;

namespace CSETWebCore.Interfaces.ML
{
    /// <summary>
    /// Interface for ML data pipeline operations
    /// </summary>
    public interface IMLDataPipeline
    {
        /// <summary>
        /// Collects assessment data for ML training
        /// </summary>
        /// <param name="startDate">Start date for data collection</param>
        /// <param name="endDate">End date for data collection</param>
        /// <returns>Collected assessment data</returns>
        Task<List<AssessmentDataPoint>> CollectAssessmentDataAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Preprocesses assessment data for ML training
        /// </summary>
        /// <param name="rawData">Raw assessment data</param>
        /// <returns>Preprocessed data ready for training</returns>
        Task<PreprocessedData> PreprocessDataAsync(List<AssessmentDataPoint> rawData);

        /// <summary>
        /// Extracts features from assessment data for ML models
        /// </summary>
        /// <param name="dataPoint">Assessment data point</param>
        /// <returns>Feature vector</returns>
        double[] ExtractFeatures(AssessmentDataPoint dataPoint);

        /// <summary>
        /// Creates a data pipeline configuration
        /// </summary>
        /// <returns>Pipeline configuration</returns>
        MLPipelineConfiguration GetPipelineConfiguration();

        /// <summary>
        /// Validates data quality for ML training
        /// </summary>
        /// <param name="data">Data to validate</param>
        /// <returns>Validation results</returns>
        Task<DataValidationResult> ValidateDataQualityAsync(List<AssessmentDataPoint> data);
    }
} 