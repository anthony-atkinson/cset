import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../../services/config.service';

// Analytics models
export interface ExecutiveSummaryAnalytics {
  assessmentId: number;
  assessmentName: string;
  assessmentDate: Date;
  lastModified: Date;
  organizationSize: string;
  assetValue: string;
  sectorId: number;
  industryId: number;
  overallComplianceScore: number;
  totalQuestions: number;
  compliantQuestions: number;
  riskAreas: RiskArea[];
  improvementRecommendations: ImprovementRecommendation[];
  generatedDate: Date;
}

export interface TrendAnalysisData {
  assessmentId: number;
  timeframe: number;
  trendDirection: string;
  trendPercentage: number;
  trendPoints: TrendPoint[];
  generatedDate: Date;
}

export interface TrendPoint {
  date: Date;
  complianceScore: number;
  totalQuestions: number;
  compliantQuestions: number;
}

export interface BenchmarkingData {
  assessmentId: number;
  sectorId: number;
  industryId: number;
  currentScore: number;
  industryAverage: number;
  industryMedian: number;
  industryMin: number;
  industryMax: number;
  percentileRank: number;
  generatedDate: Date;
}

export interface PredictiveAnalyticsData {
  assessmentId: number;
  predictedScore3Months: number;
  predictedScore6Months: number;
  predictedScore12Months: number;
  confidenceLevel: number;
  trendStrength: number;
  riskPredictions: RiskPrediction[];
  generatedDate: Date;
}

export interface RiskPrediction {
  riskType: string;
  probability: number;
  impact: string;
  timeframe: string;
  mitigationStrategy: string;
}

export interface RiskArea {
  category: string;
  riskScore: number;
  questionCount: number;
  highRiskQuestions: string[];
}

export interface ImprovementRecommendation {
  questionNumber: string;
  priority: string;
  impact: string;
  estimatedEffort: string;
  description: string;
}

export interface CustomReportData {
  assessmentId: number;
  reportType: string;
  parameters: { [key: string]: any };
  data: any;
  generatedDate: Date;
}

@Injectable({
  providedIn: 'root'
})
export class AdvancedAnalyticsService {

  private apiUrl: string;

  constructor(
    private http: HttpClient,
    private configService: ConfigService
  ) {
    this.apiUrl = this.configService.apiUrl + 'advanced-analytics/';
  }

  /**
   * Get executive summary analytics
   */
  getExecutiveSummary(): Observable<ExecutiveSummaryAnalytics> {
    return this.http.get<ExecutiveSummaryAnalytics>(`${this.apiUrl}executive-summary`);
  }

  /**
   * Get trend analysis data
   */
  getTrendAnalysis(timeframe: number = 365): Observable<TrendAnalysisData> {
    return this.http.get<TrendAnalysisData>(`${this.apiUrl}trend-analysis?timeframe=${timeframe}`);
  }

  /**
   * Get benchmarking data
   */
  getBenchmarkingData(sectorId?: number, industryId?: number): Observable<BenchmarkingData> {
    let url = `${this.apiUrl}benchmarking`;
    const params: string[] = [];
    
    if (sectorId) {
      params.push(`sectorId=${sectorId}`);
    }
    if (industryId) {
      params.push(`industryId=${industryId}`);
    }
    
    if (params.length > 0) {
      url += '?' + params.join('&');
    }
    
    return this.http.get<BenchmarkingData>(url);
  }

  /**
   * Get predictive analytics data
   */
  getPredictiveAnalytics(): Observable<PredictiveAnalyticsData> {
    return this.http.get<PredictiveAnalyticsData>(`${this.apiUrl}predictive`);
  }

  /**
   * Get custom report data
   */
  getCustomReport(reportType: string, parameters: { [key: string]: any }): Observable<CustomReportData> {
    return this.http.post<CustomReportData>(`${this.apiUrl}custom-report?reportType=${reportType}`, parameters);
  }

  /**
   * Get comprehensive analytics dashboard
   */
  getAnalyticsDashboard(includePredictive: boolean = true): Observable<any> {
    return this.http.get(`${this.apiUrl}dashboard?includePredictive=${includePredictive}`);
  }
} 