import { RecipeDto } from "../../recipes/models/recipe-dto.model";

export interface PagedResult<T> {
    items: RecipeDto[];
    totalPages: number;
    itemFrom: number;
    itemTo: number;
    totalItemsCount: number;
}