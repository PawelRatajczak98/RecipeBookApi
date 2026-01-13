import { Injectable, inject, signal, computed } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { UserIngredientDtoModel } from "../models/user-ingredient-dto.model";
import { userIngredientCreateDto } from "../models/user-ingredient-dto-create.model";
import { UserIngredientUpdateDto } from "../models/user-ingredient-dto-update.model";

@Injectable({ providedIn: 'root' })
export class UserIngredientsService {
  private baseUrl = 'https://localhost:7090/api/useringredients';
  private http = inject(HttpClient);

  // sygnał z listą składników użytkownika
  userIngredients = signal<UserIngredientDtoModel[]>([]);

  // computed sumujący budżet
  getCurrentBudget = computed(() => {
    return this.userIngredients().reduce((sum, ing) => sum + (ing.totalPrice ?? 0), 0);
  });

  // ustawia listę składników (np. po pobraniu z backendu)
  setUserIngredients(list: UserIngredientDtoModel[]) {
    this.userIngredients.set(list);
  }

  // pobranie listy składników z backendu
  getUserIngredients(): Observable<UserIngredientDtoModel[]> {
    return this.http.get<UserIngredientDtoModel[]>(`${this.baseUrl}`, { withCredentials: true });
  }

  // odświeżenie listy składników – wywołanie przy logowaniu
  refreshUserIngredients() {
    this.getUserIngredients().subscribe({
      next: list => this.userIngredients.set(list),
      error: () => console.error('Nie udało się pobrać składników użytkownika')
    });
  }

  addUserIngredient(dto: userIngredientCreateDto): Observable<boolean> {
    return this.http.post<any>(`${this.baseUrl}`, dto, { withCredentials: true })
      .pipe(map(() => true));
  }

  deleteUserIngredient(ingredientId: number): Observable<boolean> {
    return this.http.delete<any>(`${this.baseUrl}/${ingredientId}`, { withCredentials: true })
      .pipe(map(() => true));
  }

  updateUserIngredient(ingredientId: number, quantity: number): Observable<boolean> {
    const dto: UserIngredientUpdateDto = {
      IngredientId: ingredientId,
      Quantity: quantity
    };
    return this.http.patch<any>(`${this.baseUrl}/${ingredientId}`, dto, { withCredentials: true })
      .pipe(map(() => true));
  }
}
