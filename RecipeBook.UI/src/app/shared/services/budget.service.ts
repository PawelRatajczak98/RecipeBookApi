import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BudgetService {
  private readonly apiUrl = 'https://localhost:7090/api/budget';

  constructor(private http: HttpClient) {}

  /**
   * Pobiera budżet użytkownika
   */
  getBudget(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/budget`, { withCredentials: true });
  }

  /**
   * Ustawia nowy budżet użytkownika
   */
  setBudget(amount: number): Observable<number> {
    return this.http.put<number>(`${this.apiUrl}/setBudget`, amount, { withCredentials: true });
  }

  /**
   * Zwiększa budżet o podaną kwotę
   */
  increaseBudget(amount: number): Observable<number> {
    return this.http.post<number>(`${this.apiUrl}/increase`, amount, { withCredentials: true });
  }

  /**
   * Zmniejsza budżet o podaną kwotę
   */
  decreaseBudget(amount: number): Observable<number> {
    return this.http.post<number>(`${this.apiUrl}/decrease`, amount, { withCredentials: true });
  }
}
