import { Component, OnInit } from '@angular/core';
import { AdvancedAnalyticsService, PredictiveAnalyticsData } from '../../services/advanced-analytics.service';

@Component({
  selector: 'app-predictive-analytics',
  templateUrl: './predictive-analytics.component.html',
  styleUrls: ['./predictive-analytics.component.scss']
})
export class PredictiveAnalyticsComponent implements OnInit {
  
  predictiveData: PredictiveAnalyticsData;
  loading = true;
  error: string;

  constructor(private analyticsService: AdvancedAnalyticsService) { }

  ngOnInit(): void {
    this.loadPredictiveAnalytics();
  }

  loadPredictiveAnalytics(): void {
    this.loading = true;
    this.error = null;

    this.analyticsService.getPredictiveAnalytics().subscribe({
      next: (data) => {
        this.predictiveData = data;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load predictive analytics';
        this.loading = false;
        console.error('Predictive analytics error:', error);
      }
    });
  }
} 