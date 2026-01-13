import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserIngredientsService } from '../services/useringredients.services';
import { IngredientService } from '../../ingredients/services/ingredient.service';
import { UserIngredientDtoModel } from '../models/user-ingredient-dto.model';
import { Ingredient } from '../../ingredients/models/ingredient.model';
import { userIngredientCreateDto } from '../models/user-ingredient-dto-create.model';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-user-ingredients',
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './user-ingredients.html',
  styleUrl: './user-ingredients.css'
})
export class UserIngredients {
  private fb = inject(FormBuilder);
  private userIngredientService = inject(UserIngredientsService);
  private ingredientService = inject(IngredientService);

  userIngredients = signal<UserIngredientDtoModel[]>([]);
  allIngredients = signal<Ingredient[]>([]);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isSubmitting = signal<boolean>(false);
  editingIngredientId = signal<number | null>(null);
  editQuantity = signal<number>(0);

  form = this.fb.group({
    ingredientId: [0],
    ingredientName: [''],
    quantity: [null], 
    unit: ['']
  });

  onIngredientSelect() {
    const ingredientId = this.form.value.ingredientId;
    if (ingredientId) {
      const numericId = Number(ingredientId);
      
      const ingredient = this.allIngredients().find(ing => ing.id === numericId);
      if (ingredient) {
        this.form.patchValue({
          ingredientName: ingredient.name,
          unit: ingredient.unit
        });
      }
    }
  }


  ngOnInit() {
    this.loadIngredients();
    this.loadUserIngredients();
  }

  loadIngredients() {
    this.ingredientService.getAllIngredients().subscribe({
      next: list => {
        this.allIngredients.set(list);
      },
      error: () => this.errorMessage.set('Failed to load ingredients')
    })
  }

  loadUserIngredients() {
  this.userIngredientService.getUserIngredients().subscribe({
    next: list => {
      this.userIngredients.set(list);
      this.userIngredientService.setUserIngredients(list);
    },
    error: () => this.errorMessage.set('Nie udało się załadować składników użytkownika')
  });
}



  onSubmit() {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    if (this.form.invalid) {
      this.errorMessage.set('Formularz jest niepoprawny.');
      return;
    }
    const { ingredientId, ingredientName, quantity, unit } = this.form.value;
    const numericIngredientId = Number(ingredientId);
    
    if (!quantity || !unit) {
      this.errorMessage.set('Ilość i jednostka są wymagane.');
      return;
    }
    
    const ing = this.allIngredients().find(i => i.id === numericIngredientId);
    if (!ing) {
      this.errorMessage.set('Nie znaleziono składnika o podanym ID.');
      return;
    }
    const dto: userIngredientCreateDto = {
      ingredientId: numericIngredientId,
      ingredientName: ing.name,
      quantity: quantity,
      unit: unit
    };
    this.userIngredientService.addUserIngredient(dto)
    .pipe(finalize(() => this.isSubmitting.set(false)))
    .subscribe({
      next: () => {
        this.successMessage.set('Składnik dodany pomyślnie');
        this.loadUserIngredients(); // odświeżamy lokalny i serwisowy sygnał
        this.form.reset();
      },
      error: err => this.errorMessage.set(err.userMessage || 'Błąd serwera')
    });

  }

  addUserIngredient(dto: userIngredientCreateDto) {
    this.isSubmitting.set(true);

    this.userIngredientService.addUserIngredient(dto)
    .pipe(
      
      finalize(() => this.isSubmitting.set(false))
    )
    .subscribe({
      next: () => {
        this.successMessage.set('Składnik dodany ');
        this.loadUserIngredients();
        this.form.reset(); 
      },
      error: (err) => {
        this.errorMessage.set(err.userMessage || 'Wystąpił błąd serwera. Spróbuj ponownie później.');
      }
    });
  }

  deleteUserIngredient(ingredientId: number) {
    this.userIngredientService.deleteUserIngredient(ingredientId).subscribe({
      next: success => {
        if (success) {
          this.successMessage.set('Składnik usunięty pomyślnie');
          this.loadUserIngredients();
        } else {
          this.errorMessage.set('Nie udało się usunąć składnika');
        }
      },
      error: (err) => this.errorMessage.set(err.userMessage || 'Błąd podczas usuwania składnika')
    });
  }

  startEditing(ingredientId: number, currentQuantity: number) {
    this.editingIngredientId.set(ingredientId);
    this.editQuantity.set(currentQuantity);
  }

  cancelEditing() {
    this.editingIngredientId.set(null);
    this.editQuantity.set(0);
  }

  saveEdit(ingredientId: number) {
    this.userIngredientService.updateUserIngredient(ingredientId, this.editQuantity()).subscribe({
      next: success => {
        if (success) {
          this.successMessage.set('Ilość zaktualizowana pomyślnie');
          this.loadUserIngredients();
          this.cancelEditing();
        } else {
          this.errorMessage.set('Nie udało się zaktualizować ilości');
        }
      },
      error: (err) => {
        console.error('Update error:', err);
        this.errorMessage.set('Błąd podczas aktualizacji ilości: ' + (err.error?.message || err.message));
      }
    });
  }

}

