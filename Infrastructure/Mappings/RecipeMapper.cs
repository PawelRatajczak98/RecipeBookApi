using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTO.Recipe;
using Application.DTO.RecipeIngredient;
using Domain.Entities;

namespace Infrastructure.Mappings
{
    public class RecipeMapper
    {
        public static Recipe DtoToEntity(RecipeCreateDto dto, List<Ingredient> ingredientsFromDb)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Recipe data cannot be null.");

            return new Recipe
            {
                Name = dto.Name,
                Description = dto.Description,
                Categories = ingredientsFromDb
                .SelectMany( i => i.Categories)
                .DistinctBy( c => c.Name)
                .OrderBy( c => c.Name)
                .ToList(),

                Instructions = dto.Instructions,
                PreparationTime = dto.PreparationTime,
                CookingTime = dto.CookingTime,
                TotalTime = dto.PreparationTime + dto.CookingTime,
                Servings = dto.Servings,

                TotalCost = dto.RecipeIngredientsDto.Sum(ri => 
                    ingredientsFromDb.FirstOrDefault(i => i.Id == ri.IngredientId)?.PriceFor100Grams * ri.Quantity / 100 ?? 0),
                RecipeIngredients = dto.RecipeIngredientsDto.Select(ri =>
                {
                    var ingredient = ingredientsFromDb.FirstOrDefault(i => i.Id == ri.IngredientId);
                    if (ingredient == null)
                        throw new Exception($"Ingredient with ID {ri.IngredientId} not found");
                   
                    return new RecipeIngredient
                    {
                        Ingredient = ingredient,
                        Quantity = ri.Quantity,
                        Unit = ri.Unit
                    };
                }).ToList()
            };
        }

        public static RecipeDto EntityToDto(Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe), "Recipe cannot be null.");
            return new RecipeDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                TotalCost = recipe.TotalCost,
                LikesCount = recipe.Likes?.Count ?? 0,
                CommentsCount = recipe.Comments?.Count ?? 0,
                RecipeIngredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto
                {
                    IngredientId = ri.Ingredient.Id,
                    IngredientName = ri.Ingredient.Name,
                    Quantity = ri.Quantity,
                    Unit = ri.Unit
                }).ToList()
            };
        }
    }
}
