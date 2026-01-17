import { Component, computed, inject, signal, effect } from '@angular/core';
import { AuthService } from '../../../auth/services/auth.service';
import { UserInfoService } from '../../../user/services/user-info.service';
import { UserIngredientsService } from '../../../user-ingredients/services/useringredients.services';
import { BudgetService } from '../../../shared/services/budget.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user',
  imports: [CommonModule, FormsModule],
  templateUrl: './user.html',
  styleUrls: ['./user.css']
})
export class User {
  protected authService = inject(AuthService);
  public userInfoService = inject(UserInfoService);
  public userIngredientsService = inject(UserIngredientsService);
  private budgetService = inject(BudgetService);
  private router = inject(Router);

  showIngredients = signal(false);
  isEditingBudget = signal(false);
  editedBudgetValue = signal<number>(0);
  isSavingBudget = signal(false);

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

  /**
   * Rozpoczyna edycję budżetu
   */
  startEditingBudget() {
    // Round to 2 decimal places to avoid floating point precision issues
    const roundedBudget = Math.round(this.userBudget() * 100) / 100;
    this.editedBudgetValue.set(roundedBudget);
    this.isEditingBudget.set(true);
  }

  /**
   * Anuluje edycję budżetu
   */
  cancelEditingBudget() {
    this.isEditingBudget.set(false);
    this.editedBudgetValue.set(0);
  }

  /**
   * Zapisuje nowy budżet
   */
  saveBudget() {
    const newBudget = this.editedBudgetValue();

    if (newBudget < 0) {
      alert('Budżet nie może być ujemny');
      return;
    }

    this.isSavingBudget.set(true);

    this.budgetService.setBudget(newBudget).subscribe({
      next: (updatedBudget) => {
        this.userIngredientsService.refreshUserIngredients();
        this.isEditingBudget.set(false);
        this.isSavingBudget.set(false);
      },
      error: (err) => {
        console.error('Błąd podczas aktualizacji budżetu:', err);
        alert('Nie udało się zaktualizować budżetu');
        this.isSavingBudget.set(false);
      }
    });
  }
}
