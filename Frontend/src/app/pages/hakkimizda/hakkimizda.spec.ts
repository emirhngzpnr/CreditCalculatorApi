import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Hakkimizda } from './hakkimizda';

describe('Hakkimizda', () => {
  let component: Hakkimizda;
  let fixture: ComponentFixture<Hakkimizda>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Hakkimizda]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Hakkimizda);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
