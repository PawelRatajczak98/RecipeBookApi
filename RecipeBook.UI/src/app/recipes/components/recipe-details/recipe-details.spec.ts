import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RecipeDetails } from './recipe-details';

describe('RecipesList', () => {
  let component: RecipeDetails;
  let fixture: ComponentFixture<RecipeDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecipeDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RecipeDetails);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
