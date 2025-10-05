import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BankApplications } from './bank-applications';

describe('BankApplications', () => {
  let component: BankApplications;
  let fixture: ComponentFixture<BankApplications>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BankApplications]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BankApplications);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
