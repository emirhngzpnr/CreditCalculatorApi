import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const adminGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('token');

  if (token) {
    const payload = JSON.parse(atob(token.split('.')[1]));

    // .NET'in verdiği namespaced role claim'i:
    const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

    if (role?.toLowerCase() === 'admin') {
      console.log("Admin yetkisi var, geçişe izin veriliyor.");
      return true;
    } else {
      console.warn("Admin değil, erişim reddedildi.");
      return router.navigate(['/kredi-hesapla']).then(() => false);
    }
  } else {
    console.warn("Token yok, login sayfasına yönlendiriliyor.");
    return router.navigate(['/login']).then(() => false);
  }
};

