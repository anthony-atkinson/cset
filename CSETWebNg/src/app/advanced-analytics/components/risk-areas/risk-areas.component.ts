import { Component, Input } from '@angular/core';
import { RiskArea } from '../../services/advanced-analytics.service';

@Component({
  selector: 'app-risk-areas',
  templateUrl: './risk-areas.component.html',
  styleUrls: ['./risk-areas.component.scss']
})
export class RiskAreasComponent {
  
  @Input() riskAreas: RiskArea[] = [];

  getRiskLevel(score: number): string {
    if (score >= 70) return 'High';
    if (score >= 40) return 'Medium';
    return 'Low';
  }

  getRiskColor(score: number): string {
    if (score >= 70) return 'red';
    if (score >= 40) return 'orange';
    return 'green';
  }
} 