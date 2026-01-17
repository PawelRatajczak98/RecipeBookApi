using Application.DTO.UserIngredient;
using Application.Utilities;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mappings
{
    public class UserIngredientMapper
    {
        public static UserIngredientDto MapToDto(UserIngredient userIngredient)
        {
            if (userIngredient == null) return null;
            return new UserIngredientDto
            {
                IngredientId = userIngredient.IngredientId,
                Quantity = userIngredient.Quantity,
                IngredientName = userIngredient.Ingredient?.Name,
                TotalPrice = userIngredient.Ingredient != null
                    ? IngredientPriceCalculator.CalculatePrice(userIngredient.Ingredient, userIngredient.Quantity, userIngredient.Unit)
                    : 0,
                Unit = userIngredient.Unit
            };
        }

        public static List<UserIngredientDto> MapToDtoList(IEnumerable<UserIngredient> userIngredients)
        {
            if (userIngredients == null) return new List<UserIngredientDto>();
            return userIngredients.Select(MapToDto).ToList();
        }


        public static UserIngredient MapToEntity(UserIngredientCreateDto dto, string UserId)
        {
            if (dto == null) return null;
            return new UserIngredient
            {
                IngredientId = dto.IngredientId,
                Quantity = dto.Quantity,
                UserId = UserId,
                Unit= dto.Unit,
                IngredientName = dto.IngredientName
            };
        }
    }
}
