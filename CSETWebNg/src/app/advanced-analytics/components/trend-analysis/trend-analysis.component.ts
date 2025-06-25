import { Component, OnInit } from '@angular/core';
import { AdvancedAnalyticsService, TrendAnalysisData } from '../../services/advanced-analytics.service';

@Component({
  selector: 'app-trend-analysis',
  templateUrl: './trend-analysis.component.html',
  styleUrls: ['./trend-analysis.component.scss']
})
export class TrendAnalysisComponent implements OnInit {
  
  trendData: TrendAnalysisData;
  loading = true;
  error: string;

  constructor(private analyticsService: AdvancedAnalyticsService) { }

  ngOnInit(): void {
    this.loadTrendAnalysis();
  }

  loadTrendAnalysis(): void {
    this.loading = true;
    this.error = null;

    this.analyticsService.getTrendAnalysis().subscribe({
      next: (data) => {
        this.trendData = data;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load trend analysis';
        this.loading = false;
        console.error('Trend analysis error:', error);
      }
    });
  }
} 