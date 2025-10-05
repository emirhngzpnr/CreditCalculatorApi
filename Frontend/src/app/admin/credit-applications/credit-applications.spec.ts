import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditApplications } from './credit-applications';

describe('CreditApplications', () => {
  let component: CreditApplications;
  let fixture: ComponentFixture<CreditApplications>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreditApplications]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreditApplications);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
