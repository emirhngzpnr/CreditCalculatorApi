import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-kampanya-detay',
  standalone:true,
  imports: [CommonModule],
  templateUrl: './kampanya-detay.html',
  styleUrl: './kampanya-detay.css'
})
export class KampanyaDetay implements OnInit {
  kampanyaId!: number;
  kampanya: any;

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.kampanyaId = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getCampaigns().subscribe({
      next: (data) => {
        this.kampanya = data.find(k => k.id === this.kampanyaId);
        if (!this.kampanya) {
          alert('Kampanya bulunamadı');
          this.router.navigate(['/']);
        }
      },
      error: (err) => alert('Kampanya yüklenemedi: ' + err.message)
    });
  }

  krediBasvurusunaGit() {
    this.router.navigate(['/kredi-basvuru'], {
      queryParams: {
        id:this.kampanya.id,
        banka: this.kampanya.bankName,
        tip: this.kampanya.creditType
      }
    });
  }
}
