import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import Swal from 'sweetalert2';


@Component({
  selector: 'app-credit-applications',
  standalone:true,
  imports: [CommonModule,ReactiveFormsModule,FormsModule],
  templateUrl: './credit-applications.html',
  styleUrl: './credit-applications.css'
})
export class CreditApplications implements OnInit {
  basvurular:any[]=[]
  statusOptions = [
    { label: 'Onaylandı', value: 1 },
    { label: 'Reddedildi', value: 0 },
    { label: 'Beklemede', value: 2 }
  ]; 
  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getBasvurular().subscribe({
      next: (res) => {
        this.basvurular = res;
        console.log('Başvurular yüklendi:', res);
      },
      error: (err) => {
        console.error('Başvurular yüklenemedi:', err);
      }
    }); 
  }

  guncelleStatus(basvuruId: number, yeniStatus: any) {
    Swal.fire({
      title: 'Emin misiniz?',
      text: 'Bu başvurunun durumunu güncellemek istiyor musunuz?',
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Evet, güncelle',
      cancelButtonText: 'İptal'
    }).then((result) => {
      if (result.isConfirmed) {
        this.api.updateBasvuruStatus(basvuruId, Number(yeniStatus)).subscribe({
          next: () => {
            const basvuru = this.basvurular.find(x => x.id === basvuruId);
            if (basvuru) basvuru.status = Number(yeniStatus);
            Swal.fire('Başarılı!', 'Durum başarıyla güncellendi.', 'success');
          },
          error: () => {
            Swal.fire('Hata!', 'Güncelleme sırasında bir hata oluştu.', 'error');
          }
        });
      }
    });
  }
  silBasvuru(basvuruId: number) {
    Swal.fire({
      title: 'Silmek istediğinize emin misiniz?',
      text: 'Bu işlem geri alınamaz!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Evet, sil',
      cancelButtonText: 'İptal'
    }).then((result) => {
      if (result.isConfirmed) {
        this.api.deleteBasvuru(basvuruId).subscribe({
          next: () => {
            this.basvurular = this.basvurular.filter(b => b.id !== basvuruId);
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
