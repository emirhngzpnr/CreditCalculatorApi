import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditCalculateComponent } from './credit-calculate';

describe('CreditCalculate', () => {
  let component: CreditCalculateComponent;
  let fixture: ComponentFixture<CreditCalculateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreditCalculateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreditCalculateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
