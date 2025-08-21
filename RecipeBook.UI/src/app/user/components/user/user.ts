import { Component,computed,inject, signal } from '@angular/core';
import { AuthService } from '../../../auth/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user',
  imports: [CommonModule],
  templateUrl: './user.html',
  styleUrl: './user.css'
})

export class User {
protected authService = inject(AuthService);

showIngredients = signal(false);

toggleIngredients() {
    this.showIngredients.set(!this.showIngredients());
  }

onLogout() {
    this.authService.logout();}
}
