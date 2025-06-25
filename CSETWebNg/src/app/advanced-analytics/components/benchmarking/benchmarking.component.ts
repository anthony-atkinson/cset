import { Component, OnInit } from '@angular/core';
import { AdvancedAnalyticsService, BenchmarkingData } from '../../services/advanced-analytics.service';

@Component({
  selector: 'app-benchmarking',
  templateUrl: './benchmarking.component.html',
  styleUrls: ['./benchmarking.component.scss']
})
export class BenchmarkingComponent implements OnInit {
  
  benchmarkingData: BenchmarkingData;
  loading = true;
  error: string;

  constructor(private analyticsService: AdvancedAnalyticsService) { }

  ngOnInit(): void {
    this.loadBenchmarkingData();
  }

  loadBenchmarkingData(): void {
    this.loading = true;
    this.error = null;

    this.analyticsService.getBenchmarkingData().subscribe({
      next: (data) => {
        this.benchmarkingData = data;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load benchmarking data';
        this.loading = false;
        console.error('Benchmarking error:', error);
      }
    });
  }
} 