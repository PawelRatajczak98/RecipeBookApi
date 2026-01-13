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

  createRecipe(recipe: RecipeCreateDtoModel): Observable<boolean> {
    return this.http.post<boolean>(this.apiUrl, recipe, { withCredentials: true });
  }

  getCheapestRecipeCost(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/cheapest`, { withCredentials: true });
}
}
