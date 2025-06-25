import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'riskLevel'
})
export class RiskLevelPipe implements PipeTransform {

  transform(value: number): string {
    if (value >= 70) return 'High';
    if (value >= 40) return 'Medium';
    return 'Low';
  }
} 