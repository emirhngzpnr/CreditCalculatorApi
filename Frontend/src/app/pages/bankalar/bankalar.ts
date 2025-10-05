import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient} from '@angular/common/http';
import { ApiService } from '../../services/api.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-bankalar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bankalar.html',
  styleUrl: './bankalar.css'
})
export class Bankalar implements OnInit  {
   bankalar: any[] = [];

  constructor(
    private api: ApiService,
    private router:Router

  ) {}

  ngOnInit(): void {
    this.api.getBanks().subscribe({
      next: (res) => {
        this.bankalar = res;
      },
      error: (err) => {
        alert('Bankalar yüklenemedi: ' + err.message);
      }
    });
  }
  
  musteriOl(bankName: string) {
    this.router.navigate(['/musteri-ol'], {
      queryParams: { banka: bankName }
    });
  }
}
