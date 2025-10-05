import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyCredits } from './my-credits';

describe('MyCredits', () => {
  let component: MyCredits;
  let fixture: ComponentFixture<MyCredits>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyCredits]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyCredits);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
