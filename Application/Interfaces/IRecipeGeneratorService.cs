using Application.DTO.Recipe;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRecipeGeneratorService
    {
        Task<bool> GenerateAndSaveAsync(int count, CancellationToken cancellationToken);
        RecipeCreateDto HelperForGeneration(int number, Dictionary<int,string> allIngredients);
        Task<bool> RecalculateAllRecipesAdminAsync();
    }
}
