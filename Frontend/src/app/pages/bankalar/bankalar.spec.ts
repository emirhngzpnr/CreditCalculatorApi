import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Bankalar } from './bankalar';

describe('Bankalar', () => {
  let component: Bankalar;
  let fixture: ComponentFixture<Bankalar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Bankalar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Bankalar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
