import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ApiService } from '../../../services/api.service';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import Swal from 'sweetalert2';


@Component({
  selector: 'app-register',
  standalone:true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
registerForm: FormGroup;
submitted=false;

  constructor(private fb: FormBuilder, private api: AuthService, private router: Router) {
    this.registerForm = this.fb.group({
  firstName: ['', [Validators.required, Validators.maxLength(50)]],
  lastName: ['', [Validators.required, Validators.maxLength(50)]],
  email: ['', [Validators.required, Validators.email]],
  phoneNumber: ['', [
    Validators.required,
    Validators.pattern(/^5\d{9}$/) // 5 ile başlar ve 10 hane
  ]],
  birthDate: ['', [Validators.required,beAtLeast18,beYoungerThan75]],
  identityNumber: ['', [
    Validators.required,  
    Validators.minLength(11),
    Validators.maxLength(11)
  ]],
  password: ['', [
    Validators.required,
    Validators.minLength(6),
    Validators.pattern(/^(?=.*[A-Z])(?=.*[a-z])(?=.*[\+\-%&]).*$/)
  ]]
});

  }

  onSubmit() {
  this.submitted = true;

  if (this.registerForm.invalid) {
    Swal.fire({
      icon: 'error',
      title: 'Form Hatalı',
      text: 'Lütfen tüm alanları doğru şekilde doldurunuz.',
      confirmButtonText: 'Tamam'
    });
    return;
  }

  this.api.register(this.registerForm.value).subscribe({
    next: () => {
      Swal.fire({
        icon: 'success',
        title: 'Kayıt Başarılı!',
        text: 'Giriş sayfasına yönlendiriliyorsunuz...',
        showConfirmButton: false,
        timer: 2000
      });
      setTimeout(() => {
        this.router.navigate(['/login']);
      }, 2000);
    },
    error: (err) => {
  const defaultMsg = typeof err.error === 'string' ? err.error : (err.error?.message || 'Kayıt sırasında bir hata oluştu.');

  // Aynı email hatasını özel olarak kontrol et
  if (defaultMsg.includes('zaten kayıtlı')) {
    Swal.fire({
      icon: 'warning',
      title: 'E-posta Zaten Kullanılıyor',
      text: 'Bu e-posta adresi ile daha önce kayıt olunmuş.',
      confirmButtonText: 'Tamam'
    });
    return;
  }

  // Diğer tüm validasyon veya sistem hataları
  const backendErrors = err?.error?.errors;
  if (backendErrors) {
    const errorMessages = Object.values(backendErrors)
      .flat()
      .map(msg => `<li>${msg}</li>`)
      .join('');

    Swal.fire({
      icon: 'error',
      title: 'Kayıt Başarısız',
      html: `<ul style="text-align:left;">${errorMessages}</ul>`,
      confirmButtonText: 'Tamam'
    });
  } else {
    Swal.fire({
      icon: 'error',
      title: 'Kayıt Başarısız',
      text: defaultMsg,
      confirmButtonText: 'Tamam'
    });
  }
}

  });
}

}
function beAtLeast18(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) return null;

  const today = new Date();
  const birthDate = new Date(value);
  const age = today.getFullYear() - birthDate.getFullYear();
  const month = today.getMonth() - birthDate.getMonth();
  const day = today.getDate() - birthDate.getDate();

  const is18OrOlder = age > 18 || (age === 18 && (month > 0 || (month === 0 && day >= 0)));

  return is18OrOlder ? null : { underage: true };
}
function beYoungerThan75(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) return null;

  const today = new Date();
  const birthDate = new Date(value);
  const age = today.getFullYear() - birthDate.getFullYear();
  const month = today.getMonth() - birthDate.getMonth();
  const day = today.getDate() - birthDate.getDate();

  const is75OrYounger = age < 75 || (age === 75 && (month < 0 || (month === 0 && day <= 0)));

  return is75OrYounger ? null : { overage: true };
}
