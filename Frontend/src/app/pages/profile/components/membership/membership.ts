import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CustomerApplicationResponse } from '../../../../models/customer-application-response';
import { BankApplicationService } from '../../../../services/bank-application.service';

@Component({
  selector: 'app-membership',
  standalone:true,
  imports: [CommonModule],
  templateUrl: './membership.html',
  styleUrl: './membership.css'
})
export class Membership implements OnInit{
  memberships: CustomerApplicationResponse[] = [];
  isLoading = false;
  constructor(private bankService:BankApplicationService){}
ngOnInit(): void {
  this.isLoading = true;
  this.bankService.getMemberships().subscribe({
    next: (res) => {
      this.memberships = res;
      this.isLoading = false;
    },
    error: () => {
      this.isLoading = false;
    }
  });
}
formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('tr-TR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
