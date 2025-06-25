import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MLService, MLPredictionResult, MLModelMetadata, AssessmentDataPoint } from '../../../services/ml.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-predictions',
  templateUrl: './predictions.component.html',
  styleUrls: ['./predictions.component.scss']
})
export class PredictionsComponent implements OnInit, OnDestroy {
  
  predictionForm: FormGroup;
  availableModels: MLModelMetadata[] = [];
  predictionResult: MLPredictionResult;
  loading = false;
  makingPrediction = false;
  error: string;
  
  // Feature input options
  featureInputModes = [
    { value: 'manual', label: 'Manual Input' },
    { value: 'assessment', label: 'From Assessment' }
  ];
  
  // Sample feature names for manual input
  sampleFeatureNames: string[] = [
    'Compliance Score',
    'Security Controls',
    'Incident Response Time',
    'Vulnerability Count',
    'Patch Level',
    'Access Controls',
    'Network Segmentation',
    'Data Encryption'
  ];
  
  private subscriptions = new Subscription();

  constructor(
    private mlService: MLService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadAvailableModels();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  private initForm(): void {
    this.predictionForm = this.fb.group({
      modelId: ['', Validators.required],
      inputMode: ['manual', Validators.required],
      features: this.fb.array([]),
      assessmentId: [''],
      customFeatures: this.fb.group({
        complianceScore: [50, [Validators.min(0), Validators.max(100)]],
        securityControls: [5, [Validators.min(0), Validators.max(20)]],
        incidentResponseTime: [24, [Validators.min(1), Validators.max(168)]],
        vulnerabilityCount: [10, [Validators.min(0), Validators.max(1000)]],
        patchLevel: [80, [Validators.min(0), Validators.max(100)]],
        accessControls: [7, [Validators.min(0), Validators.max(10)]],
        networkSegmentation: [6, [Validators.min(0), Validators.max(10)]],
        dataEncryption: [8, [Validators.min(0), Validators.max(10)]]
      })
    });

    // Watch for input mode changes
    this.predictionForm.get('inputMode')?.valueChanges.subscribe(mode => {
      if (mode === 'manual') {
        this.predictionForm.get('assessmentId')?.clearValidators();
        this.predictionForm.get('customFeatures')?.enable();
      } else {
        this.predictionForm.get('assessmentId')?.setValidators([Validators.required]);
        this.predictionForm.get('customFeatures')?.disable();
      }
      this.predictionForm.get('assessmentId')?.updateValueAndValidity();
    });
  }

  loadAvailableModels(): void {
    this.loading = true;
    this.error = null;

    const sub = this.mlService.getRegisteredModels().subscribe({
      next: (models) => {
        this.availableModels = models.filter(m => m.isActive);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading models:', error);
        this.error = 'Failed to load available models';
        this.loading = false;
      }
    });

    this.subscriptions.add(sub);
  }

  makePrediction(): void {
    if (this.predictionForm.invalid) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    const formValue = this.predictionForm.value;
    this.makingPrediction = true;
    this.predictionResult = null;
    this.error = null;

    if (formValue.inputMode === 'manual') {
      // Convert custom features to array
      const features = this.extractFeaturesFromForm(formValue.customFeatures);
      this.makeManualPrediction(formValue.modelId, features);
    } else {
      // Use assessment data
      const assessmentData = this.createAssessmentDataPoint(formValue.assessmentId);
      this.makeAssessmentPrediction(formValue.modelId, assessmentData);
    }
  }

  private makeManualPrediction(modelId: string, features: number[]): void {
    const sub = this.mlService.makePrediction(modelId, features).subscribe({
      next: (result) => {
        this.predictionResult = result;
        this.makingPrediction = false;
        this.snackBar.open('Prediction completed successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Prediction error:', error);
        this.error = 'Failed to make prediction';
        this.makingPrediction = false;
        this.snackBar.open('Failed to make prediction', 'Close', { duration: 3000 });
      }
    });

    this.subscriptions.add(sub);
  }

  private makeAssessmentPrediction(modelId: string, assessmentData: AssessmentDataPoint): void {
    const sub = this.mlService.predictFromAssessment(modelId, assessmentData).subscribe({
      next: (result) => {
        this.predictionResult = result;
        this.makingPrediction = false;
        this.snackBar.open('Prediction completed successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Assessment prediction error:', error);
        this.error = 'Failed to make prediction from assessment';
        this.makingPrediction = false;
        this.snackBar.open('Failed to make prediction from assessment', 'Close', { duration: 3000 });
      }
    });

    this.subscriptions.add(sub);
  }

  private extractFeaturesFromForm(customFeatures: any): number[] {
    return [
      customFeatures.complianceScore,
      customFeatures.securityControls,
      customFeatures.incidentResponseTime,
      customFeatures.vulnerabilityCount,
      customFeatures.patchLevel,
      customFeatures.accessControls,
      customFeatures.networkSegmentation,
      customFeatures.dataEncryption
    ];
  }

  private createAssessmentDataPoint(assessmentId: string): AssessmentDataPoint {
    // Create a sample assessment data point
    return {
      assessmentId: parseInt(assessmentId),
      assessmentDate: new Date(),
      totalQuestions: 100,
      compliantQuestions: 75,
      complianceScore: 75,
      riskScore: 25,
      organizationSize: 'Medium',
      assetValue: '1000000',
      sectorId: 1,
      industryId: 1,
      categoryScores: {
        'Access Control': 80,
        'Data Protection': 70,
        'Incident Response': 75,
        'Network Security': 85
      }
    };
  }

  getConfidenceColor(confidence: number): string {
    if (confidence >= 0.8) return 'accent';
    if (confidence >= 0.6) return 'primary';
    return 'warn';
  }

  getRiskColor(riskLevel: string): string {
    switch (riskLevel.toLowerCase()) {
      case 'lowrisk': return 'accent';
      case 'mediumrisk': return 'primary';
      case 'highrisk': return 'warn';
      default: return 'primary';
    }
  }

  getRiskLabel(riskLevel: string): string {
    switch (riskLevel.toLowerCase()) {
      case 'lowrisk': return 'Low Risk';
      case 'mediumrisk': return 'Medium Risk';
      case 'highrisk': return 'High Risk';
      default: return riskLevel;
    }
  }

  clearResults(): void {
    this.predictionResult = null;
    this.error = null;
  }

  refreshModels(): void {
    this.loadAvailableModels();
  }
} 