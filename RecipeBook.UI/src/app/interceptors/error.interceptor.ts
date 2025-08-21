import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'Wystąpił nieznany błąd';

      console.log('HTTP Error caught by interceptor:', error);

      if (error.status === 400 && error.error) {
        // ValidationException z backendu
        if (typeof error.error === 'string') {
          errorMessage = error.error;
        } else if (error.error.message) {
          errorMessage = error.error.message;
        } else if (Array.isArray(error.error) && error.error.length > 0) {
          // FluentValidation errors
          errorMessage = error.error[0]?.errorMessage || error.error[0];
        } else if (error.error.title) {
          // ASP.NET Core ValidationProblem
          errorMessage = error.error.title;
        }
      } else if (error.status === 401) {
        errorMessage = 'Brak autoryzacji. Zaloguj się ponownie.';
      } else if (error.status === 403) {
        errorMessage = 'Brak uprawnień do wykonania tej operacji.';
      } else if (error.status === 404) {
        errorMessage = 'Nie znaleziono zasobu.';
      } else if (error.status === 409) {
        errorMessage = 'Konflikt danych. Zasób już istnieje.';
      } else if (error.status >= 500) {
        errorMessage = 'Błąd serwera. Spróbuj ponownie później.';
      } else if (error.status === 0) {
        errorMessage = 'Brak połączenia z serwerem.';
      }

      const enhancedError = {
        ...error,
        userMessage: errorMessage
      };

      console.log('Enhanced error with user message:', enhancedError);

      return throwError(() => enhancedError);
    })
  );
};