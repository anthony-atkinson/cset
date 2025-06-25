import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MLService, MLRecommendation, MLModelMetadata, AssessmentDataPoint } from '../../../services/ml.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-recommendations',
  templateUrl: './recommendations.component.html',
  styleUrls: ['./recommendations.component.scss']
})
export class RecommendationsComponent implements OnInit, OnDestroy {
  
  recommendationsForm: FormGroup;
  availableModels: MLModelMetadata[] = [];
  recommendations: MLRecommendation[] = [];
  loading = false;
  generatingRecommendations = false;
  error: string;
  
  // Filter options
  priorityFilters = [
    { value: 'all', label: 'All Priorities' },
    { value: 'high', label: 'High Priority' },
    { value: 'medium', label: 'Medium Priority' },
    { value: 'low', label: 'Low Priority' }
  ];
  
  categoryFilters = [
    { value: 'all', label: 'All Categories' },
    { value: 'ML Insights', label: 'ML Insights' },
    { value: 'Security Controls', label: 'Security Controls' },
    { value: 'Compliance', label: 'Compliance' },
    { value: 'Risk Management', label: 'Risk Management' },
    { value: 'Incident Response', label: 'Incident Response' }
  ];
  
  // Current filters
  selectedPriority = 'all';
  selectedCategory = 'all';
  selectedSortBy = 'priority';
  
  // Sort options
  sortOptions = [
    { value: 'priority', label: 'Priority' },
    { value: 'impact', label: 'Impact' },
    { value: 'effort', label: 'Effort' },
    { value: 'confidence', label: 'Confidence' },
    { value: 'generatedAt', label: 'Date Generated' }
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
    this.recommendationsForm = this.fb.group({
      assessmentId: ['', Validators.required],
      modelId: [''],
      includeHistoricalData: [true],
      maxRecommendations: [10, [Validators.min(1), Validators.max(50)]]
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

  generateRecommendations(): void {
    if (this.recommendationsForm.invalid) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    const formValue = this.recommendationsForm.value;
    this.generatingRecommendations = true;
    this.recommendations = [];
    this.error = null;

    // Create assessment data point
    const assessmentData = this.createAssessmentDataPoint(formValue.assessmentId);

    const sub = this.mlService.generateRecommendations(assessmentData, formValue.modelId).subscribe({
      next: (results) => {
        this.recommendations = results.slice(0, formValue.maxRecommendations);
        this.generatingRecommendations = false;
        this.snackBar.open(`Generated ${this.recommendations.length} recommendations`, 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Recommendations error:', error);
        this.error = 'Failed to generate recommendations';
        this.generatingRecommendations = false;
        this.snackBar.open('Failed to generate recommendations', 'Close', { duration: 3000 });
      }
    });

    this.subscriptions.add(sub);
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

  getFilteredRecommendations(): MLRecommendation[] {
    let filtered = [...this.recommendations];

    // Filter by priority
    if (this.selectedPriority !== 'all') {
      filtered = filtered.filter(rec => rec.priority.toLowerCase() === this.selectedPriority);
    }

    // Filter by category
    if (this.selectedCategory !== 'all') {
      filtered = filtered.filter(rec => rec.category === this.selectedCategory);
    }

    // Sort recommendations
    filtered.sort((a, b) => {
      switch (this.selectedSortBy) {
        case 'priority':
          return this.getPriorityWeight(b.priority) - this.getPriorityWeight(a.priority);
        case 'impact':
          return this.getImpactWeight(b.impact) - this.getImpactWeight(a.impact);
        case 'effort':
          return this.getEffortWeight(a.effort) - this.getEffortWeight(b.effort);
        case 'confidence':
          return b.confidence - a.confidence;
        case 'generatedAt':
          return new Date(b.generatedAt).getTime() - new Date(a.generatedAt).getTime();
        default:
          return 0;
      }
    });

    return filtered;
  }

  private getPriorityWeight(priority: string): number {
    switch (priority.toLowerCase()) {
      case 'high': return 3;
      case 'medium': return 2;
      case 'low': return 1;
      default: return 0;
    }
  }

  private getImpactWeight(impact: string): number {
    switch (impact.toLowerCase()) {
      case 'high': return 3;
      case 'medium': return 2;
      case 'low': return 1;
      default: return 0;
    }
  }

  private getEffortWeight(effort: string): number {
    switch (effort.toLowerCase()) {
      case 'low': return 3;
      case 'medium': return 2;
      case 'high': return 1;
      default: return 0;
    }
  }

  getPriorityColor(priority: string): string {
    switch (priority.toLowerCase()) {
      case 'high': return 'warn';
      case 'medium': return 'primary';
      case 'low': return 'accent';
      default: return 'primary';
    }
  }

  getImpactColor(impact: string): string {
    switch (impact.toLowerCase()) {
      case 'high': return 'warn';
      case 'medium': return 'primary';
      case 'low': return 'accent';
      default: return 'primary';
    }
  }

  getEffortColor(effort: string): string {
    switch (effort.toLowerCase()) {
      case 'high': return 'warn';
      case 'medium': return 'primary';
      case 'low': return 'accent';
      default: return 'primary';
    }
  }

  getConfidenceColor(confidence: number): string {
    if (confidence >= 0.8) return 'accent';
    if (confidence >= 0.6) return 'primary';
    return 'warn';
  }

  clearResults(): void {
    this.recommendations = [];
    this.error = null;
  }

  refreshModels(): void {
    this.loadAvailableModels();
  }

  exportRecommendations(): void {
    const filteredRecommendations = this.getFilteredRecommendations();
    const csvContent = this.generateCSV(filteredRecommendations);
    this.downloadCSV(csvContent, 'ml-recommendations.csv');
  }

  private generateCSV(recommendations: MLRecommendation[]): string {
    const headers = ['Title', 'Description', 'Category', 'Priority', 'Impact', 'Effort', 'Confidence', 'Generated At'];
    const rows = recommendations.map(rec => [
      rec.title,
      rec.description,
      rec.category,
      rec.priority,
      rec.impact,
      rec.effort,
      `${(rec.confidence * 100).toFixed(1)}%`,
      new Date(rec.generatedAt).toLocaleDateString()
    ]);

    return [headers, ...rows].map(row => row.map(cell => `"${cell}"`).join(',')).join('\n');
  }

  private downloadCSV(content: string, filename: string): void {
    const blob = new Blob([content], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }
} 