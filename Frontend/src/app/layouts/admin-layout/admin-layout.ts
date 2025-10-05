import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { Banks } from '../../admin/banks/banks';
import { Campaigns } from '../../admin/campaigns/campaigns';

@Component({
  selector: 'app-admin-layout',
  standalone:true,
  imports: [RouterOutlet,RouterModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css'
})
export class AdminLayout {
 logout() {
    localStorage.clear();
    window.location.href = '/'; // veya router.navigate(['/login']) gibi bir yönlendirme
  }
}
