import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs';
import { RecipeDto } from '../../models/recipe-dto.model';
import { RecipeService } from '../../services/recipe.service';
import { RecipeFeedbackComponent } from '../../../shared/components/recipe-feedback/recipe-feedback.component';

@Component({
  selector: 'app-recipe-details',
  imports: [CommonModule, RecipeFeedbackComponent],
  templateUrl: './recipe-details.html',
  styleUrl: './recipe-details.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RecipeDetails {
  private route = inject(ActivatedRoute);
  private recipeService = inject(RecipeService);

  recipe = toSignal(
    this.route.params.pipe(
      switchMap(params => this.recipeService.getRecipeById(+params['id']))
    )
  );
}