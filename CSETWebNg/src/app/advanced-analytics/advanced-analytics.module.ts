import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSliderModule } from '@angular/material/slider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';

// Components
import { AdvancedAnalyticsComponent } from './advanced-analytics.component';
import { ExecutiveSummaryComponent } from './components/executive-summary/executive-summary.component';
import { TrendAnalysisComponent } from './components/trend-analysis/trend-analysis.component';
import { BenchmarkingComponent } from './components/benchmarking/benchmarking.component';
import { PredictiveAnalyticsComponent } from './components/predictive-analytics/predictive-analytics.component';
import { CustomReportBuilderComponent } from './components/custom-report-builder/custom-report-builder.component';
import { AnalyticsDashboardComponent } from './components/analytics-dashboard/analytics-dashboard.component';
import { RiskAreasComponent } from './components/risk-areas/risk-areas.component';
import { ImprovementRecommendationsComponent } from './components/improvement-recommendations/improvement-recommendations.component';

// Services
import { AdvancedAnalyticsService } from './services/advanced-analytics.service';

// Directives
import { ChartDirective } from './directives/chart.directive';

// Pipes
import { PercentagePipe } from './pipes/percentage.pipe';
import { RiskLevelPipe } from './pipes/risk-level.pipe';

@NgModule({
  declarations: [
    AdvancedAnalyticsComponent,
    ExecutiveSummaryComponent,
    TrendAnalysisComponent,
    BenchmarkingComponent,
    PredictiveAnalyticsComponent,
    CustomReportBuilderComponent,
    AnalyticsDashboardComponent,
    RiskAreasComponent,
    ImprovementRecommendationsComponent,
    ChartDirective,
    PercentagePipe,
    RiskLevelPipe
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatTabsModule,
    MatProgressBarModule,
    MatChipsModule,
    MatIconModule,
    MatTooltipModule,
    MatExpansionModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSliderModule,
    MatCheckboxModule,
    MatRadioModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  providers: [
    AdvancedAnalyticsService
  ],
  exports: [
    AdvancedAnalyticsComponent,
    ExecutiveSummaryComponent,
    TrendAnalysisComponent,
    BenchmarkingComponent,
    PredictiveAnalyticsComponent,
    CustomReportBuilderComponent,
    AnalyticsDashboardComponent
  ]
})
export class AdvancedAnalyticsModule { } 