using Application;
using Application.DTO.Recipe;
using Application.Query;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IRecipeService 
    {
        Task<PagedResult<RecipeDto>> GetAllAsync(RecipeQuery recipeQuery);
        Task<RecipeDto> GetByIdAsync(int id);
        Task<bool> CreateAsync(RecipeCreateDto recipeCreateDto, string? imageUrl = null);
        Task<bool> UpdateAsync(int id, string description);
        Task<bool> DeleteAsync(int id);
        Task<decimal> CalculateRecipeCostAsync(int recipeId);
        Task<PagedResult<RecipeDto>> GetRecipesWithinBudget(RecipeQuery recipeQuery);
        Task<PagedResult<RecipeDto>> GetRecipesUserCanPrepareAsync(RecipeQuery recipeQuery);
        Task<decimal> GetCheapestRecipeCostAsync();
    }
}
