import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { Navbar } from '../../components/navbar/navbar';

@Component({
  selector: 'app-default-layout',
  standalone:true,
  imports: [RouterOutlet, Navbar],
  templateUrl: './default-layout.html',
  styleUrl: './default-layout.css'
})
export class DefaultLayout {

}
