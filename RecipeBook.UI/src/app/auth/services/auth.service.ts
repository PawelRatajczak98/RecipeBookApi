import { HttpClient } from "@angular/common/http";
import { Injectable, computed, inject,signal } from "@angular/core";
import { UserDtoModel } from "../../user/models/user-dto.model";
import { UserLoginDtoModel } from "../../user/models/user-login-dto.model";
import { RegisterRequestDto } from "../models/register-request.model";
import { Observable, tap, catchError, of, throwError, switchMap, finalize, EMPTY } from "rxjs";
import { UserInfoService } from "../../user/services/user-info.service";
import { UserIngredientsService } from "../../user-ingredients/services/useringredients.services";
import { UserIngredientDtoModel } from "../../user-ingredients/models/user-ingredient-dto.model";

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private userInfoService = inject(UserInfoService);
  private userIngredientsService = inject(UserIngredientsService);

  private readonly API = {
    login: 'https://localhost:7091/api/account/login',
    register: 'https://localhost:7091/api/account/register',
    me: 'https://localhost:7091/api/account/login/me',
  };

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly ingredients = signal<string[]>([]);
  readonly user = computed(() => this.userInfoService.currentUser());

  register(userData: RegisterRequestDto) {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    return this.http.post<boolean>(this.API.register, userData, { withCredentials: true }).pipe(
      tap((success: boolean) => {
        if (success) {
          this.successMessage.set('Rejestracja przebiegła pomyślnie! Możesz się teraz zalogować.');
        }
      }),
      catchError((error: unknown) => {
        const message = this.extractErrorMessage(error, 'Błąd rejestracji.');
        this.errorMessage.set(message);
        return throwError(() => error);
      }),
      finalize(() => this.isLoading.set(false))
    );
  }

  loginAndLoad(credentials: UserLoginDtoModel) {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    
    return this.http.post<void>(this.API.login, credentials, { withCredentials: true }).pipe(
      switchMap(() =>
        this.http.get<UserDtoModel>(this.API.me, { withCredentials: true }).pipe(
          tap(user => {
      this.userInfoService.setUser(user);
    })
  )
      ),
      switchMap(() =>
        this.userIngredientsService.getUserIngredients().pipe(
          tap((list: UserIngredientDtoModel[]) =>
            this.ingredients.set(list.map(i => i.ingredientName))
          )
        )
      ),
      catchError((error: unknown) => {
        const message = this.extractErrorMessage(error, 'Błąd logowania lub pobierania danych.');
        this.errorMessage.set(message);
        return EMPTY;
      }),
      finalize(() => this.isLoading.set(false))
    );
  }

  logout() {
    this.userInfoService.clearUser();
    this.ingredients.set([]);
    this.errorMessage.set(null);
    }

  private extractErrorMessage(error: any, fallback: string): string {
    return error?.error?.message || fallback;
  }
}
