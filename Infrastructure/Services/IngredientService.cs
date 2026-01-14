using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Application.Exceptions;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Application.DTO.Ingredient;


namespace Infrastructure.Services
{
    
    public class IngredientService : IIngredientService
    {
        private readonly AppDbContext _context;

        public IngredientService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<IEnumerable<Ingredient>> GetAllAsync()
        {
            var ingredients = await _context.Ingredients.ToListAsync();
            if (ingredients.Count == 0)
            {
                throw new ValidationException("Brak składników w bazie danych");
            }

            return ingredients;
        }

        public async Task<Ingredient> GetByIdAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if(ingredient == null)
            {
                throw new NotFoundException("Nie znaleziono takiego składniku");
            }
            return ingredient;
        }

        public async Task<Ingredient> CreateAsync(IngredientCreateDto dto)
        {
            if (dto == null)
            {
                throw new ValidationException("Składnik jest pusty");
            }
            if (_context.Ingredients.Any(i => i.Name == dto.Name))
            {
                throw new ValidationException("Składnik już istnieje w bazie danych");
            }
            var ingredient = new Ingredient
            {
                Name = dto.Name,
                Description = dto.Description,
                PriceFor100Grams = dto.PriceFor100Grams,
                Unit = "gramy"
            };
            await _context.Ingredients.AddAsync(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<Ingredient> UpdateAsync(int id, Ingredient updatedIngredient)
        {
            var existingIngredient = await _context.Ingredients.FindAsync(id);
            if (existingIngredient == null)
            {
                throw new ValidationException("Nie znaleziono takiego składniku");
            }
            existingIngredient.Description = updatedIngredient.Description;
            existingIngredient.PriceFor100Grams = updatedIngredient.PriceFor100Grams;
            await _context.SaveChangesAsync();
            return existingIngredient;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Ingredients.FindAsync(id);
            if (entity == null)
            {
                throw new ValidationException("Nie znaleziono takiego składniku");
            }
            _context.Ingredients.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

    
    }
}
