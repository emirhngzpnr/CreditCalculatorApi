import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminCustomerapplication } from './admin-customerapplication';

describe('AdminCustomerapplication', () => {
  let component: AdminCustomerapplication;
  let fixture: ComponentFixture<AdminCustomerapplication>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminCustomerapplication]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminCustomerapplication);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
