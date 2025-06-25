import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdvancedAnalyticsService, CustomReportData } from '../../services/advanced-analytics.service';

@Component({
  selector: 'app-custom-report-builder',
  templateUrl: './custom-report-builder.component.html',
  styleUrls: ['./custom-report-builder.component.scss']
})
export class CustomReportBuilderComponent implements OnInit {
  
  reportForm: FormGroup;
  reportData: CustomReportData;
  loading = false;
  error: string;

  reportTypes = [
    { value: 'compliance', label: 'Compliance Report' },
    { value: 'risk', label: 'Risk Assessment Report' },
    { value: 'trend', label: 'Trend Analysis Report' },
    { value: 'benchmark', label: 'Benchmarking Report' },
    { value: 'comprehensive', label: 'Comprehensive Report' }
  ];

  constructor(
    private analyticsService: AdvancedAnalyticsService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initForm();
  }

  initForm(): void {
    this.reportForm = this.fb.group({
      reportType: ['comprehensive', Validators.required],
      timeframe: [365],
      includeRecommendations: [true],
      sectorId: [null],
      industryId: [null]
    });
  }

  generateReport(): void {
    if (this.reportForm.valid) {
      this.loading = true;
      this.error = null;

      const formValue = this.reportForm.value;
      const parameters: { [key: string]: any } = {};

      if (formValue.timeframe) parameters.timeframe = formValue.timeframe;
      if (formValue.includeRecommendations) parameters.includeRecommendations = formValue.includeRecommendations;
      if (formValue.sectorId) parameters.sectorId = formValue.sectorId;
      if (formValue.industryId) parameters.industryId = formValue.industryId;

      this.analyticsService.getCustomReport(formValue.reportType, parameters).subscribe({
        next: (data) => {
          this.reportData = data;
          this.loading = false;
        },
        error: (error) => {
          this.error = 'Failed to generate custom report';
          this.loading = false;
          console.error('Custom report error:', error);
        }
      });
    }
  }
} 