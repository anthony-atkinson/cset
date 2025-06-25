import { Component, Input } from '@angular/core';
import { ImprovementRecommendation } from '../../services/advanced-analytics.service';

@Component({
  selector: 'app-improvement-recommendations',
  templateUrl: './improvement-recommendations.component.html',
  styleUrls: ['./improvement-recommendations.component.scss']
})
export class ImprovementRecommendationsComponent {
  
  @Input() recommendations: ImprovementRecommendation[] = [];

  getPriorityColor(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'high': return 'red';
      case 'medium': return 'orange';
      case 'low': return 'green';
      default: return 'gray';
    }
  }

  getImpactColor(impact: string): string {
    switch (impact?.toLowerCase()) {
      case 'significant': return 'red';
      case 'high': return 'orange';
      case 'medium': return 'yellow';
      case 'low': return 'green';
      default: return 'gray';
    }
  }
} 