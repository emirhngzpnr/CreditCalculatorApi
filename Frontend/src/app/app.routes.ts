import { Routes } from '@angular/router';
import { CreditCalculateComponent } from './pages/credit-calculate/credit-calculate';
import { KrediBasvuru } from './pages/kredi-basvuru/kredi-basvuru';
import { Bankalar } from './pages/bankalar/bankalar';
import { MusteriOl } from './pages/musteri-ol/musteri-ol';
import { Hakkimizda } from './pages/hakkimizda/hakkimizda';
import { KampanyaDetay } from './pages/kampanya-detay/kampanya-detay';

import { Banks } from './admin/banks/banks';
import { Campaigns } from './admin/campaigns/campaigns';
import { AdminLayout } from './layouts/admin-layout/admin-layout';
import { DefaultLayout } from './layouts/default-layout/default-layout';
import { CreditApplications } from './admin/credit-applications/credit-applications';
import { AdminCustomerapplication } from './admin/admin-customerapplication/admin-customerapplication';
import { Register } from './pages/auth/register/register';
import { Login } from './pages/auth/login/login';
import { authGuard } from './guards/auth-guard';
import { ForgotPassword } from './pages/auth/forgot-password/forgot-password';
import { ResetPassword } from './pages/auth/reset-password/reset-password';
import { Profile } from './pages/profile/profile';
import { ConfirmEmail } from './pages/confirm-email/confirm-email';
import { adminGuard } from './guards/admin-guard';
import { Logs } from './admin/logs/logs/logs';

export const routes: Routes = [
  {
    path: '',
    component: DefaultLayout,
    children: [
      { path: '', redirectTo: 'kredi-hesapla', pathMatch: 'full' },
      { path: 'kredi-hesapla', component: CreditCalculateComponent },
      {
        path: 'kredi-basvuru',
        component: KrediBasvuru,
        canActivate: [authGuard],
      },
      { path: 'bankalar', component: Bankalar },
      { path: 'musteri-ol', component: MusteriOl, canActivate: [authGuard] },
      { path: 'hakkimizda', component: Hakkimizda },
      { path: 'kampanya-detay/:id', component: KampanyaDetay },
      { path: 'register', component: Register },
      { path: 'login', component: Login },
      { path: 'forgot-password', component: ForgotPassword },
      { path: 'reset-password', component: ResetPassword },
        { path: 'profil', component: Profile },
        {path:'confirm-email',component:ConfirmEmail}
      
    ],
  },

  // Admin layout
  {
    path: 'admin',
    component: AdminLayout,
    canActivate:[adminGuard],
    children: [
      { path: 'bankalar', component: Banks },
      { path: 'kampanyalar', component: Campaigns },
      { path: 'kredi-basvuru-listesi', component: CreditApplications },
      {
        path: 'admin-customerapplication',
        component: AdminCustomerapplication,
      },
       { path: 'logs', component: Logs }, 
      
    ],
  },
];
