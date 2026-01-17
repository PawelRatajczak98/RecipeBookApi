import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CommentDto, CommentCreateDto } from '../models/comment.model';
import { LikeSummaryDto } from '../models/like.model';

@Injectable({
  providedIn: 'root'
})
export class RecipeFeedbackService {
  private readonly apiUrl = 'https://localhost:7090/api';

  constructor(private http: HttpClient) {}

  // ===== COMMENTS =====

  /**
   * Pobiera wszystkie komentarze dla danego przepisu
   */
  getComments(recipeId: number): Observable<CommentDto[]> {
    const params = new HttpParams().set('recipeId', recipeId.toString());
    return this.http.get<CommentDto[]>(`${this.apiUrl}/comments`, { params, withCredentials: true });
  }

  /**
   * Dodaje nowy komentarz do przepisu
   */
  addComment(dto: CommentCreateDto): Observable<CommentDto> {
    return this.http.post<CommentDto>(`${this.apiUrl}/comments`, dto, { withCredentials: true });
  }

  /**
   * Usuwa komentarz użytkownika dla danego przepisu
   */
  deleteComment(recipeId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/comments/${recipeId}`, { withCredentials: true });
  }

  // ===== LIKES =====

  /**
   * Pobiera podsumowanie lajków dla przepisu (liczba + czy obecny użytkownik polubił)
   */
  getLikesSummary(recipeId: number): Observable<LikeSummaryDto> {
    return this.http.get<LikeSummaryDto>(`${this.apiUrl}/likes/${recipeId}`, { withCredentials: true });
  }

  /**
   * Dodaje lajk do przepisu
   */
  addLike(recipeId: number): Observable<string> {
    return this.http.post(`${this.apiUrl}/likes/${recipeId}`, {}, {
      responseType: 'text',
      withCredentials: true
    });
  }

  /**
   * Usuwa lajk z przepisu
   */
  deleteLike(recipeId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/likes/${recipeId}`, { withCredentials: true });
  }
}
