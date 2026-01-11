using Application.DTO.UserIngredient;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Mappings;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    
    public class UserIngredientService : IUserIngredientService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;

        public UserIngredientService(AppDbContext context, IUserContextService userContextService)
        {
             _context = context;
            _userContextService = userContextService;
        }

        public async Task<List<UserIngredientDto>> GetAllAsync()
        {
            var userId = _userContextService.GetUserId();
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User not logged in.");
            }

            var userIngredients =  await _context.UserIngredients
                .AsNoTracking()
                .Where(ui => ui.UserId == userId)
                .Include(ui => ui.Ingredient)
                .ToListAsync();
                  
            var userIngredientsDto = UserIngredientMapper.MapToDtoList(userIngredients);
            return userIngredientsDto;
        }

        public async Task<UserIngredientDto> GetIngredientById(int ingredientId)
        {
            var userId = _userContextService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User not logged in.");
            }
            var ingredient = await _context.UserIngredients
                .AsNoTracking()
                .Include(ui => ui.Ingredient)
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IngredientId == ingredientId);
            if (ingredient == null)
            {
                throw new ValidationException("Ingredient not found.");
            }
            var ingredientDto = UserIngredientMapper.MapToDto(ingredient);
            return ingredientDto;
        }

        public async Task<UserIngredient> UpdateAsync(int ingredientId, UserIngredientUpdateDto updatedUserIngredient)
        {
            
            var userId = _userContextService.GetUserId;
            if(userId is null)
            {
                throw new UnauthorizedException("User not found");
            }
            var userIngredient = await _context.UserIngredients
                .Include(ui => ui.Ingredient)
                .FirstOrDefaultAsync(ui => ui.UserId.Equals(userId) && ui.IngredientId == ingredientId);

            if (userIngredient == null)
            {
                throw new ValidationException("User ingredient is empty");
            }

            userIngredient.Quantity = updatedUserIngredient.Quantity;

            await _context.SaveChangesAsync();
            return userIngredient;
        }

        public async Task<bool> DeleteAsync(int ingredientId)
        {
            var userId = _userContextService.GetUserId();
            if (userId is null)
            {
                throw new UnauthorizedException("User not found");
            }
            var userIngredient = await _context.UserIngredients
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IngredientId == ingredientId);
            if (userIngredient == null) { return false; }

            _context.UserIngredients.Remove(userIngredient);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateAsync(UserIngredientCreateDto userIngredientCreateDto)
        {
            var userId = _userContextService.GetUserId();
            if (userId == null)
            {
                throw new UnauthorizedException("User not logged in.");
            }

            var ingredient = await _context.Ingredients.FirstOrDefaultAsync( i => i.Id == userIngredientCreateDto.IngredientId );
            if ( ingredient == null )
            {
                throw new ValidationException("Ingredient not exist");
            }
            
            var alreadyExist = await _context.UserIngredients
                .AnyAsync(ui=>ui.UserId == userId && ui.IngredientId == ingredient.Id);
            if (alreadyExist)
            {
                throw new ValidationException("You already have this product.");
            }

            var userIngredient = UserIngredientMapper.MapToEntity(userIngredientCreateDto, userId);
            _context.UserIngredients.Add(userIngredient);      
            await _context.SaveChangesAsync();           
            return true;
        }
    }
}
