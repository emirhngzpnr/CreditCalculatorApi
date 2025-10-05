import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators,ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ApiService } from '../../services/api.service';  
import { Router } from '@angular/router';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { BankResponse } from '../../models/bank-response';
import { CampaignResponse } from '../../models/campaign-application-response';




@Component({
  selector: 'app-credit-calculate',
  standalone:true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './credit-calculate.html',
  styleUrl: './credit-calculate.css'
})
export class CreditCalculateComponent {
 creditForm: FormGroup;
  result: any = null;
  submitted:boolean=false;
  bankalar: BankResponse[] = [];

  campaigns: CampaignResponse[] = [];
  

  constructor(private fb: FormBuilder, private http: HttpClient,private api:ApiService,private router:Router ) {
    this.creditForm = this.fb.group({
      krediTutari: [null, 
        [Validators.required, 
         Validators.min(1000),
         Validators.max(20000000)]],
      vade: [null, 
        [Validators.required, 
         Validators.min(1),
         Validators.max(240)]],
      faizOrani: [null, [Validators.required, Validators.min(0.01),Validators.max(100), Validators.pattern(/^\d+(\.\d{1,2})?$/)]]
    });
    
}
 submitForm() {
  this.submitted=true;
    if (this.creditForm.invalid) {
      this.creditForm.markAllAsTouched();
      return;
    }

    const formData = this.creditForm.value;

    this.api.hesaplaVeKaydet(this.creditForm.value).subscribe({
      next: (res) => {
        this.result = res;
        console.log('Kayıt Başarılı !'+ res)
      },
      error: (err) => {
       alert('Hesaplama hatası: ' + (err?.error?.message || err.message));
      }
    });
  }
  temizleFormu(): void {
  this.creditForm.reset();   // Formu temizler
 
  this.submitted = false;    // Hata mesajlarını sıfırlar
}

  
  ngOnInit(): void {
    this.api.getBanks().subscribe({
      next: (res) => {
        this.bankalar = res;
      },
      error: (err) => {
        alert('Bankalar yüklenemedi: ' + err.message);
      }
    });
    this.api.getCampaigns().subscribe({
    next: (res) => this.campaigns = res,
    error: (err) => alert('Kampanyalar yüklenemedi: ' + err.message)
  });
  }

 bankaSec(banka: BankResponse): void {
  this.router.navigate(['/bankalar'], {
    queryParams: { banka: banka.name }
  });
  }
/*kampanyayaGit(kampanya: any): void {
  this.router.navigate(['/kampanya-detay'], {
    queryParams: {
      banka: kampanya.bankaAdi,
      tip: kampanya.krediTipi
    }
  });
}*/
kampanyayaGit(kampanya: CampaignResponse): void {
    console.log('Kampanya:', kampanya); 
  this.router.navigate(['/kampanya-detay', kampanya.id]);

}
exportToPDF() {
  if (!this.result?.installments?.length) {
  alert("Önce hesaplama yapmalısınız.");
  return;
}
  const doc = new jsPDF();

  doc.setFontSize(14);
  doc.text('Kredi Itfa Tablosu', 14, 15);

  const headers = [['Taksit No', 'Taksit Tutari', 'Faiz', 'Anapara', 'Kalan Anapara']];
  
  const rows = (this.result.installments as any[]).map(item => [
  item.installmentNo,
  `${item.payment.toFixed(2)} TL`,
  `${item.interest.toFixed(2)} TL`,
  `${item.principal.toFixed(2)} TL`,
  `${item.remainingPrincipal.toFixed(2)} TL`
]);

  autoTable(doc, {
    startY: 20,
    head: headers,
    body: rows
  });

  doc.save('kredi-itfa-tablosu.pdf');
}


  
}



