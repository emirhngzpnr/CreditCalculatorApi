import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LoginRequest } from '../../../models/login-request';
import { AuthService } from '../../../services/auth.service';
import { Router, RouterModule } from '@angular/router';
import Swal from 'sweetalert2'; 

@Component({
  selector: 'app-login',
  standalone:true,
  imports: [CommonModule,ReactiveFormsModule,RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
 loginForm: FormGroup;
  errorMessage = '';
  submitted = false;
  showPassword: boolean = false;


  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
  this.submitted = true;
  this.errorMessage = '';

  if (this.loginForm.invalid) return;

  const loginData: LoginRequest = this.loginForm.value;

  this.authService.login(loginData).subscribe({
    next: (res) => {
      this.authService.setToken(res.token);

      if (this.authService.isLoggedIn()) {
        Swal.fire({
          icon: 'success',
          title: 'Giriş Başarılı!',
          text: 'Profil sayfasına yönlendiriliyorsunuz...',
          timer: 2000,
          showConfirmButton: false
        });
        setTimeout(() => {
          this.router.navigate(['/profil']);
        }, 2000);
      } else {
        this.authService.logout();
        Swal.fire({
          icon: 'error',
          title: 'Geçersiz Token!',
          text: 'Oturum geçersiz. Lütfen tekrar giriş yapın.'
        });
      }
    },
    error: (err) => {
      const msg = typeof err.error === 'string' ? err.error : (err.error?.message || 'Giriş başarısız!');

      if (msg.includes('doğrulanmamış')) {
        Swal.fire({
          icon: 'warning',
          title: 'E-posta Doğrulanmamış',
          text: 'Lütfen e-posta adresinizi doğrulayın ve tekrar deneyin.'
        });
      } else if (msg.includes('bulunamadı')) {
        Swal.fire({
          icon: 'error',
          title: 'Kullanıcı Bulunamadı',
          text: 'Bu e-posta ile kayıtlı kullanıcı yok.'
        });
      } else if (msg.includes('Şifre hatalı')) {
        Swal.fire({
          icon: 'error',
          title: 'Şifre Hatalı',
          text: 'Lütfen şifrenizi kontrol edin ve tekrar deneyin.'
        });
      } else {
        Swal.fire({
          icon: 'error',
          title: 'Giriş Başarısız',
          text: msg
        });
      }
    }
  });
}
}

