import { CanActivateFn, Router, Routes } from '@angular/router';
import { Login } from './auth/components/login/login';
import { RecipesList } from './recipes/components/recipes-list/recipes-list';
import { RecipeCreate } from './recipes/components/recipe-create/recipe-create';
import { NotFound} from './shared/components/not-found/not-found';
import { UserInfoService } from './user/services/user-info.service';
import { inject } from '@angular/core';
import { UserIngredientDtoModel } from './user-ingredients/models/user-ingredient-dto.model';
import { RecipeDetails } from './recipes/components/recipe-details/recipe-details';
import { User } from './user/components/user/user';
import { UserIngredients } from './user-ingredients/user-ingredients/user-ingredients';

export const authGuard: CanActivateFn = () => {
  const user = inject(UserInfoService).currentUser();
  const router = inject(Router);
  return user ? true : router.createUrlTree(['/auth/login']);
};


export const loginRedirectGuard: CanActivateFn = () => {
  const user = inject(UserInfoService).currentUser();
  const router = inject(Router);
  return user ? router.createUrlTree(['/recipes']) : true;
};


export const routes: Routes = [
    {
        path: '',
        redirectTo: '/recipes',
        pathMatch: 'full'
    },
    {
        path: 'account/login',
        canActivate: [loginRedirectGuard],
        component: Login
    },
    {
    path: 'recipes',
    children: [
      {
        path: '',
        component: RecipesList,
      },
      {
        path: 'create',
        canActivate: [authGuard],
        component: RecipeCreate,
      },
      {
        path: ':id',
        component: RecipeDetails,
      },
    ],
  },
  {
    path: 'user/ingredients',
    canActivate: [authGuard],
    component: UserIngredients,
  },

  {
    path: 'user',
    outlet: 'side',
    component: User,
  },

  {
    path: '**',
    component: NotFound,
  },
    

];