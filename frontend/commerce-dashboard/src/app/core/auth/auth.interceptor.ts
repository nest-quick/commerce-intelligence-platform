import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (
  request,
  next
) => {
  const authService = inject(AuthService);

  const accessToken = authService.getAccessToken();

  const isAuthRequest =
    request.url.includes('/api/auth/');

  let requestToSend = request;

  if (accessToken != null && !isAuthRequest) 
    {
        requestToSend = request.clone({
        setHeaders: {Authorization: `Bearer ${accessToken}`}
        });
    }

  return next(requestToSend).pipe(
    catchError((error: HttpErrorResponse) => {
      if (
        error.status !== 401 ||
        isAuthRequest
      ) {
        return throwError(() => error);
      }

      return authService.refresh().pipe(
        switchMap(() => {
          const newAccessToken =
            authService.getAccessToken();

          if (newAccessToken == null) {
            authService.clearSession();

            return throwError(() => error);
          }

          const retriedRequest = request.clone({
            setHeaders: {
              Authorization:
                `Bearer ${newAccessToken}`
            }
          });

          return next(retriedRequest);
        }),

        catchError(refreshError => {
          authService.clearSession();

          return throwError(() => refreshError);
        })
      );
    })
  );
};