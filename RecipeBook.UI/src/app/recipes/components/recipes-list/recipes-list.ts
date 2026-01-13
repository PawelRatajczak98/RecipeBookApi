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

function emptyPagedResult<T>(): PagedResult<T> {
  return { items: [], totalPages: 0, itemFrom: 0, itemTo: 0, totalItemsCount: 0 };
}

type RecipesMode = { type: 'all' } | { type: 'budget' } | { type: 'canPrepare' };

@Component({
  selector: 'app-recipes-list',
  imports: [CommonModule, RouterLink],
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
    pageSize: 10,
    searchPhrase: '',
  });

  mode = signal<RecipesMode>({ type: 'all' });

  currentBudget = computed(() => this.userIngredientService.getCurrentBudget());

  customBudget = signal<number | null>(null);
  minBudget = signal<number>(0); // domyślnie 0, ustawimy na backendzie

  constructor() {
    // efekt reagujący na zalogowanie/wylogowanie
    effect(() => {
      const user = this.userInfoService.currentUser();
      if (user) {
        this.userIngredientService.refreshUserIngredients();
      } else {
        this.userIngredientService.userIngredients.set([]);
      }
    });

    // przy inicjalizacji komponentu pobierz najtańszy przepis i ustaw minBudget
    this.loadMinBudget();
  }

  private async loadMinBudget() {
    try {
      const cheapest = await this.recipeService.getCheapestRecipeCost().toPromise();
      this.minBudget.set(cheapest ?? 0);
    } catch {
      this.minBudget.set(0.01); // minimalna awaryjna wartość
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
        if (mode.type === 'all') {
          return this.recipeService.getRecipes(query);
        }

        if (mode.type === 'budget') {
          // jeśli budżet mniejszy niż minBudget → zwróć pustą stronę
          if ((budget ?? 0) < this.minBudget()) return of(emptyPagedResult<RecipeDto>());

          return this.recipeService.getRecipesWithinBudget(budget ?? 0, query);
        }

        if (mode.type === 'canPrepare') {
          if (!userId) return of(emptyPagedResult<RecipeDto>());
          return this.recipeService.getRecipesUserCanPrepare(userId, query);
        }

        return this.recipeService.getRecipes(query);
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
    const parsed = parseFloat(value);
    if (isNaN(parsed)) {
      this.customBudget.set(null);
    } else {
      const rounded = Math.round(parsed * 100) / 100;
      this.customBudget.set(rounded);
    }

    if (this.mode().type === 'budget') {
      this.query.update(q => ({ ...q, pageNumber: 1 }));
    }
  }


  setMinBudget(value: number) {
    this.minBudget.set(value);
  }
}
