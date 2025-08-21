import { RecipeIngredientDto } from "../../recipeingredients/models/recipeingredient-dto.model";

export interface RecipeCreateDtoModel {
  name: string;
  description: string;
  recipeIngredientsDto: RecipeIngredientDto[];
  instructions: string;
  preparationTime: string;
  cookingTime: string;
  servings: number;
}