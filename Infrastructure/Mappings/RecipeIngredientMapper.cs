using Application.DTO.RecipeIngredient;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public class RecipeIngredientMapper
    {
        public static RecipeIngredientDto EntityToDto(RecipeIngredient recipeIngredient)
        {
            if (recipeIngredient == null) return null;
            return new RecipeIngredientDto
            { 
                IngredientId = recipeIngredient.IngredientId,
                IngredientName = recipeIngredient.Ingredient.Name,
                Quantity = recipeIngredient.Quantity,
                Unit = recipeIngredient.Unit
            };
           
        }
        
        
    }
}
