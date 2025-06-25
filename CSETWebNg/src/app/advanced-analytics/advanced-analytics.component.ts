import { Component, OnInit } from '@angular/core';
import { AdvancedAnalyticsService } from './services/advanced-analytics.service';

@Component({
  selector: 'app-advanced-analytics',
  templateUrl: './advanced-analytics.component.html',
  styleUrls: ['./advanced-analytics.component.scss']
})
export class AdvancedAnalyticsComponent implements OnInit {
  
  constructor(private analyticsService: AdvancedAnalyticsService) { }

  ngOnInit(): void {
    // Initialize analytics dashboard
  }
} 