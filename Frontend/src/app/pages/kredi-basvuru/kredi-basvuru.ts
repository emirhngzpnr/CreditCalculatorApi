import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { CreditType } from '../../enums/credit-type';
import { BankResponse } from '../../models/bank-response';
import { CampaignResponse } from '../../models/campaign-application-response';
import { ActivatedRoute, Router } from '@angular/router';
import Swal from 'sweetalert2';
import { UserProfileService } from '../../services/user-profile.service';


@Component({
  selector: 'app-kredi-basvuru',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './kredi-basvuru.html',
  styleUrl: './kredi-basvuru.css',
  
})
export class KrediBasvuru {
  basvuruForm: FormGroup;
  successMessage = '';
  errorMessage = '';
  bankalar: BankResponse[] = [];
  kampanyalar: CampaignResponse[] = [];
  selectedCampaign: CampaignResponse|null = null;
  hesaplamaSonucu: any = null;


   submitted:boolean=false;
   creditTypes = [
    { label: 'İhtiyaç Kredisi', value: CreditType.Ihtiyac },
    { label: 'Konut Kredisi', value: CreditType.Konut },
    { label: 'Taşıt Kredisi', value: CreditType.Tasit },
  ];
  constructor(private fb: FormBuilder, private api: ApiService,private userProfileService:UserProfileService, private route: ActivatedRoute,private router:Router ) {
    this.basvuruForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
       phoneNumber: ['', [Validators.required, Validators.pattern(/^0?\d{10}$/)]],
      bankName: ['', Validators.required],
      creditType: ['', Validators.required],
      campaignId: [null, Validators.required],
      creditAmount: [null, [Validators.required, Validators.min(1000),Validators.max(10000000)]],
      creditTerm: [null, [Validators.required, Validators.min(2), Validators.max(240)]],
    
      monthlyIncome: [null, [Validators.required, Validators.min(1000),Validators.max(10000000  )]]
    });
  }
  clearCampaigns() {
  this.kampanyalar = [];
  this.selectedCampaign = null;
  this.basvuruForm.patchValue({
    campaignId: null,
    creditAmount: null,
    creditTerm: null
  });
}
kampanyaDegisti(event: Event): void {
  const selectedId = Number((event.target as HTMLSelectElement).value);
  const kampanya = this.kampanyalar.find(k => k.id === selectedId);

  if (kampanya) {
    this.selectedCampaign = kampanya;
    // Varsayılan min değerlerle doldur
    this.basvuruForm.patchValue({
      creditAmount: kampanya.minKrediTutar,
      creditTerm: kampanya.minVade
    });
  } else {
    this.selectedCampaign = null;
    this.basvuruForm.patchValue({
      creditAmount: null,
      creditTerm: null
    });
  }
}




ngOnInit(): void {
  this.userProfileService.getCurrentUser().subscribe({
    next: (user) => {
      this.basvuruForm.patchValue({
        fullName: `${user.firstName} ${user.lastName}`,
        email: user.email,
        phoneNumber: user.phoneNumber
      });

      // İsteğe bağlı olarak disable edebilirsin
      this.basvuruForm.get('fullName')?.disable();
      this.basvuruForm.get('email')?.disable();
      this.basvuruForm.get('phoneNumber')?.disable();
    },
    error: (err) => console.error('Kullanıcı bilgisi alınamadı:', err)
  });
  this.api.getBanks().subscribe({
    next: (res) => (this.bankalar = res),
    error: (err) => console.error('Bankalar alınamadı:', err)
  });

  this.route.queryParams.subscribe((params) => {
    const banka = params['banka'];
    const gelenTip = params['tip'];
    let tip: number;

    switch (gelenTip) {
      case 'İhtiyaç':
      case 'Ihtiyac':
        tip = CreditType.Ihtiyac;
        break;
      case 'Konut':
        tip = CreditType.Konut;
        break;
      case 'Taşıt':
      case 'Tasit':
        tip = CreditType.Tasit;
        break;
      default:
        tip = CreditType.Ihtiyac;
        break;
    }

    const kampanyaId = Number(params['id']);

    if (banka && !isNaN(tip)) {
      this.basvuruForm.patchValue({
        bankName: banka,
        creditType: tip
      });

      this.api.getCampaignsByBankAndType(banka, tip).subscribe({
        next: (res) => {
          this.kampanyalar = res;
          const kampanya = res.find(k => k.id === kampanyaId);
          if (kampanya) {
            this.selectedCampaign = kampanya;
            this.basvuruForm.patchValue({
              campaignId: kampanya.id,
              creditAmount: kampanya.minKrediTutar,
              creditTerm: kampanya.minVade
            });
          }
        },
        error: (err) => console.error('Kampanyalar alınamadı:', err)
      });
    }
  });

  //  Her durumda dinlemeler aktif
  this.basvuruForm.get('bankName')?.valueChanges.subscribe(() => {
    this.clearCampaigns();
    this.loadCampaigns();
  });

  this.basvuruForm.get('creditType')?.valueChanges.subscribe(() => {
    this.clearCampaigns();
    this.loadCampaigns();
  });
}



loadCampaigns() {
  this.clearCampaigns();

  const bankName = this.basvuruForm.value.bankName;
  const creditType = this.basvuruForm.value.creditType;

  console.log('Kampanya yükleniyor:', { bankName, creditType });

  if (bankName && creditType) {
    this.api.getCampaignsByBankAndType(bankName, creditType).subscribe({
      next: (res) => {
        console.log('Kampanyalar geldi:', res);
        this.kampanyalar = res;
      },
      error: (err) => {
        console.error('Kampanyalar yüklenemedi:', err);
        this.kampanyalar = [];
      }
    });
  }
}



  

submitForm() {
  this.submitted = true;

  if (this.basvuruForm.invalid) {
    this.basvuruForm.markAllAsTouched();
    return;
  }

  const formData = {
    ...this.basvuruForm.getRawValue(),
    creditType: Number(this.basvuruForm.value.creditType),
    campaignId: Number(this.basvuruForm.value.campaignId),
  };

  //  Müşteri kontrolü
  this.api.isCustomer(formData.email, formData.bankName).subscribe({
    next: (isCustomer: boolean) => {
      if (!isCustomer) {
        Swal.fire({
          icon: 'info',
          title: 'Müşteri Olmanız Gerekli',
          text: `${formData.bankName} bankasına kredi başvurusu yapabilmek için önce müşteri olmalısınız.`,
          confirmButtonText: 'Müşteri Ol',
          cancelButtonText:'İptal Et',
          showCancelButton: true
        }).then(result => {
          if (result.isConfirmed) {
            this.router.navigate(['/musteri-ol'], {
              queryParams: { banka: formData.bankName }
            });
          }
        });
        return;
      }

      //  Müşteriyse kredi başvurusu gönder
      this.api.basvuruGonder(formData).subscribe({
        next: (res) => {
          console.log("Gönderilen veri:", formData);
          this.basvuruForm.reset();
          this.submitted = false;
          this.hesaplamaSonucu = res;

          Swal.fire({
            icon: 'success',
            title: 'Başvuru Tamamlandı',
            text: 'Başvurunuz hesaplandı ve kaydedildi.',
            confirmButtonColor: '#3085d6',
            confirmButtonText: 'Tamam'
          });
        },
        error: (err) => {
          const code = err.error?.code?.toUpperCase?.();

          if (err.status === 400 && code === 'DUPLICATE_EMAIL') {
            Swal.fire({
              icon: 'error',
              title: 'Başvuru Hatası',
              text: 'Bu bankaya zaten bir başvuru yaptınız.',
              confirmButtonColor: '#d33'
            });
          } else if (err.status === 400 && code === 'APPLICATION_ERROR') {
            Swal.fire({
              icon: 'error',
              title: 'Hata',
              text: 'Sunucu hatası veya geçersiz başvuru. Lütfen kontrol edin.',
              confirmButtonColor: '#d33'
            });
          } else {
            Swal.fire({
              icon: 'error',
              title: 'Hata',
              text: 'Bilinmeyen bir hata oluştu.',
              confirmButtonColor: '#d33'
            });
          }
        }
      });
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'Kontrol Hatası',
        text: 'Müşteri kontrolü sırasında bir hata oluştu.',
        confirmButtonColor: '#d33'
      });
    }
  });
}

odemePlaniniGoster() {
  this.submitted = true;

  if (
    this.basvuruForm.get('creditAmount')?.invalid ||
    this.basvuruForm.get('creditTerm')?.invalid ||
    !this.selectedCampaign?.faizOrani
  ) {
    Swal.fire({
      icon: 'error',
      title: 'Eksik Bilgi',
      text: 'Lütfen kampanya seçin ve geçerli kredi bilgilerini girin.',
      confirmButtonColor: '#d33'
    });
    return;
  }

  const krediHesaplama = {
    krediTutari: this.basvuruForm.get('creditAmount')?.value,
    vade: this.basvuruForm.get('creditTerm')?.value,
    faizOrani: this.selectedCampaign.faizOrani
  };

  this.api.hesaplaVeKaydet(krediHesaplama).subscribe({
    next: (res) => {
      this.hesaplamaSonucu = res;
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'Hesaplama Hatası',
        text: 'Ödeme planı hesaplanırken bir hata oluştu.',
        confirmButtonColor: '#d33'
      });
    }
  });
}
syncCreditAmount(event: Event): void {
  const inputValue = Number((event.target as HTMLInputElement).value);
  this.basvuruForm.get('creditAmount')?.setValue(inputValue);
}

syncCreditTerm(event: Event): void {
  const inputValue = Number((event.target as HTMLInputElement).value);
  this.basvuruForm.get('creditTerm')?.setValue(inputValue);
}

}
