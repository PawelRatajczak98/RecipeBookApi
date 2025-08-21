import { Component,inject, ChangeDetectionStrategy,computed,signal } from '@angular/core';
import { RecipeDto } from '../../models/recipe-dto.model';
import { RecipeQuery } from '../../query/recipe-query.model';
import { RecipeService } from '../../services/recipe.service';
import { UserInfoService } from '../../../user/services/user-info.service';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs';   
import { PagedResult } from '../../../paged-result/models/paged-result.model';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

function emptyPagedResult<T>(): PagedResult<T> {
  return { items: [], totalPages: 0, itemFrom: 0, itemTo: 0, totalItemsCount: 0 };
}

type RecipesMode = { type: 'all' } | { type: 'budget' } | { type: 'canPrepare' };

@Component({
  selector: 'app-recipes-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './recipes-list.html',
  styleUrl: './recipes-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})

export class RecipesList {
  private recipeService = inject(RecipeService);
  userInfoService = inject(UserInfoService);

  private user = this.userInfoService.currentUser;

  query = signal<RecipeQuery>({
    pageNumber: 1,
    pageSize: 10,
    searchPhrase: '',
  });

  mode = signal<RecipesMode>({type: 'all'});
  customBudget = signal<number>(0);

  recipesResult = toSignal(
    toObservable(
      computed(() => ({
        query: this.query(),
        mode: this.mode(),
        budget: this.customBudget() > 0 ? this.customBudget() : this.userInfoService.currentUser()?.budget ?? 0,
        userId: this.userInfoService.currentUser()?.userId ?? "id"
      }))
    ).pipe(
      switchMap(({ query, mode, budget, userId }) => {
        console.log('Mode:', mode, 'Budget:', budget, 'UserId:', userId);
        if (mode.type === 'all') {
          return this.recipeService.getRecipes(query);
        } else if (mode.type === 'budget' && budget > 0) {
          return this.recipeService.getRecipesWithinBudget(budget, query);
        } else if (mode.type === 'canPrepare') {
          return this.recipeService.getRecipesUserCanPrepare(userId, query);
        } else{
          return this.recipeService.getRecipes(query);
        }
      })
    ),
    { initialValue: emptyPagedResult<RecipeDto>() } 
  );

  recipes = computed<RecipeDto[]>(() => {
    const result = this.recipesResult().items ?? [];
    return result;
  });
  totalCount = computed(() => this.recipesResult().totalItemsCount);
  totalPages = computed(() => {return Math.ceil(this.totalCount() / this.query().pageSize);});
  
  pages = computed(() => {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  });

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

  onCustomBudgetChange(budget: number): void {
    this.customBudget.set(budget);
    if (this.mode().type === 'budget') {
      this.query.update(q => ({ ...q, pageNumber: 1 }));
    }
  }

  onGetCanPrepare(): void {
    this.mode.set({ type: 'canPrepare' });
    this.query.update(q => ({ ...q, pageNumber: 1 }));
  }

  
}

