import {RecipeIngredientDto} from '../../recipeingredients/models/recipeingredient-dto.model';
import {CommentDto} from '../../comments/models/comment-dto.model';
import {LikeDto} from '../../likes/models/like-dto.model';

export interface RecipeDto {
    id : number;
    name : string;
    description : string;
    recipeIngredients: RecipeIngredientDto[];
    comments: CommentDto[];
    likes: LikeDto[];
    totalCost: number;
}