import { Component, inject, ChangeDetectionStrategy, computed, signal, effect} from '@angular/core';
import { RecipeDto } from '../../models/recipe-dto.model';
import { RecipeQuery } from '../../query/recipe-query.model';
import { RecipeService } from '../../services/recipe.service';
import { UserInfoService } from '../../../user/services/user-info.service';
import { UserIngredientsService } from '../../../user-ingredients/services/useringredients.services';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs';
import { PagedResult } from '../../../paged-result/models/paged-result.model';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { RecipeFeedbackComponent } from '../../../shared/components/recipe-feedback/recipe-feedback.component';

function emptyPagedResult<T>(): PagedResult<T> {
  return { items: [], totalPages: 0, itemFrom: 0, itemTo: 0, totalItemsCount: 0 };
}

type RecipesMode = { type: 'all' } | { type: 'budget' } | { type: 'canPrepare' };

@Component({
  selector: 'app-recipes-list',
  imports: [CommonModule, RouterLink, RecipeFeedbackComponent],
  templateUrl: './recipes-list.html',
  styleUrls: ['./recipes-list.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecipesList {
  private recipeService = inject(RecipeService);
  public userInfoService = inject(UserInfoService);
  private userIngredientService = inject(UserIngredientsService);

  query = signal<RecipeQuery>({
    pageNumber: 1,
    pageSize: 12,
    searchPhrase: '',
  });

  mode = signal<RecipesMode>({ type: 'all' });

  currentBudget = computed(() => this.userIngredientService.getCurrentBudget());

  customBudget = signal<number | null>(null);
  minBudget = signal<number>(0);

  constructor() {
    effect(() => {
      const user = this.userInfoService.currentUser();
      if (user) {
        this.userIngredientService.refreshUserIngredients();
      } else {
        this.userIngredientService.userIngredients.set([]);
      }
    });

    this.loadMinBudget();
  }

  private async loadMinBudget() {
    try {
      const cheapest = await this.recipeService.getCheapestRecipeCost().toPromise();
      this.minBudget.set(cheapest ?? 0);
    } catch {
      this.minBudget.set(0.01);
    }
  }

recipesResult = toSignal(
  toObservable(
    computed(() => {
      const user = this.userInfoService.currentUser();
      const rawBudget = this.customBudget() ?? this.currentBudget();
      const budget = Math.round(rawBudget * 100) / 100;
      const userId = user?.userId;
      return { query: this.query(), mode: this.mode(), budget, userId };
    })
  ).pipe(
    switchMap(({ query, mode, budget, userId }) => {
      let request$;

      if (mode.type === 'all') {
        request$ = this.recipeService.getRecipes(query);
      } else if (mode.type === 'budget') {
        if ((budget ?? 0) < this.minBudget()) return of(emptyPagedResult<RecipeDto>());
        request$ = this.recipeService.getRecipesWithinBudget(budget ?? 0, query);
      } else if (mode.type === 'canPrepare') {
        if (!userId) return of(emptyPagedResult<RecipeDto>());
        request$ = this.recipeService.getRecipesUserCanPrepare(userId, query);
      } else {
        request$ = this.recipeService.getRecipes(query);
      }

      return request$.pipe(
        catchError(error => {
          console.warn('Backend zwrócił błąd (prawdopodobnie brak wyników):', error);
          return of(emptyPagedResult<RecipeDto>());
        })
      );
    })
  ),
  { initialValue: emptyPagedResult<RecipeDto>() }
);

  recipes = computed(() => this.recipesResult().items ?? []);
  totalCount = computed(() => this.recipesResult().totalItemsCount);
  totalPages = computed(() => Math.ceil(this.totalCount() / this.query().pageSize));
  pages = computed(() => Array.from({ length: this.totalPages() }, (_, i) => i + 1));

  onPageChange(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages()) {
      this.query.update(q => ({ ...q, pageNumber: newPage }));
    }
  }

  onSearchChange(searchPhrase: string): void {
    this.query.update(q => ({ ...q, searchPhrase, pageNumber: 1 }));
  }

  onPageSizeChange(pageSize: number): void {
    this.query.update(q => ({ ...q, pageSize, pageNumber: 1 }));
  }

  onShowAll(): void {
    this.mode.set({ type: 'all' });
    this.query.update(q => ({ ...q, pageNumber: 1 }));
  }

  onGetWithinBudget(): void {
    this.mode.set({ type: 'budget' });
    this.query.update(q => ({ ...q, pageNumber: 1 }));
  }

  onGetCanPrepare(): void {
    this.mode.set({ type: 'canPrepare' });
    this.query.update(q => ({ ...q, pageNumber: 1 }));
  }

  onCustomBudgetChange(value: string) {
  if (value === '') {
    this.customBudget.set(null);
  } else {
    const parsed = parseFloat(value);
    if (!isNaN(parsed)) {
      const rounded = Math.round(parsed * 100) / 100;
      this.customBudget.set(rounded);
    }
  }

  if (this.mode().type === 'budget') {
    this.query.update(q => ({ ...q, pageNumber: 1 }));
  }
}
}
