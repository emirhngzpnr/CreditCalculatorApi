import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KrediBasvuru } from './kredi-basvuru';

describe('KrediBasvuru', () => {
  let component: KrediBasvuru;
  let fixture: ComponentFixture<KrediBasvuru>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KrediBasvuru]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KrediBasvuru);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
