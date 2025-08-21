import { FormControl, FormGroup, FormArray } from '@angular/forms';

export interface RecipeIngredientForm {
  ingredientName: FormControl<string>;
  ingredientId: FormControl<number>;
  quantity: FormControl<number>;
  unit: FormControl<string>;
}

export interface RecipeCreateForm {
  name: FormControl<string>;
  description: FormControl<string>;
  instructions: FormControl<string>;
  preparationTime: FormControl<number>;
  cookingTime: FormControl<number>;
  servings: FormControl<number>;
  recipeIngredientsDto: FormArray<FormGroup<RecipeIngredientForm>>;
}
