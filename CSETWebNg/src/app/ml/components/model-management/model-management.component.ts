import { Component, OnInit } from '@angular/core';
import { MLService, MLModelMetadata } from '../../../services/ml.service';

@Component({
  selector: 'app-model-management',
  templateUrl: './model-management.component.html',
  styleUrls: ['./model-management.component.scss']
})
export class ModelManagementComponent implements OnInit {
  models: MLModelMetadata[] = [];
  loading = false;
  error: string | null = null;

  constructor(private mlService: MLService) {}

  ngOnInit(): void {
    this.fetchModels();
  }

  fetchModels(): void {
    this.loading = true;
    this.error = null;
    this.mlService.getRegisteredModels().subscribe({
      next: (models) => {
        this.models = models;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load models.';
        this.loading = false;
      }
    });
  }

  toggleModelStatus(model: MLModelMetadata): void {
    this.mlService.toggleModelStatus(model.modelId, !model.isActive).subscribe({
      next: () => this.fetchModels(),
      error: () => this.error = 'Failed to update model status.'
    });
  }

  deleteModel(model: MLModelMetadata): void {
    if (!confirm(`Are you sure you want to delete model '${model.modelName}'?`)) return;
    this.mlService.deleteModel(model.modelId).subscribe({
      next: () => this.fetchModels(),
      error: () => this.error = 'Failed to delete model.'
    });
  }

  viewModelDetails(model: MLModelMetadata): void {
    // Placeholder for viewing model details (could open a dialog or route to details page)
    alert(`Model details for: ${model.modelName}`);
  }
} 