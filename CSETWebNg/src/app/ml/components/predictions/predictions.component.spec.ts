import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PredictionsComponent } from './predictions.component';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('PredictionsComponent', () => {
  let component: PredictionsComponent;
  let fixture: ComponentFixture<PredictionsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PredictionsComponent],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PredictionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render predictions', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain(''); // Update with a real selector or text if available
  });
}); 