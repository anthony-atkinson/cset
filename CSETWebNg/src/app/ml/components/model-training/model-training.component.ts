import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MLService, ModelTrainingRequest, ModelTrainingResult, DataValidationResult } from '../../../services/ml.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-model-training',
  templateUrl: './model-training.component.html',
  styleUrls: ['./model-training.component.scss']
})
export class ModelTrainingComponent implements OnInit, OnDestroy {
  
  trainingForm: FormGroup;
  availableAlgorithms: string[] = [];
  modelTypes: string[] = ['Classification', 'Regression', 'Clustering'];
  loading = false;
  validating = false;
  validationResult: DataValidationResult;
  
  private subscriptions = new Subscription();

  constructor(
    private mlService: MLService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadAlgorithms();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  private initForm(): void {
    this.trainingForm = this.fb.group({
      modelName: ['', [Validators.required, Validators.minLength(3)]],
      modelType: ['Classification', Validators.required],
      algorithm: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      validateModel: [true],
      deployAfterTraining: [false],
      parameters: this.fb.group({
        maxDepth: [10, [Validators.min(1), Validators.max(50)]],
        nEstimators: [100, [Validators.min(10), Validators.max(1000)]],
        learningRate: [0.1, [Validators.min(0.01), Validators.max(1.0)]],
        randomState: [42]
      })
    });

    // Set default dates
    const endDate = new Date();
    const startDate = new Date();
    startDate.setFullYear(startDate.getFullYear() - 1);
    
    this.trainingForm.patchValue({
      startDate,
      endDate
    });
  }

  loadAlgorithms(): void {
    const sub = this.mlService.getAvailableAlgorithms().subscribe({
      next: (algorithms) => {
        this.availableAlgorithms = algorithms;
        if (algorithms.length > 0) {
          this.trainingForm.patchValue({ algorithm: algorithms[0] });
        }
      },
      error: (error) => {
        console.error('Error loading algorithms:', error);
        this.snackBar.open('Failed to load algorithms', 'Close', { duration: 3000 });
      }
    });
    this.subscriptions.add(sub);
  }

  validateRequest(): void {
    if (this.trainingForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.validating = true;
    const request = this.buildTrainingRequest();

    const sub = this.mlService.validateTrainingRequest(request).subscribe({
      next: (result) => {
        this.validationResult = result;
        this.validating = false;
        
        if (result.isValid) {
          this.snackBar.open('Training request is valid', 'Close', { duration: 3000 });
        } else {
          this.snackBar.open('Training request has issues', 'Close', { duration: 3000 });
        }
      },
      error: (error) => {
        console.error('Validation error:', error);
        this.snackBar.open('Failed to validate request', 'Close', { duration: 3000 });
        this.validating = false;
      }
    });
    this.subscriptions.add(sub);
  }

  startTraining(): void {
    if (this.trainingForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    const request = this.buildTrainingRequest();

    const sub = this.mlService.startTrainingJob(request).subscribe({
      next: (result) => {
        this.loading = false;
        this.snackBar.open(`Training job started: ${result.trainingJobId}`, 'Close', { duration: 5000 });
        this.trainingForm.reset();
        this.initForm();
        this.validationResult = null;
      },
      error: (error) => {
        console.error('Training error:', error);
        this.snackBar.open('Failed to start training job', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
    this.subscriptions.add(sub);
  }

  private buildTrainingRequest(): ModelTrainingRequest {
    const formValue = this.trainingForm.value;
    return {
      modelName: formValue.modelName,
      modelType: formValue.modelType,
      algorithm: formValue.algorithm,
      startDate: formValue.startDate,
      endDate: formValue.endDate,
      parameters: formValue.parameters,
      validateModel: formValue.validateModel,
      deployAfterTraining: formValue.deployAfterTraining
    };
  }

  private markFormGroupTouched(): void {
    Object.keys(this.trainingForm.controls).forEach(key => {
      const control = this.trainingForm.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        Object.keys(control.controls).forEach(nestedKey => {
          control.get(nestedKey)?.markAsTouched();
        });
      }
    });
  }

  getErrorMessage(controlName: string): string {
    const control = this.trainingForm.get(controlName);
    if (control?.hasError('required')) {
      return `${controlName} is required`;
    }
    if (control?.hasError('minlength')) {
      return `${controlName} must be at least ${control.getError('minlength').requiredLength} characters`;
    }
    if (control?.hasError('min')) {
      return `${controlName} must be at least ${control.getError('min').min}`;
    }
    if (control?.hasError('max')) {
      return `${controlName} must be at most ${control.getError('max').max}`;
    }
    return '';
  }

  getParameterErrorMessage(controlName: string): string {
    const control = this.trainingForm.get(`parameters.${controlName}`);
    if (control?.hasError('min')) {
      return `Must be at least ${control.getError('min').min}`;
    }
    if (control?.hasError('max')) {
      return `Must be at most ${control.getError('max').max}`;
    }
    return '';
  }

  onModelTypeChange(): void {
    // Reset algorithm when model type changes
    this.trainingForm.patchValue({ algorithm: '' });
  }

  onAlgorithmChange(): void {
    // Update parameters based on algorithm
    const algorithm = this.trainingForm.get('algorithm')?.value;
    const parameters = this.trainingForm.get('parameters');
    
    if (parameters) {
      switch (algorithm) {
        case 'RandomForest':
          parameters.patchValue({
            maxDepth: 10,
            nEstimators: 100,
            learningRate: 0.1
          });
          break;
        case 'GradientBoosting':
          parameters.patchValue({
            maxDepth: 6,
            nEstimators: 100,
            learningRate: 0.1
          });
          break;
        case 'NeuralNetwork':
          parameters.patchValue({
            maxDepth: 3,
            nEstimators: 100,
            learningRate: 0.01
          });
          break;
        default:
          parameters.patchValue({
            maxDepth: 10,
            nEstimators: 100,
            learningRate: 0.1
          });
      }
    }
  }
} 