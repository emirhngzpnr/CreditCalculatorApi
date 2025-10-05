import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-reset-password',
  standalone:true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css'
})
export class ResetPassword {
 form!: FormGroup;
  token: string = '';
  submitted = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // URL'den token parametresini al
    this.token = this.route.snapshot.queryParamMap.get('token') || '';

    this.form = this.fb.group({
      newPassword: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$/)
      ]]
    });
  }

  submit(): void {
    this.submitted = true;
    if (this.form.invalid) return;

    const newPassword = this.form.value.newPassword;

    this.authService.resetPassword(this.token, newPassword).subscribe({
      next: () => {
        Swal.fire('Başarılı', 'Şifre başarıyla güncellendi.', 'success');
        this.router.navigate(['/login']);
      },
    error: (err) => {
  console.log('BACKEND HATASI:', err); // bu önemli
  this.errorMessage = err.error?.message || 'Şifre sıfırlama başarısız!';
  Swal.fire('Hata', this.errorMessage, 'error');
}

    });
  }
}
