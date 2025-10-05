import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KampanyaDetay } from './kampanya-detay';

describe('KampanyaDetay', () => {
  let component: KampanyaDetay;
  let fixture: ComponentFixture<KampanyaDetay>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KampanyaDetay]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KampanyaDetay);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
