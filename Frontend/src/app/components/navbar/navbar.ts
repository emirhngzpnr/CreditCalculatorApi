import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class Navbar implements OnInit {
   isLoggedIn = false;
  userName = '';

  constructor(private router: Router,private authService:AuthService) {}

 ngOnInit(): void {
    // anlık login state takibi
    this.authService.loginState$.subscribe((loggedIn) => {
      this.isLoggedIn = loggedIn;

      if (loggedIn) {
        const token = this.authService.getToken();
        if (token) {
          try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            this.userName = payload?.firstName || 'Kullanıcı';
          } catch {
            this.userName = 'Kullanıcı';
          }
        }
      } else {
        this.userName = '';
      }
    });
  }

  logout(): void {
    this.authService.logout();
   //  this.userName = '';
    this.router.navigate(['/login']);
  }
}
