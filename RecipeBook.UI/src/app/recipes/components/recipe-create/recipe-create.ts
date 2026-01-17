import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RecipeService } from '../../services/recipe.service';
import { CommonModule } from '@angular/common';
import { RecipeCreateDtoModel } from '../../models/recipe-create-dto.model';
import { IngredientService } from '../../../ingredients/services/ingredient.service';
import { Ingredient } from '../../../ingredients/models/ingredient.model';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';



@Component({
  selector: 'app-recipe-create',
  imports: [
    CommonModule, 
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatIconModule,
    MatDividerModule
  ],
  templateUrl: './recipe-create.html',
  styleUrl: './recipe-create.css'
})
export class RecipeCreate implements OnInit {
  private recipeService = inject(RecipeService);
  private ingredientService = inject(IngredientService);

  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  ingredientsList = signal<Ingredient[]>([]);

  name = '';
  description = '';
  instructions = '';
  preparationTime = 1;
  cookingTime = 1;
  servings = 1;

  selectedFile: File | null = null;
  imagePreviewUrl: string | null = null;
  fileInputError = signal<string | null>(null);

  ingredients: Array<{
    ingredientId: number;
    ingredientName: string;
    quantity: number;
    unit: string;
  }> = [];

  ngOnInit() {
    this.ingredientService.getAllIngredients().subscribe({
      next: list => this.ingredientsList.set(list),
      error: err => console.error('Błąd ładowania składników', err)
    });
    
    this.addIngredient();
  }

  addIngredient() {
    this.ingredients.push({
      ingredientId: 0,
      ingredientName: '',
      quantity: 0,
      unit: ''
    });
  }

  removeIngredient(index: number) {
    this.ingredients.splice(index, 1);
  }

  onIngredientSelect(index: number, value: any) {
    if (!value || value === 0) return;

    const ingredientId = Number(value);
    const ingredient = this.ingredientsList().find(p => p.id === ingredientId);

    if (ingredient) {
      this.ingredients[index].ingredientId = ingredient.id;
      this.ingredients[index].ingredientName = ingredient.name;
      this.ingredients[index].unit = ingredient.unit;
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileInputError.set(null);

    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        this.fileInputError.set('Nieprawidłowy typ pliku. Dozwolone: JPG, PNG, GIF, WEBP');
        this.selectedFile = null;
        this.imagePreviewUrl = null;
        return;
      }

      if (file.size > 5 * 1024 * 1024) {
        this.fileInputError.set('Plik jest za duży. Maksymalny rozmiar: 5MB');
        this.selectedFile = null;
        this.imagePreviewUrl = null;
        return;
      }

      this.selectedFile = file;

      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreviewUrl = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage(): void {
    this.selectedFile = null;
    this.imagePreviewUrl = null;
    this.fileInputError.set(null);
  }

  private minutesToTimeSpan(minutes: number): string {
    const h = Math.floor(minutes / 60).toString().padStart(2, '0');
    const m = (minutes % 60).toString().padStart(2, '0');
    return `${h}:${m}:00`;
  }

  onSubmit() {
    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const dto: RecipeCreateDtoModel = {
      name: this.name,
      description: this.description,
      instructions: this.instructions,
      preparationTime: this.minutesToTimeSpan(this.preparationTime),
      cookingTime: this.minutesToTimeSpan(this.cookingTime),
      servings: this.servings,
      recipeIngredientsDto: this.ingredients.filter(ing => ing.ingredientId > 0)
    };

    this.recipeService.createRecipe(dto, this.selectedFile || undefined).subscribe({
      next: (result) => {
        this.isSubmitting.set(false);
        this.successMessage.set('Przepis został dodany pomyślnie!');
        this.resetForm();
      },
      error: err => {
        console.error('Error creating recipe:', err);
        this.isSubmitting.set(false);
        this.errorMessage.set('Błąd przy tworzeniu przepisu.');
      }
    });
  }

  resetForm() {
    this.name = '';
    this.description = '';
    this.instructions = '';
    this.preparationTime = 1;
    this.cookingTime = 1;
    this.servings = 1;
    this.ingredients = [];
    this.selectedFile = null;
    this.imagePreviewUrl = null;
    this.fileInputError.set(null);
    this.addIngredient();
  }
}