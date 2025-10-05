import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MusteriOl } from './musteri-ol';

describe('MusteriOl', () => {
  let component: MusteriOl;
  let fixture: ComponentFixture<MusteriOl>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MusteriOl]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MusteriOl);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
