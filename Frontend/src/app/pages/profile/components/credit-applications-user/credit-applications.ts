import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CreditApplication } from '../../../../models/credit-application.model';
import { CreditApplicationService } from '../../../../services/credit-application.service';

@Component({
  selector: 'app-credit-applications',
  standalone:true,
  imports: [CommonModule],
  templateUrl: './credit-applications.html',
  styleUrl: './credit-applications.css'
})
export class CreditApplications implements OnInit {
  applications: CreditApplication[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(private creditService: CreditApplicationService ) {}

  ngOnInit(): void {
    this.creditService.getMyApplications().subscribe({
      next: (res) => {
        this.applications = res;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Başvurular alınamadı.';
        this.isLoading = false;
        console.error(err);
      },
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
