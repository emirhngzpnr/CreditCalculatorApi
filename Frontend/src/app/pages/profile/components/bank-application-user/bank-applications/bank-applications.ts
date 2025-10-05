import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { BankApplication } from '../../../../../models/bank-application';
import { BankApplicationService } from '../../../../../services/bank-application.service';

@Component({
  selector: 'app-bank-applications',
  standalone:true,
  imports: [CommonModule],
  templateUrl: './bank-applications.html',
  styleUrl: './bank-applications.css'
})
export class BankApplications implements OnInit {
  applications: BankApplication[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(private bankService: BankApplicationService) {}

  ngOnInit(): void {
    this.isLoading = true;
    this.bankService.getMyBankApplications().subscribe({
      next: (res) => {
        this.applications = res;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Başvurular yüklenirken hata oluştu.';
        this.isLoading = false;
      }
    });
  }
 getApplicationStatus(status: number): string {
  switch (status) {
    case 0: return 'Reddedildi';
    case 1: return 'Onaylandı';
    case 2: return 'Beklemede';
    default: return 'Bilinmiyor';
  }
}
}
