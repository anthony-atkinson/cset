import { Component, OnInit } from '@angular/core';
import { AdvancedAnalyticsService, ExecutiveSummaryAnalytics } from '../../services/advanced-analytics.service';

@Component({
  selector: 'app-executive-summary',
  templateUrl: './executive-summary.component.html',
  styleUrls: ['./executive-summary.component.scss']
})
export class ExecutiveSummaryComponent implements OnInit {
  
  executiveSummary: ExecutiveSummaryAnalytics;
  loading = true;
  error: string;

  constructor(private analyticsService: AdvancedAnalyticsService) { }

  ngOnInit(): void {
    this.loadExecutiveSummary();
  }

  loadExecutiveSummary(): void {
    this.loading = true;
    this.error = null;

    this.analyticsService.getExecutiveSummary().subscribe({
      next: (data) => {
        this.executiveSummary = data;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load executive summary';
        this.loading = false;
        console.error('Executive summary error:', error);
      }
    });
  }

  getComplianceColor(score: number): string {
    if (score >= 80) return 'green';
    if (score >= 60) return 'orange';
    return 'red';
  }

  getRiskLevel(score: number): string {
    if (score >= 70) return 'Low';
    if (score >= 40) return 'Medium';
    return 'High';
  }

  getRiskColor(score: number): string {
    if (score >= 70) return 'green';
    if (score >= 40) return 'orange';
    return 'red';
  }
} 