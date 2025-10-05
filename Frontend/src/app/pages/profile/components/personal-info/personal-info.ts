import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import Swal from 'sweetalert2';
import { UserProfile } from '../../../../models/user-profile';
import { UserProfileService } from '../../../../services/user-profile.service';


@Component({
  selector: 'app-personal-info',
  standalone:true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './personal-info.html',
  styleUrl: './personal-info.css'
})
export class PersonalInfo implements OnInit {
 personalForm!: FormGroup;
  isEditing: boolean = false;
  userNumber: string = '';


  constructor(private fb: FormBuilder, private userProfileService:UserProfileService ) {}

  ngOnInit(): void {
    this.personalForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: [{ value: '', disabled: true }, [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern('^5\\d{9}$')]],
      
      birthDate: [{ value: '', disabled: true }],
  identityNumber: [{ value: '', disabled: true }]
    });

    //  Kullanıcı bilgilerini çek
    this.userProfileService.getCurrentUser().subscribe({
      
  next: (user: UserProfile) => {
    console.log('Kullanıcı verisi:', user);

    const formattedBirthDate = user.birthDate?.split('T')[0] || '';

    this.personalForm.patchValue({
      ...user,
      birthDate: formattedBirthDate,
      identityNumber: user.identityNumber 
      
    });

    this.personalForm.controls['email'].disable();
      this.userNumber = user.userNumber;
  },  
  error: () => {
    Swal.fire('Hata', 'Kullanıcı bilgileri alınamadı.', 'error');
  },
});

  }

  enableEdit() {
   this.isEditing = true;

  // Sadece değiştirilebilir alanları aç
  this.personalForm.controls['firstName'].enable();
  this.personalForm.controls['lastName'].enable();
  this.personalForm.controls['phoneNumber'].enable();

  // Geri kalanlar kilitli kalsın
  this.personalForm.controls['email'].disable();
  this.personalForm.controls['birthDate'].disable();
  this.personalForm.controls['identityNumber'].disable();
  }

  cancelEdit() {
    this.isEditing = false;
    this.personalForm.disable();
    this.personalForm.controls['email'].disable();
    this.personalForm.controls['birthDate'].disable(); 
  this.personalForm.controls['identityNumber'].disable(); 
  }

  saveChanges() {
    if (this.personalForm.invalid) {
      Swal.fire('Hata', 'Lütfen formu eksiksiz ve doğru doldurunuz.', 'error');
      return;
    }

    const updatedData = this.personalForm.getRawValue();
if (updatedData.birthDate) {
    updatedData.birthDate = new Date(updatedData.birthDate).toISOString();
  }
    //  Backend'e PUT isteği gönder
    this.userProfileService.updateProfile(updatedData).subscribe({
      next: () => {
        Swal.fire('Başarılı', 'Bilgileriniz güncellendi.', 'success');
        this.isEditing = false;
        this.personalForm.controls['email'].disable();
      },
      error: () => {
        Swal.fire('Hata', 'Güncelleme sırasında hata oluştu.', 'error');
      },
    });
  }
}
