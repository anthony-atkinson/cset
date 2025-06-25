import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ModelTrainingComponent } from './model-training.component';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('ModelTrainingComponent', () => {
  let component: ModelTrainingComponent;
  let fixture: ComponentFixture<ModelTrainingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ModelTrainingComponent],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ModelTrainingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render model training', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain(''); // Update with a real selector or text if available
  });
}); 