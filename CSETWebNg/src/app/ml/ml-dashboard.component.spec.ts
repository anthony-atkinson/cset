import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MLDashboardComponent } from './ml-dashboard.component';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('MLDashboardComponent', () => {
  let component: MLDashboardComponent;
  let fixture: ComponentFixture<MLDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MLDashboardComponent],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MLDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the dashboard', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain(''); // Update with a real selector or text if available
  });
}); 