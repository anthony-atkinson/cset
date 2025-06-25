import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MLTestComponent } from './ml-test.component';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('MLTestComponent', () => {
  let component: MLTestComponent;
  let fixture: ComponentFixture<MLTestComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MLTestComponent],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MLTestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the test component', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain(''); // Update with a real selector or text if available
  });
}); 