import { Observable } from "rxjs";
import { Ingredient } from "../models/ingredient.model";
import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";

@Injectable({ providedIn: 'root' })
export class IngredientService {
  private http = inject(HttpClient);
  private readonly API = 'https://localhost:7090/api/ingredient';

  getAllIngredients(): Observable<Ingredient[]> {
    return this.http.get<Ingredient[]>(this.API, { withCredentials: true });
  }

  getIngredientById(id: number): Observable<Ingredient> {
    return this.http.get<Ingredient>(`${this.API}/${id}`, { withCredentials: true });
  }

  createIngredient(ingredient: Ingredient): Observable<boolean> {
    return this.http.post<boolean>(this.API, ingredient, { withCredentials: true });
  }

  updateIngredient(id: number, ingredient: Ingredient): Observable<boolean> {
    return this.http.put<boolean>(`${this.API}/${id}`, ingredient, { withCredentials: true });
  }

  deleteIngredient(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.API}/${id}`, { withCredentials: true });
  }
}