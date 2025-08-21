using Application.DTO.UserIngredient;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserIngredientService
    {
        Task<List<UserIngredientDto>> GetAllAsync();
        Task<UserIngredientDto> GetIngredientById(int id);
        Task<UserIngredient> UpdateAsync(int ingredientId,UserIngredientUpdateDto updatedUserIngredient);
        Task<bool> DeleteAsync(int id);
        Task<bool> CreateAsync(UserIngredientCreateDto userIngredientCreateDto);
    }
}
