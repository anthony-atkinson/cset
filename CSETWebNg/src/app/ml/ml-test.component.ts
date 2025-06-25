import { Component, OnInit } from '@angular/core';
import { MLService } from '../services/ml.service';

@Component({
  selector: 'app-ml-test',
  template: `
    <div>
      <h2>ML Service Test</h2>
      <button (click)="testService()">Test ML Service</button>
      <div *ngIf="testResult">
        <p>Test Result: {{ testResult }}</p>
      </div>
    </div>
  `
})
export class MLTestComponent implements OnInit {
  testResult: string = '';

  constructor(private mlService: MLService) { }

  ngOnInit(): void {
  }

  testService(): void {
    this.mlService.getAvailableAlgorithms().subscribe({
      next: (algorithms) => {
        this.testResult = `Success! Found ${algorithms.length} algorithms: ${algorithms.join(', ')}`;
      },
      error: (error) => {
        this.testResult = `Error: ${error.message}`;
      }
    });
  }
} 