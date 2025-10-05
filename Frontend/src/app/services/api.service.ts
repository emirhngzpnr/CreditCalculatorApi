import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BankResponse } from '../models/bank-response';
import { BankRequest } from '../models/bank-request';
import { CampaignResponse } from '../models/campaign-application-response';
import { CampaignRequest } from '../models/campaign-application-request';
import { CustomerApplicationResponse } from '../models/customer-application-response';
import { CustomerApplicationRequest } from '../models/customer-application-request';
import { RegisterRequest } from '../models/register-request';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private BASE_URL = 'https://localhost:7152/api';

  constructor(private http: HttpClient) {}

  // Kredi hesapla
  hesaplaKredi(data: any): Observable<any> {
    return this.http.post(`${this.BASE_URL}/credits/hesapla`, data);
  }
  // Kredi hesapla ve veritabanına kaydet
  hesaplaVeKaydet(data: any): Observable<any> {
    return this.http.post(`${this.BASE_URL}/credits/hesapla-ve-kaydet`, data);
  }

  // Kredi başvurusu oluştur
  basvuruGonder(data: any): Observable<any> {
    return this.http.post(`${this.BASE_URL}/CreditApplications`, data);
  }

  // Tüm başvuruları getir
  getBasvurular(): Observable<any[]> {
    return this.http.get<any[]>(`${this.BASE_URL}/CreditApplications`);
  }

 // Tüm bankaları getir
  getBanks(): Observable<BankResponse[]> {
  return this.http.get<BankResponse[]>(`${this.BASE_URL}/banks`);
}
// Banka ekle
addBank(data: BankRequest): Observable<BankResponse> {
  return this.http.post<BankResponse>(`${this.BASE_URL}/banks`, data);
}
// Bankaları güncelle
updateBank(id: number, data: BankRequest): Observable<void> {
  return this.http.put<void>(`${this.BASE_URL}/banks/${id}`, data);
}
// Banka sil
deleteBank(id: number): Observable<void> {
  return this.http.delete<void>(`${this.BASE_URL}/banks/${id}`);
}

  
  // Bankaları faiz aralıklarıyla getir
  getBanksWithInterest(): Observable<any[]> {
    return this.http.get<any[]>(`${this.BASE_URL}/banks/with-interest`);
  }
  // Banka müşterisi başvurusu
  //bankaMusterisiOl(data: any): Observable<any> {
    //return this.http.post(`${this.BASE_URL}/customers/apply`, data);
  //}
  bankaMusterisiOl(data: CustomerApplicationRequest): Observable<CustomerApplicationResponse> {
  return this.http.post<CustomerApplicationResponse>(`${this.BASE_URL}/customers/apply`, data);
}

  // Kampanyaları getir
  getCampaigns(): Observable<CampaignResponse[]> {
    return this.http.get<CampaignResponse[]>(`${this.BASE_URL}/campaigns`);
  }
  // kampanya oluştur
 createCampaign(campaign: CampaignRequest): Observable<void> {
    return this.http.post<void>(`${this.BASE_URL}/campaigns`, campaign);
  }
  // kampanya güncelle
   updateCampaign(id: number, campaign: CampaignRequest): Observable<void> {
    return this.http.put<void>(`${this.BASE_URL}/campaigns/${id}`, campaign);
  }
  // kampanya sil
  deleteCampaign(id: number): Observable<void> {
    return this.http.delete<void>(`${this.BASE_URL}/campaigns/${id}`);
  }
  // kampanyaları bankalara göre getir
getCampaignsByBankAndType(bankName: string, creditType: number) {
  return this.http.get<CampaignResponse[]>(
    `${this.BASE_URL}/campaigns/filter?bankName=${bankName}&creditType=${creditType}`
  );
  // kampanyaları kredi türlerine göre getir
}
getCampaignsByCreditType(creditType: number): Observable<CampaignResponse[]> {
  return this.http.get<CampaignResponse[]>(`${this.BASE_URL}/campaigns/filter-by-type?creditType=${creditType}`);
}
// kampanyaları bankalara göre getir
getCampaignsByBank(bankName: string): Observable<CampaignResponse[]> {
  return this.http.get<CampaignResponse[]>(`${this.BASE_URL}/campaigns/filter-by-bank?bankName=${bankName}`);
}
// basvuru durumunu güncelle
updateBasvuruStatus(id: number, newStatus: number): Observable<void> {
  return this.http.put<void>(`${this.BASE_URL}/CreditApplications/status/${id}`, {
    applicationId: id,
    status: newStatus
  });
  
}
// başvuru sil
deleteBasvuru(id: number): Observable<void> {
  return this.http.delete<void>(`${this.BASE_URL}/CreditApplications/${id}`);
}

  // kampanya başvuru
  kampanyaBasvuru(data: any): Observable<any> {
    return this.http.post(`${this.BASE_URL}/campaignapplications/apply`, data);
  }
  // banka ismine göre başvuruları getirme
 getCustomerApplicationsByBank(bankName: string): Observable<CustomerApplicationResponse[]> {
  return this.http.get<CustomerApplicationResponse[]>(`${this.BASE_URL}/customers/by-bank/${bankName}`);
}
isCustomer(email: string, bankName: string): Observable<boolean> {
  return this.http.get<boolean>(`${this.BASE_URL}/customers/is-customer`, {
    params: { email, bankName }
  });
}

updateCustomerApplicationStatus(id: number, newStatus: number): Observable<void> {
  return this.http.put<void>(`${this.BASE_URL}/customers/status/${id}`, {
    status: newStatus
  });
}
deleteCustomerApplication(id: number): Observable<void> {
  return this.http.delete<void>(`${this.BASE_URL}/customers/${id}`);
}




}
