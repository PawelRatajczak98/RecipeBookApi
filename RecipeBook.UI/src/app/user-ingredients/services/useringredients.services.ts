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
  private budgetUrl = 'https://localhost:7090/api/budget';
  private http = inject(HttpClient);

  // sygnał z listą składników użytkownika
  userIngredients = signal<UserIngredientDtoModel[]>([]);

  // sygnał z budżetem użytkownika
  private budget = signal<number>(0);

  // zwraca aktualny budżet
  getCurrentBudget = computed(() => {
    return this.budget();
  });

  // ustawia listę składników (np. po pobraniu z backendu)
  setUserIngredients(list: UserIngredientDtoModel[]) {
    this.userIngredients.set(list);
  }

  // pobranie listy składników z backendu
  getUserIngredients(): Observable<UserIngredientDtoModel[]> {
    return this.http.get<UserIngredientDtoModel[]>(`${this.baseUrl}`, { withCredentials: true });
  }

  // pobranie budżetu z backendu
  getBudget(): Observable<number> {
    return this.http.get<number>(`${this.budgetUrl}/budget`, { withCredentials: true });
  }

  // odświeżenie listy składników i budżetu – wywołanie przy logowaniu
  refreshUserIngredients() {
    this.getUserIngredients().subscribe({
      next: list => this.userIngredients.set(list),
      error: () => console.error('Nie udało się pobrać składników użytkownika')
    });

    this.getBudget().subscribe({
      next: budget => this.budget.set(budget),
      error: () => console.error('Nie udało się pobrać budżetu użytkownika')
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
