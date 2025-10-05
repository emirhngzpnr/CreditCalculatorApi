import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { AbstractControl, ValidationErrors } from '@angular/forms';
import Swal from 'sweetalert2';
import { UserProfileService } from '../../services/user-profile.service';


// 18 yaş kontrolü
export function minimumAgeValidator(minAge: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    const inputDate = new Date(control.value);
    if (isNaN(inputDate.getTime())) return null;

    const today = new Date();
    let age = today.getFullYear() - inputDate.getFullYear();

    const hasHadBirthdayThisYear =
      today.getMonth() > inputDate.getMonth() ||
      (today.getMonth() === inputDate.getMonth() &&
        today.getDate() >= inputDate.getDate());

    if (!hasHadBirthdayThisYear) age--;

    return age >= minAge
      ? null
      : { minAge: { requiredAge: minAge, actualAge: age } };
  };
}

@Component({
  selector: 'app-musteri-ol',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './musteri-ol.html',
  styleUrl: './musteri-ol.css',
})
export class MusteriOl implements OnInit {
  basvuruForm!: FormGroup;
  seciliBanka: string | null = null;
  bankalar: string[] = [];
  successMessage = '';
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private api: ApiService,
    private userProfileService:UserProfileService
  ) {}

  ngOnInit(): void {
    // 1) Formu oluştur
    this.basvuruForm = this.fb.group({
      name: ['', Validators.required],
      surname: ['', Validators.required],

      //  TC Kimlik No: Sadece 11 haneli rakam
      identityNumber: [
        '',
        [Validators.required, Validators.pattern(/^\d{11}$/)],
      ],

      //  Telefon: 10 ya da 11 haneli rakam
      phone: ['', [Validators.required, Validators.pattern(/^0?\d{10}$/)]],

      //  Doğum Tarihi: En az 18 yaşında olmalı 
      birthDate: ['', [Validators.required, minimumAgeValidator(18)]],

      email: ['', [Validators.required, Validators.email]],
      bankName: [
        { value: '', disabled: !!this.seciliBanka },
        Validators.required,
      ],
    });

    // 2) Query param varsa bankayı ata (form oluşturulduktan sonra)
    this.route.queryParams.subscribe((params) => {
      this.seciliBanka = params['banka'] || null;
      if (this.seciliBanka) {
        this.basvuruForm.patchValue({ bankName: this.seciliBanka });
      }
    });

    // 3) Tüm bankaları getir
    this.api.getBanks().subscribe({
      next: (res) => {
        this.bankalar = res.map((b) => b.name);
      },
      error: () => {
        alert('Bankalar yüklenemedi');
      },
    });
    this.userProfileService.getCurrentUser().subscribe({
  next: (user) => {
    this.basvuruForm.patchValue({
      name: user.firstName,
      surname: user.lastName,
      identityNumber: user.identityNumber,
      phone: user.phoneNumber,
      email: user.email
    });

    // Gerekirse alanları pasif yap
    this.basvuruForm.get('name')?.disable();
    this.basvuruForm.get('surname')?.disable();
    this.basvuruForm.get('identityNumber')?.disable();
    this.basvuruForm.get('phone')?.disable();
    this.basvuruForm.get('email')?.disable();
  },
  error: (err) => {
    console.error('Kullanıcı bilgisi alınamadı:', err);
  }
});
  }

  submit(): void {
  if (this.basvuruForm.invalid) {
    this.basvuruForm.markAllAsTouched();
    return;
  }

  const formData = {
    ...this.basvuruForm.getRawValue(),
  };

  this.api.bankaMusterisiOl(formData).subscribe({
    next: () => {
     

      Swal.fire({
        icon: 'success',
        title: 'Başvuru Başarılı',
        text: 'Müşteri başvurunuz başarıyla alındı!',
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'Tamam'
      });
    },

    error: (err) => {
      const errorMsg = err?.error?.message?.toLowerCase?.() || '';

      let mesaj = 'Beklenmeyen bir hata oluştu.';

      if (err.status === 400 && errorMsg.includes('tc') && errorMsg.includes('banka')) {
        mesaj = 'Bu TC ile bu bankaya zaten başvuru yaptınız.';
      } else if (err.status === 400) {
        mesaj = err?.error?.message || 'Geçersiz başvuru. Lütfen bilgilerinizi kontrol edin.';
      } else if (err.status === 500) {
        mesaj = 'Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.';
      }

      Swal.fire({
        icon: 'error',
        title: 'Başvuru Hatası',
        text: mesaj,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Kapat'
      });

      console.error('API Hatası:', err);
    }
  });
}

}
