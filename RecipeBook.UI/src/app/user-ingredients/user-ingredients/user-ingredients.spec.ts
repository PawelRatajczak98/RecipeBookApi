import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserIngredients } from './user-ingredients';

describe('UserIngredients', () => {
  let component: UserIngredients;
  let fixture: ComponentFixture<UserIngredients>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserIngredients]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserIngredients);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
