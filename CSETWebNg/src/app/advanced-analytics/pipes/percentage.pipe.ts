import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'percentage'
})
export class PercentagePipe implements PipeTransform {

  transform(value: number, decimals: number = 1): string {
    if (value === null || value === undefined) {
      return '0%';
    }
    return `${value.toFixed(decimals)}%`;
  }
} 