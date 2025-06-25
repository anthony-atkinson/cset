import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MLService, ModelTrainingRequest, AssessmentDataPoint, MLPredictionResult, MLRecommendation, MLModelMetadata } from './ml.service';
import { ConfigService } from './config.service';

describe('MLService', () => {
  let service: MLService;
  let httpMock: HttpTestingController;
  let configService: jasmine.SpyObj<ConfigService>;

  const mockApiUrl = 'http://localhost:5000/api/';

  beforeEach(() => {
    const configSpy = jasmine.createSpyObj('ConfigService', [], { apiUrl: mockApiUrl });

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        MLService,
        { provide: ConfigService, useValue: configSpy }
      ]
    });

    service = TestBed.inject(MLService);
    httpMock = TestBed.inject(HttpTestingController);
    configService = TestBed.inject(ConfigService) as jasmine.SpyObj<ConfigService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Training Job Management', () => {
    const mockTrainingRequest: ModelTrainingRequest = {
      modelName: 'Test Model',
      modelType: 'classification',
      algorithm: 'random_forest',
      startDate: new Date('2024-01-01'),
      endDate: new Date('2024-01-31'),
      parameters: { n_estimators: 100 },
      validateModel: true,
      deployAfterTraining: false
    };

    const mockTrainingResult = {
      trainingJobId: 'job-123',
      modelId: 'model-456',
      status: 'running',
      startedAt: new Date(),
      metrics: { accuracy: 0.85 },
      trainingSamples: 1000,
      validationSamples: 200
    };

    it('should start a training job', () => {
      service.startTrainingJob(mockTrainingRequest).subscribe(result => {
        expect(result).toEqual(mockTrainingResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/start`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockTrainingRequest);
      req.flush(mockTrainingResult);
    });

    it('should get training job status', () => {
      const jobId = 'job-123';
      service.getTrainingJobStatus(jobId).subscribe(result => {
        expect(result).toEqual(mockTrainingResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/${jobId}/status`);
      expect(req.request.method).toBe('GET');
      req.flush(mockTrainingResult);
    });

    it('should get all training jobs', () => {
      const mockJobs = [mockTrainingResult];
      service.getTrainingJobs(true, 50).subscribe(jobs => {
        expect(jobs).toEqual(mockJobs);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/jobs?includeCompleted=true&limit=50`);
      expect(req.request.method).toBe('GET');
      req.flush(mockJobs);
    });

    it('should cancel a training job', () => {
      const jobId = 'job-123';
      service.cancelTrainingJob(jobId).subscribe(result => {
        expect(result).toBe(true);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/${jobId}/cancel`);
      expect(req.request.method).toBe('POST');
      req.flush(true);
    });

    it('should delete a training job', () => {
      const jobId = 'job-123';
      service.deleteTrainingJob(jobId).subscribe(result => {
        expect(result).toBe(true);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/${jobId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(true);
    });

    it('should get training job logs', () => {
      const jobId = 'job-123';
      const mockLogs = ['Training started', 'Epoch 1 completed', 'Training finished'];
      service.getTrainingJobLogs(jobId).subscribe(logs => {
        expect(logs).toEqual(mockLogs);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/${jobId}/logs`);
      expect(req.request.method).toBe('GET');
      req.flush(mockLogs);
    });

    it('should validate training request', () => {
      const mockValidationResult = {
        isValid: true,
        issues: [],
        statistics: { dataPoints: 1000, features: 10 }
      };

      service.validateTrainingRequest(mockTrainingRequest).subscribe(result => {
        expect(result).toEqual(mockValidationResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/validate`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockTrainingRequest);
      req.flush(mockValidationResult);
    });

    it('should get available algorithms', () => {
      const mockAlgorithms = ['random_forest', 'logistic_regression', 'neural_network'];
      service.getAvailableAlgorithms().subscribe(algorithms => {
        expect(algorithms).toEqual(mockAlgorithms);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/algorithms`);
      expect(req.request.method).toBe('GET');
      req.flush(mockAlgorithms);
    });

    it('should get training statistics', () => {
      const mockStats = {
        totalJobs: 10,
        runningJobs: 2,
        completedJobs: 7,
        failedJobs: 1,
        cancelledJobs: 0,
        averageTrainingTime: 300,
        successRate: 0.8
      };

      service.getTrainingStatistics().subscribe(stats => {
        expect(stats).toEqual(mockStats);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/statistics`);
      expect(req.request.method).toBe('GET');
      req.flush(mockStats);
    });
  });

  describe('Prediction Services', () => {
    const mockPredictionResult: MLPredictionResult = {
      predictionId: 'pred-123',
      modelId: 'model-456',
      predictedLabel: 'high_risk',
      confidence: 0.92,
      classProbabilities: { low_risk: 0.05, medium_risk: 0.03, high_risk: 0.92 },
      featureImportance: { feature1: 0.3, feature2: 0.7 },
      predictedAt: new Date(),
      inputFeatures: [1, 2, 3, 4, 5],
      metadata: { assessmentId: 123 }
    };

    const mockAssessmentData: AssessmentDataPoint = {
      assessmentId: 123,
      assessmentDate: new Date(),
      totalQuestions: 100,
      compliantQuestions: 85,
      complianceScore: 0.85,
      riskScore: 0.15,
      organizationSize: 'medium',
      assetValue: 'high',
      sectorId: 1,
      industryId: 2,
      categoryScores: { access_control: 0.9, data_protection: 0.8 }
    };

    it('should make a prediction using model and features', () => {
      const modelId = 'model-456';
      const features = [1, 2, 3, 4, 5];

      service.makePrediction(modelId, features).subscribe(result => {
        expect(result).toEqual(mockPredictionResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/prediction/make`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ modelId, features });
      req.flush(mockPredictionResult);
    });

    it('should make prediction from assessment data', () => {
      const modelId = 'model-456';

      service.predictFromAssessment(modelId, mockAssessmentData).subscribe(result => {
        expect(result).toEqual(mockPredictionResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/prediction/assessment`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ modelId, assessmentData: mockAssessmentData });
      req.flush(mockPredictionResult);
    });
  });

  describe('Recommendation Services', () => {
    const mockRecommendations: MLRecommendation[] = [
      {
        recommendationId: 'rec-123',
        title: 'Improve Access Controls',
        description: 'Implement multi-factor authentication',
        category: 'access_control',
        priority: 'high',
        impact: 'high',
        effort: 'medium',
        confidence: 0.95,
        relatedQuestions: ['AC-1', 'AC-2'],
        evidence: ['Low MFA adoption rate'],
        generatedAt: new Date()
      }
    ];

    it('should generate recommendations', () => {
      const modelId = 'model-456';

      service.generateRecommendations(mockAssessmentData, modelId).subscribe(recommendations => {
        expect(recommendations).toEqual(mockRecommendations);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/recommendations/generate`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ assessmentData: mockAssessmentData, modelId });
      req.flush(mockRecommendations);
    });
  });

  describe('Model Management', () => {
    const mockModel: MLModelMetadata = {
      modelId: 'model-456',
      modelName: 'Test Model',
      version: '1.0.0',
      modelType: 'classification',
      algorithm: 'random_forest',
      trainedAt: new Date(),
      accuracy: 0.85,
      precision: 0.82,
      recall: 0.88,
      f1Score: 0.85,
      trainingSamples: 1000,
      featureNames: ['feature1', 'feature2'],
      parameters: { n_estimators: 100 },
      isActive: true,
      modelFilePath: '/models/model-456.pkl'
    };

    it('should get registered models', () => {
      const mockModels = [mockModel];

      service.getRegisteredModels().subscribe(models => {
        expect(models).toEqual(mockModels);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/models`);
      expect(req.request.method).toBe('GET');
      req.flush(mockModels);
    });

    it('should get model by ID', () => {
      const modelId = 'model-456';

      service.getModelById(modelId).subscribe(model => {
        expect(model).toEqual(mockModel);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/models/${modelId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockModel);
    });

    it('should toggle model status', () => {
      const modelId = 'model-456';
      const isActive = false;

      service.toggleModelStatus(modelId, isActive).subscribe(result => {
        expect(result).toBe(true);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/models/${modelId}/toggle`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ isActive });
      req.flush(true);
    });

    it('should delete a model', () => {
      const modelId = 'model-456';

      service.deleteModel(modelId).subscribe(result => {
        expect(result).toBe(true);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/models/${modelId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(true);
    });
  });

  describe('Data Operations', () => {
    it('should collect assessment data', () => {
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-01-31');
      const mockData = [mockAssessmentData];

      service.collectAssessmentData(startDate, endDate).subscribe(data => {
        expect(data).toEqual(mockData);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/data/collect?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should preprocess data', () => {
      const rawData = [mockAssessmentData];
      const mockPreprocessedData = { features: [[1, 2, 3]], labels: ['high_risk'] };

      service.preprocessData(rawData).subscribe(data => {
        expect(data).toEqual(mockPreprocessedData);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/data/preprocess`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(rawData);
      req.flush(mockPreprocessedData);
    });

    it('should validate data quality', () => {
      const data = [mockAssessmentData];
      const mockValidationResult = {
        isValid: true,
        issues: [],
        statistics: { totalRecords: 1, missingValues: 0 }
      };

      service.validateDataQuality(data).subscribe(result => {
        expect(result).toEqual(mockValidationResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/data/validate`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(data);
      req.flush(mockValidationResult);
    });
  });

  describe('Observables and State Management', () => {
    it('should provide training jobs observable', () => {
      const mockJobs = [mockTrainingResult];
      
      service.trainingJobs$.subscribe(jobs => {
        expect(jobs).toEqual(mockJobs);
      });

      service.updateTrainingJobs(mockJobs);
    });

    it('should provide active models observable', () => {
      const mockModels = [mockModel];
      
      service.activeModels$.subscribe(models => {
        expect(models).toEqual(mockModels);
      });

      service.updateActiveModels(mockModels);
    });
  });

  describe('Performance and Analytics', () => {
    it('should get model performance summary', () => {
      const modelId = 'model-456';
      const mockSummary = {
        accuracy: 0.85,
        precision: 0.82,
        recall: 0.88,
        f1Score: 0.85,
        confusionMatrix: [[80, 20], [10, 90]]
      };

      service.getModelPerformanceSummary(modelId).subscribe(summary => {
        expect(summary).toEqual(mockSummary);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/models/${modelId}/performance`);
      expect(req.request.method).toBe('GET');
      req.flush(mockSummary);
    });

    it('should get feature importance', () => {
      const modelId = 'model-456';
      const mockFeatureImportance = { feature1: 0.3, feature2: 0.7 };

      service.getFeatureImportance(modelId).subscribe(importance => {
        expect(importance).toEqual(mockFeatureImportance);
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/models/${modelId}/features`);
      expect(req.request.method).toBe('GET');
      req.flush(mockFeatureImportance);
    });
  });

  describe('Error Handling', () => {
    it('should handle HTTP errors gracefully', () => {
      service.getAvailableAlgorithms().subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.status).toBe(500);
          expect(error.message).toContain('Internal Server Error');
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/algorithms`);
      req.flush('Internal Server Error', { status: 500, statusText: 'Internal Server Error' });
    });

    it('should handle network errors', () => {
      service.getAvailableAlgorithms().subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.name).toBe('NetworkError');
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}ml/training/algorithms`);
      req.error(new ErrorEvent('NetworkError'));
    });
  });
}); 