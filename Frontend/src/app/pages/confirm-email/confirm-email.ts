import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import Swal from 'sweetalert2';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-email.html',
  styleUrl: './confirm-email.css',
})
export class ConfirmEmail implements OnInit {
    confirmationMessage: string = '';
  error: boolean = false;
  countdown: number = 5;

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private router:Router
  ) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.authService.confirmEmail(token).subscribe({
        next: (res) => {
          this.confirmationMessage = res;
          this.error = false;
          this.startCountdown();
        },
        error: (err) => {
          this.error = true;
          this.confirmationMessage =
            err.error?.message || 'Doğrulama başarısız.';
        },
      });
    } else {
      this.error = true;
      this.confirmationMessage = 'Doğrulama tokenı eksik.';
    }
  }

  startCountdown() {
    const interval = setInterval(() => {
      this.countdown--;
      if (this.countdown === 0) {
        clearInterval(interval);
        this.router.navigate(['/login']);
      }
    }, 1000);
  }
}
