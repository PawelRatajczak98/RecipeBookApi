import { inject, Injectable } from "@angular/core";
import { PagedResult } from "../../paged-result/models/paged-result.model";
import { RecipeDto } from "../models/recipe-dto.model";
import { HttpClient, HttpParams } from "@angular/common/http";
import { RecipeQuery } from "../query/recipe-query.model";
import { Observable } from "rxjs";
import { RecipeCreateDtoModel } from "../models/recipe-create-dto.model";

@Injectable({ providedIn: 'root' })
export class RecipeService {
  private apiUrl = 'https://localhost:7090/api/recipes';
  private http = inject(HttpClient);

  getRecipes(query: RecipeQuery): Observable<PagedResult<RecipeDto>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.searchPhrase) {
      params = params.set('searchPhrase', query.searchPhrase);
    }

    return this.http.get<PagedResult<RecipeDto>>(this.apiUrl, { params, withCredentials: true });
  }

  getRecipesWithinBudget(budget: number, query: RecipeQuery): Observable<PagedResult<RecipeDto>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber.toString())
      .set('pageSize', query.pageSize.toString())
      .set('budget', budget.toString());

    if (query.searchPhrase) {
      params = params.set('searchPhrase', query.searchPhrase);
    }
    return this.http.get<PagedResult<RecipeDto>>(this.apiUrl + '/within-budget', {
      params,
      withCredentials: true
    });
  }

  getRecipesUserCanPrepare(userId: string, query: RecipeQuery): Observable<PagedResult<RecipeDto>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber.toString())
      .set('pageSize', query.pageSize.toString())
      .set('userId', userId);

    if (query.searchPhrase) {
      params = params.set('searchPhrase', query.searchPhrase);
    }

    return this.http.get<PagedResult<RecipeDto>>(`${this.apiUrl}/can-prepare`, {
      params,
      withCredentials: true
    });
  }

  getRecipeById(id: number): Observable<RecipeDto> {
    return this.http.get<RecipeDto>(`${this.apiUrl}/${id}`, { withCredentials: true });
  }

  createRecipe(recipe: RecipeCreateDtoModel, imageFile?: File): Observable<boolean> {
    const formData = new FormData();

    formData.append('name', recipe.name);
    formData.append('description', recipe.description);
    formData.append('instructions', recipe.instructions);
    formData.append('preparationTime', recipe.preparationTime);
    formData.append('cookingTime', recipe.cookingTime);
    formData.append('servings', recipe.servings.toString());

    recipe.recipeIngredientsDto.forEach((ingredient, index) => {
      formData.append(`recipeIngredientsDto[${index}].ingredientId`, ingredient.ingredientId.toString());
      formData.append(`recipeIngredientsDto[${index}].quantity`, ingredient.quantity.toString());
      formData.append(`recipeIngredientsDto[${index}].unit`, ingredient.unit);
      formData.append(`recipeIngredientsDto[${index}].ingredientName`, ingredient.ingredientName);
    });

    if (imageFile) {
      formData.append('image', imageFile, imageFile.name);
    }

    return this.http.post<boolean>(this.apiUrl, formData, { withCredentials: true });
  }

  getCheapestRecipeCost(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/cheapest`, { withCredentials: true });
}
}
