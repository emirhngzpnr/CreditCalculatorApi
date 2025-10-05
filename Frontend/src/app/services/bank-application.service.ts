import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BankApplication } from "../models/bank-application";
import { Observable } from "rxjs";
import { CustomerApplicationResponse } from "../models/customer-application-response";

@Injectable({
  providedIn: 'root'
})
export class BankApplicationService {
  private apiUrl = 'https://localhost:7152/api/customers';

  constructor(private http: HttpClient) {}

  getMyBankApplications(): Observable<BankApplication[]> {
    return this.http.get<BankApplication[]>(`${this.apiUrl}/my`);
  }
  getMemberships(): Observable<CustomerApplicationResponse[]> {
  return this.http.get<CustomerApplicationResponse[]>(`${this.apiUrl}/memberships`);
}

}