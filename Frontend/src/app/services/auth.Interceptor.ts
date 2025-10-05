import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const now = Math.floor(Date.now() / 1000);
    return payload.exp < now;
  } catch {
    return true; // token bozuksa expired kabul et
  }
}

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
   const token = localStorage.getItem('token');
  const router = inject(Router);

  let authReq = req;

  if (token && !isTokenExpired(token)) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(authReq).pipe(
    catchError((error) => {
      if (error.status === 401) {
        localStorage.removeItem('token');
        router.navigate(['/login']); // veya window.location.href = '/login';
      }
      return throwError(() => error);
    })
  );
};