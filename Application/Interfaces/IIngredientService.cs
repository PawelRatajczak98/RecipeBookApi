using Application.DTO.Ingredient;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IIngredientService
    {
        Task<IEnumerable<Ingredient>> GetAllAsync();
        Task<Ingredient> GetByIdAsync(int id);
        Task<Ingredient> CreateAsync(IngredientCreateDto dto);
        Task<Ingredient> UpdateAsync(int id, Ingredient ingredient);
        Task<bool> DeleteAsync(int id);
    }
}
