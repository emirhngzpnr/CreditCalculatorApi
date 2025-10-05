import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BankResponse } from '../../models/bank-response';
import { ApiService } from '../../services/api.service';
import { BankRequest } from '../../models/bank-request';
import Swal from 'sweetalert2';


@Component({
  selector: 'app-admin-banks',
  standalone:true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './banks.html',
  styleUrl: './banks.css'
})
export class Banks implements OnInit {
  banks:BankResponse[]=[];
  bankForm;
  selectedBankId: number | null = null;

   constructor(private fb: FormBuilder, private api: ApiService) {
    this.bankForm = this.fb.group({
      name: ['', Validators.required]
    });
  }
  ngOnInit(): void {
    this.loadBanks(); // ← bu eksikti
  }

  loadBanks(){
    this.api.getBanks().subscribe({
      next:(res)=>this.banks=res,
      error:(err)=>alert("Bankalar Yüklenemedi: "+err.message)
    });
  }
 addBank() {
  if (this.bankForm.invalid) return;

  const newBank: BankRequest = {
    name: this.bankForm.value.name ?? ''
  };

  this.api.addBank(newBank).subscribe({
    next: (res) => {
      this.banks.push(res);
      this.bankForm.reset();
      Swal.fire({
        icon: 'success',
        title: 'Başarılı!',
        text: 'Banka başarıyla eklendi.',
        timer: 1500,
        showConfirmButton: false
      });
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'Hata!',
        text: 'Banka eklenemedi: ' + (err.message || 'Sunucu hatası.')
      });
    }
  });
}

  editBank(bank: BankResponse) {
  this.selectedBankId = bank.id;
  this.bankForm.patchValue({ name: bank.name });
}
updateBank() {
  if (!this.selectedBankId || this.bankForm.invalid) return;

  const updatedBank: BankRequest = {
    name: this.bankForm.value.name ?? ''
  };

  this.api.updateBank(this.selectedBankId, updatedBank).subscribe({
    next: () => {
      this.loadBanks();
      this.bankForm.reset();
      this.selectedBankId = null;
      Swal.fire({
        icon: 'success',
        title: 'Başarılı!',
        text: 'Banka bilgisi güncellendi.',
        timer: 1500,
        showConfirmButton: false
      });
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'Hata!',
        text: 'Güncelleme başarısız: ' + (err.message || 'Sunucu hatası.')
      });
    }
  });
}

cancelEdit() {
  this.selectedBankId = null;
  this.bankForm.reset();
  Swal.fire({
    icon: 'info',
    title: 'Düzenleme iptal edildi',
    timer: 1200,
    showConfirmButton: false
  });
}


  deleteBank(id: number) {
  Swal.fire({
    title: 'Emin misiniz?',
    text: 'Bu bankayı silmek istiyor musunuz?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Sil',
    cancelButtonText: 'İptal',
    confirmButtonColor: '#d33',
    cancelButtonColor: '#6c757d'
  }).then((result) => {
    if (result.isConfirmed) {
      this.api.deleteBank(id).subscribe({
        next: () => {
          this.banks = this.banks.filter(b => b.id !== id);
          Swal.fire({
            icon: 'success',
            title: 'Silindi!',
            text: 'Banka başarıyla silindi.',
            timer: 1500,
            showConfirmButton: false
          });
        },
        error: (err) => {
          Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: 'Silme işlemi başarısız: ' + (err.message || 'Sunucu hatası.')
          });
        }
      });
    }
  });
}

}
