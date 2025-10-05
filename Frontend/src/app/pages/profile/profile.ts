import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { PersonalInfo } from './components/personal-info/personal-info';

import { CreditApplications } from './components/credit-applications-user/credit-applications';

import { MyCredits } from './components/my-credits/my-credits';
import { Membership } from './components/membership/membership';
import { BankApplications } from './components/bank-application-user/bank-applications/bank-applications';


@Component({
  selector: 'app-profile',
  standalone:true,
  imports: [CommonModule,
    PersonalInfo,
    BankApplications,
    CreditApplications,
    Membership,
    MyCredits],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile {
 activeTab: string = 'personal-info';

  setTab(tab: string) {
    this.activeTab = tab;
  }
}
