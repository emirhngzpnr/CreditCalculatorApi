import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('token');

  if (token) {
    console.log("Giriş yapmış, geçişe izin veriliyor.");
    return true;
  } else {
    console.log("Giriş yok, yönlendiriliyor.");
    // önemli! router.navigate async çalışır, Promise dönmelisin
    return router.navigate(['/login']).then(() => false);
  }
};
