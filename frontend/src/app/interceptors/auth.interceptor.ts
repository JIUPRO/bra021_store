import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  
  return next(req).pipe(
    catchError((error) => {
      // Se receber 401 (Não autorizado), limpar sessão e redirecionar para login
      if (error.status === 401) {
        sessionStorage.removeItem('clienteLogado');
        sessionStorage.removeItem('tokenExpiracao');
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
