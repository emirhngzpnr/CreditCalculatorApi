import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreditApplication } from '../models/credit-application.model';

@Injectable({
  providedIn: 'root',
})
export class CreditApplicationService {
  private apiUrl = 'https://localhost:7152/api/CreditApplications'; 

  constructor(private http: HttpClient) {}

 
  getMyApplications(): Observable<CreditApplication[]> {
    return this.http.get<CreditApplication[]>(`${this.apiUrl}/me`);
  }

getApprovedCredits(): Observable<CreditApplication[]> {
  return this.http.get<CreditApplication[]>(`${this.apiUrl}/approved`);
}

}
