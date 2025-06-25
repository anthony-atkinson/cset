import { Component, OnInit, OnDestroy } from '@angular/core';
import { MLService, TrainingStatistics, MLModelMetadata } from '../services/ml.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-ml-dashboard',
  templateUrl: './ml-dashboard.component.html',
  styleUrls: ['./ml-dashboard.component.scss']
})
export class MLDashboardComponent implements OnInit, OnDestroy {
  
  trainingStats: TrainingStatistics;
  activeModels: MLModelMetadata[] = [];
  loading = true;
  error: string;
  
  private subscriptions = new Subscription();

  constructor(private mlService: MLService) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = null;

    // Load training statistics
    const statsSub = this.mlService.getTrainingStatistics().subscribe({
      next: (stats) => {
        this.trainingStats = stats;
        this.checkLoadingComplete();
      },
      error: (error) => {
        console.error('Error loading training statistics:', error);
        this.error = 'Failed to load training statistics';
        this.checkLoadingComplete();
      }
    });

    // Load active models
    const modelsSub = this.mlService.getRegisteredModels().subscribe({
      next: (models) => {
        this.activeModels = models.filter(m => m.isActive);
        this.mlService.updateActiveModels(this.activeModels);
        this.checkLoadingComplete();
      },
      error: (error) => {
        console.error('Error loading models:', error);
        this.error = 'Failed to load models';
        this.checkLoadingComplete();
      }
    });

    // Load training jobs
    const jobsSub = this.mlService.getTrainingJobs(true, 10).subscribe({
      next: (jobs) => {
        this.mlService.updateTrainingJobs(jobs);
        this.checkLoadingComplete();
      },
      error: (error) => {
        console.error('Error loading training jobs:', error);
        this.checkLoadingComplete();
      }
    });

    // Combine subscriptions
    this.subscriptions.add(statsSub);
    this.subscriptions.add(modelsSub);
    this.subscriptions.add(jobsSub);
  }

  private checkLoadingComplete(): void {
    // Simple approach - set loading to false after a reasonable delay
    setTimeout(() => {
      this.loading = false;
    }, 1000);
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Running': return 'primary';
      case 'Completed': return 'accent';
      case 'Failed': return 'warn';
      case 'Cancelled': return 'warn';
      default: return 'primary';
    }
  }

  getAccuracyColor(accuracy: number): string {
    if (accuracy >= 0.9) return 'accent';
    if (accuracy >= 0.8) return 'primary';
    if (accuracy >= 0.7) return 'warn';
    return 'warn';
  }

  formatDuration(duration: string): string {
    if (!duration) return 'N/A';
    return duration;
  }

  refreshData(): void {
    this.loadDashboardData();
  }
} 