import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile } from '../models/user-profile';


@Injectable({
  providedIn: 'root'
})
export class UserProfileService {
  private apiUrl = 'https://localhost:7152/api/UserProfile';    

  constructor(private http: HttpClient) {}

  getCurrentUser(): Observable<UserProfile> {
    
    return this.http.get<UserProfile>(`${this.apiUrl}/current`);
  }

  updateProfile(data: UserProfile): Observable<any> {
    return this.http.put(`${this.apiUrl}`, data);
  }
}
