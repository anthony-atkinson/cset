import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { ConfigService } from './config.service';

// ML Models
export interface ModelTrainingRequest {
  modelName: string;
  modelType: string;
  algorithm: string;
  startDate: Date;
  endDate: Date;
  parameters: { [key: string]: any };
  validateModel: boolean;
  deployAfterTraining: boolean;
}

export interface ModelTrainingResult {
  trainingJobId: string;
  modelId: string;
  status: string;
  startedAt: Date;
  completedAt?: Date;
  duration?: string;
  metrics: { [key: string]: number };
  errorMessage?: string;
  trainingSamples: number;
  validationSamples: number;
}

export interface MLPredictionResult {
  predictionId: string;
  modelId: string;
  predictedLabel: string;
  confidence: number;
  classProbabilities: { [key: string]: number };
  featureImportance: { [key: string]: number };
  predictedAt: Date;
  inputFeatures: number[];
  metadata: { [key: string]: any };
}

export interface MLRecommendation {
  recommendationId: string;
  title: string;
  description: string;
  category: string;
  priority: string;
  impact: string;
  effort: string;
  confidence: number;
  relatedQuestions: string[];
  evidence: string[];
  generatedAt: Date;
}

export interface AssessmentDataPoint {
  assessmentId: number;
  assessmentDate: Date;
  totalQuestions: number;
  compliantQuestions: number;
  complianceScore: number;
  riskScore: number;
  organizationSize: string;
  assetValue: string;
  sectorId: number;
  industryId: number;
  categoryScores: { [key: string]: number };
}

export interface MLModelMetadata {
  modelId: string;
  modelName: string;
  version: string;
  modelType: string;
  algorithm: string;
  trainedAt: Date;
  accuracy: number;
  precision: number;
  recall: number;
  f1Score: number;
  trainingSamples: number;
  featureNames: string[];
  parameters: { [key: string]: any };
  isActive: boolean;
  modelFilePath: string;
}

export interface DataValidationResult {
  isValid: boolean;
  issues: string[];
  statistics: { [key: string]: any };
}

export interface TrainingStatistics {
  totalJobs: number;
  runningJobs: number;
  completedJobs: number;
  failedJobs: number;
  cancelledJobs: number;
  averageTrainingTime: number;
  successRate: number;
}

@Injectable({
  providedIn: 'root'
})
export class MLService {

  private apiUrl: string;
  private trainingJobsSubject = new BehaviorSubject<ModelTrainingResult[]>([]);
  private activeModelsSubject = new BehaviorSubject<MLModelMetadata[]>([]);

  constructor(
    private http: HttpClient,
    private configService: ConfigService
  ) {
    this.apiUrl = this.configService.apiUrl + 'ml/';
  }

  // Training Job Management
  /**
   * Start a new model training job
   */
  startTrainingJob(request: ModelTrainingRequest): Observable<ModelTrainingResult> {
    return this.http.post<ModelTrainingResult>(`${this.apiUrl}training/start`, request);
  }

  /**
   * Get training job status
   */
  getTrainingJobStatus(jobId: string): Observable<ModelTrainingResult> {
    return this.http.get<ModelTrainingResult>(`${this.apiUrl}training/${jobId}/status`);
  }

  /**
   * Get all training jobs
   */
  getTrainingJobs(includeCompleted: boolean = true, limit: number = 50): Observable<ModelTrainingResult[]> {
    return this.http.get<ModelTrainingResult[]>(`${this.apiUrl}training/jobs?includeCompleted=${includeCompleted}&limit=${limit}`);
  }

  /**
   * Cancel a training job
   */
  cancelTrainingJob(jobId: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}training/${jobId}/cancel`, {});
  }

  /**
   * Delete a training job
   */
  deleteTrainingJob(jobId: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}training/${jobId}`);
  }

  /**
   * Get training job logs
   */
  getTrainingJobLogs(jobId: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}training/${jobId}/logs`);
  }

  /**
   * Validate training request
   */
  validateTrainingRequest(request: ModelTrainingRequest): Observable<DataValidationResult> {
    return this.http.post<DataValidationResult>(`${this.apiUrl}training/validate`, request);
  }

  /**
   * Get available algorithms
   */
  getAvailableAlgorithms(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}training/algorithms`);
  }

  /**
   * Get training statistics
   */
  getTrainingStatistics(): Observable<TrainingStatistics> {
    return this.http.get<TrainingStatistics>(`${this.apiUrl}training/statistics`);
  }

  // Prediction Services
  /**
   * Make a prediction using model and features
   */
  makePrediction(modelId: string, features: number[]): Observable<MLPredictionResult> {
    return this.http.post<MLPredictionResult>(`${this.apiUrl}prediction/make`, {
      modelId,
      features
    });
  }

  /**
   * Make prediction from assessment data
   */
  predictFromAssessment(modelId: string, assessmentData: AssessmentDataPoint): Observable<MLPredictionResult> {
    return this.http.post<MLPredictionResult>(`${this.apiUrl}prediction/assessment`, {
      modelId,
      assessmentData
    });
  }

  /**
   * Generate recommendations from assessment data
   */
  generateRecommendations(assessmentData: AssessmentDataPoint, modelId?: string): Observable<MLRecommendation[]> {
    const payload: any = { assessmentData };
    if (modelId) {
      payload.modelId = modelId;
    }
    return this.http.post<MLRecommendation[]>(`${this.apiUrl}prediction/recommendations`, payload);
  }

  // Model Management
  /**
   * Get all registered models
   */
  getRegisteredModels(): Observable<MLModelMetadata[]> {
    return this.http.get<MLModelMetadata[]>(`${this.apiUrl}models`);
  }

  /**
   * Get model by ID
   */
  getModelById(modelId: string): Observable<MLModelMetadata> {
    return this.http.get<MLModelMetadata>(`${this.apiUrl}models/${modelId}`);
  }

  /**
   * Activate/deactivate model
   */
  toggleModelStatus(modelId: string, isActive: boolean): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}models/${modelId}/status`, { isActive });
  }

  /**
   * Delete model
   */
  deleteModel(modelId: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}models/${modelId}`);
  }

  // Data Pipeline
  /**
   * Collect assessment data for training
   */
  collectAssessmentData(startDate: Date, endDate: Date): Observable<AssessmentDataPoint[]> {
    return this.http.get<AssessmentDataPoint[]>(`${this.apiUrl}data/collect?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`);
  }

  /**
   * Preprocess data for training
   */
  preprocessData(rawData: AssessmentDataPoint[]): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}data/preprocess`, rawData);
  }

  /**
   * Validate data quality
   */
  validateDataQuality(data: AssessmentDataPoint[]): Observable<DataValidationResult> {
    return this.http.post<DataValidationResult>(`${this.apiUrl}data/validate`, data);
  }

  // Observable streams for real-time updates
  get trainingJobs$(): Observable<ModelTrainingResult[]> {
    return this.trainingJobsSubject.asObservable();
  }

  get activeModels$(): Observable<MLModelMetadata[]> {
    return this.activeModelsSubject.asObservable();
  }

  // Update methods for reactive UI
  updateTrainingJobs(jobs: ModelTrainingResult[]): void {
    this.trainingJobsSubject.next(jobs);
  }

  updateActiveModels(models: MLModelMetadata[]): void {
    this.activeModelsSubject.next(models);
  }

  // Utility methods
  /**
   * Poll training job status
   */
  pollTrainingJobStatus(jobId: string, interval: number = 5000): Observable<ModelTrainingResult> {
    return new Observable(observer => {
      const poll = () => {
        this.getTrainingJobStatus(jobId).subscribe({
          next: (result) => {
            observer.next(result);
            if (result.status === 'Running') {
              setTimeout(poll, interval);
            } else {
              observer.complete();
            }
          },
          error: (error) => observer.error(error)
        });
      };
      poll();
    });
  }

  /**
   * Get model performance summary
   */
  getModelPerformanceSummary(modelId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}models/${modelId}/performance`);
  }

  /**
   * Get feature importance for a model
   */
  getFeatureImportance(modelId: string): Observable<{ [key: string]: number }> {
    return this.http.get<{ [key: string]: number }>(`${this.apiUrl}models/${modelId}/features`);
  }
} 