import { Directive, ElementRef, Input, OnChanges, SimpleChanges } from '@angular/core';

@Directive({
  selector: '[appChart]'
})
export class ChartDirective implements OnChanges {
  
  @Input() chartData: any;
  @Input() chartType: string = 'line';
  @Input() chartOptions: any = {};

  private chart: any;

  constructor(private el: ElementRef) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['chartData'] || changes['chartType'] || changes['chartOptions']) {
      this.updateChart();
    }
  }

  private updateChart(): void {
    // Chart.js implementation will be added here
    // For now, this is a placeholder
    console.log('Chart directive - data:', this.chartData, 'type:', this.chartType);
  }
} 