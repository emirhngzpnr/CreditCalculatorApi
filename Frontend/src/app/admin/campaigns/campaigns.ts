import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CampaignResponse } from '../../models/campaign-application-response';
import { ApiService } from '../../services/api.service';
import { CampaignRequest } from '../../models/campaign-application-request';
import Swal from 'sweetalert2';


@Component({
  selector: 'app-campaigns',
  standalone:true,
  imports: [CommonModule,FormsModule,ReactiveFormsModule],
  templateUrl: './campaigns.html',
  styleUrl: './campaigns.css'
})
export class Campaigns implements OnInit {
  campaigns: CampaignResponse[] = [];
  campaignForm!: FormGroup;
  editingId: number | null = null;
  showForm = false;
  today: string = new Date().toISOString().split('T')[0];

  message = '';
  creditTypes = [
  { label: 'İhtiyaç Kredisi', value: 0 },
  { label: 'Konut Kredisi', value: 1 },
  { label: 'Taşıt Kredisi', value: 2   }
];


  constructor(private api: ApiService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.getCampaigns();
    this.campaignForm = this.fb.group({
      id:null,
      creditType: [1, Validators.required],
      minVade: [1, Validators.required],
      maxVade: [12, Validators.required],
      minKrediTutar: [10000, Validators.required],
      maxKrediTutar: [100000, Validators.required],
      baslangicTarihi: ['', Validators.required],
      bitisTarihi: ['', Validators.required],
      description: ['', Validators.required],
      faizOrani: [1, Validators.required],
      bankId: [1, Validators.required],
      
    });
  }

  getCampaigns() {
    this.api.getCampaigns().subscribe({
      next: (res) => (this.campaigns = res),
      error: () => (this.message = 'Kampanyalar yüklenemedi.')
    });
  }

  edit(campaign: CampaignResponse) {
    this.editingId = campaign.id;
    this.showForm = true;
    this.campaignForm.patchValue(campaign);
  }

  delete(id: number) {
  Swal.fire({
    title: 'Emin misiniz?',
    text: 'Bu kampanyayı silmek istiyor musunuz?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Evet, sil',
    cancelButtonText: 'İptal'
  }).then((result) => {
    if (result.isConfirmed) {
      this.api.deleteCampaign(id).subscribe({
        next: () => {
          this.getCampaigns();
          Swal.fire('Silindi!', 'Kampanya başarıyla silindi.', 'success');
        },
        error: () => {
          Swal.fire('Hata!', 'Kampanya silinirken bir hata oluştu.', 'error');
        }
      });
    }
  });
}


  submitForm() {
  const formValue: CampaignRequest = this.campaignForm.value;

  if (this.editingId) {
    this.api.updateCampaign(this.editingId, formValue).subscribe({
      next: () => {
        Swal.fire('Güncellendi!', 'Kampanya başarıyla güncellendi.', 'success');
        this.getCampaigns();
        this.cancelEdit();
      },
      error: () => {
        Swal.fire('Hata!', 'Güncelleme sırasında hata oluştu.', 'error');
      }
    });
  } else {
    this.api.createCampaign(formValue).subscribe({
      next: () => {
        Swal.fire('Eklendi!', 'Kampanya başarıyla eklendi.', 'success');
        this.getCampaigns();
        this.campaignForm.reset();
        this.showForm = false;
      },
      error: () => {
        Swal.fire('Hata!', 'Ekleme sırasında hata oluştu.', 'error');
      }
    });
  }
}


  cancelEdit() {
    this.editingId = null;
    this.showForm = false;
    this.campaignForm.reset();
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

}
