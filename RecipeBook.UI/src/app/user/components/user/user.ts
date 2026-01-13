import { Component, computed, inject, signal, effect } from '@angular/core';
import { AuthService } from '../../../auth/services/auth.service';
import { UserInfoService } from '../../../user/services/user-info.service';
import { UserIngredientsService } from '../../../user-ingredients/services/useringredients.services';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user',
  imports: [CommonModule],
  templateUrl: './user.html',
  styleUrls: ['./user.css']
})
export class User {
  protected authService = inject(AuthService);
  public userInfoService = inject(UserInfoService);
  public userIngredientsService = inject(UserIngredientsService);
  private router = inject(Router);

  showIngredients = signal(false);

  constructor() {
    effect(() => {
      const user = this.userInfoService.currentUser();
      if (user) {
        this.userIngredientsService.refreshUserIngredients();
      }
    });
  }

  toggleIngredients() {
    this.showIngredients.set(!this.showIngredients());
  }

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/recipes']);
  }

  userBudget = computed(() => this.userIngredientsService.getCurrentBudget());

  userIngredients = computed(() => this.userIngredientsService.userIngredients());
}
