import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone:true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPassword {
form: FormGroup;
  successMessage = '';
  errorMessage = '';
  submitted = false;

  constructor(private fb: FormBuilder, private authService:AuthService, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  get f() {
    return this.form.controls;
  }

   onSubmit() {
    this.submitted = true;

    if (this.form.invalid) return;

    const email = this.form.value.email;

    this.authService.forgotPassword(email).subscribe({
     next: () => {
  Swal.fire({
    icon: 'success',
    title: 'Şifre sıfırlama bağlantısı gönderildi!',
    text: 'E-posta adresinize gönderilen bağlantı ile şifrenizi sıfırlayabilirsiniz.',
    confirmButtonColor: '#3085d6'
  }).then(() => {
    this.router.navigate(['/login']); //  yönlendirme burada
  });
}
,
    error: (err) => {
  const message = err.status === 404
    ? err.error?.message || 'Kullanıcı bulunamadı.'
    : 'E-posta gönderilirken bir hata oluştu.';

        Swal.fire({
          icon: 'error',
          title: 'Hata!',
          text:message,
         
          confirmButtonColor: '#d33'
        });
      }
    });
  }
}

