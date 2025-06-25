import { Component, OnInit } from '@angular/core';
import { AdvancedAnalyticsService, ExecutiveSummaryAnalytics, TrendAnalysisData, BenchmarkingData, PredictiveAnalyticsData } from '../../services/advanced-analytics.service';

@Component({
  selector: 'app-analytics-dashboard',
  templateUrl: './analytics-dashboard.component.html',
  styleUrls: ['./analytics-dashboard.component.scss']
})
export class AnalyticsDashboardComponent implements OnInit {
  
  executiveSummary: ExecutiveSummaryAnalytics;
  trendAnalysis: TrendAnalysisData;
  benchmarking: BenchmarkingData;
  predictiveAnalytics: PredictiveAnalyticsData;
  
  loading = true;
  error: string;

  constructor(private analyticsService: AdvancedAnalyticsService) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = null;

    this.analyticsService.getAnalyticsDashboard(true).subscribe({
      next: (data) => {
        this.executiveSummary = data.executiveSummary;
        this.trendAnalysis = data.trendAnalysis;
        this.benchmarking = data.benchmarking;
        this.predictiveAnalytics = data.predictiveAnalytics;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load analytics dashboard data';
        this.loading = false;
        console.error('Analytics dashboard error:', error);
      }
    });
  }

  refreshData(): void {
    this.loadDashboardData();
  }

  getComplianceColor(score: number): string {
    if (score >= 80) return 'green';
    if (score >= 60) return 'orange';
    return 'red';
  }

  getTrendIcon(direction: string): string {
    switch (direction?.toLowerCase()) {
      case 'improving': return 'trending_up';
      case 'declining': return 'trending_down';
      default: return 'trending_flat';
    }
  }

  getTrendColor(direction: string): string {
    switch (direction?.toLowerCase()) {
      case 'improving': return 'green';
      case 'declining': return 'red';
      default: return 'gray';
    }
  }
} 