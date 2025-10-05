import { Component, OnInit } from '@angular/core';
import { CustomerApplicationResponse } from '../../models/customer-application-response';
import { ApiService } from '../../services/api.service';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-admin-customerapplication',
  imports: [FormsModule,ReactiveFormsModule,CommonModule],
  templateUrl: './admin-customerapplication.html',
  styleUrl: './admin-customerapplication.css',
  standalone:true
  
})
export class AdminCustomerapplication implements OnInit {
   selectedBankName: string = '';
  customerApplications: CustomerApplicationResponse[] = [];
  banks: any[] = [];
statusOptions = [
  { value: 0, label: 'Reddedildi' },
  { value: 1, label: 'Onaylandı' },
  { value: 2, label: 'Beklemede' }
];

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getBanks().subscribe(res => this.banks = res);
  }

  getApplications() {
  this.api.getCustomerApplicationsByBank(this.selectedBankName).subscribe({
    next: (res) => {
      this.customerApplications = res;

      if (this.customerApplications.length === 0) {
        Swal.fire({
          icon: 'info',
          title: 'Bilgi',
          text: 'Seçilen bankaya ait müşteri başvurusu bulunmamaktadır.'
        });
      }
    },
    error: (err) => {
      console.error('Başvurular alınırken hata oluştu:', err);
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Başvurular alınırken bir hata oluştu.'
      });
    }
  });
}

updateStatus(id: number, newStatus: number) {
  Swal.fire({
    title: 'Durumu güncellemek istiyor musunuz?',
    icon: 'question',
    showCancelButton: true,
    confirmButtonText: 'Evet, güncelle',
    cancelButtonText: 'İptal',
    confirmButtonColor: '#3085d6',
    cancelButtonColor: '#d33'
  }).then((result) => {
    if (result.isConfirmed) {
      this.api.updateCustomerApplicationStatus(id, newStatus).subscribe({
        next: () => {
          const index = this.customerApplications.findIndex(x => x.id === id);
          if (index !== -1) {
            this.customerApplications[index] = {
              ...this.customerApplications[index],
              status: newStatus
            };
          }

          Swal.fire({
            icon: 'success',
            title: 'Başarılı!',
            text: 'Başvuru durumu güncellendi.',
            timer: 1500,
            showConfirmButton: false
          });
        },
        error: (err) => {
          Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: 'Güncelleme sırasında bir hata oluştu.'
          });
          console.error('Güncelleme hatası:', err);
        }
      });
    }
  });
}

deleteApplication(id: number) {
  Swal.fire({
    title: 'Silmek istediğinize emin misiniz?',
    text: 'Bu işlem geri alınamaz!',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Evet, sil',
    cancelButtonText: 'İptal'
  }).then((result) => {
    if (result.isConfirmed) {
      this.api.deleteCustomerApplication(id).subscribe({
        next: () => {
          this.customerApplications = this.customerApplications.filter(app => app.id !== id);
          Swal.fire('Silindi!', 'Başvuru başarıyla silindi.', 'success');
        },
        error: () => {
          Swal.fire('Hata!', 'Silme işlemi sırasında bir hata oluştu.', 'error');
        }
      });
    }
  });
}


}

