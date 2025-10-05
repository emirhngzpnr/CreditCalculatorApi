import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CreditApplication } from '../../../../models/credit-application.model';
import { CreditApplicationService } from '../../../../services/credit-application.service';

@Component({
  selector: 'app-my-credits',
  standalone:true,
  imports: [CommonModule],
  templateUrl: './my-credits.html',
  styleUrl: './my-credits.css'
})
export class MyCredits implements OnInit {
  credits: CreditApplication[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(private creditService: CreditApplicationService) {}

  ngOnInit(): void {
    this.fetchApprovedCredits();
  }

  fetchApprovedCredits(): void {
    this.isLoading = true;
    this.creditService.getApprovedCredits().subscribe({
      next: (data) => {
        this.credits = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Veriler alınırken hata oluştu.';
        this.isLoading = false;
      },
    });
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 0:
        return 'Beklemede';
      case 1:
        return 'Onaylandı';
      case 2:
        return 'Reddedildi';
      default:
        return 'Bilinmiyor';
    }
  }

}
