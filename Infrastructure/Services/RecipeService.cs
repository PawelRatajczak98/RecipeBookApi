using Application;
using Application.DTO.Recipe;
using Application.DTO.RecipeIngredient;
using Application.Exceptions;
using Application.Query;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Mappings;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;

        public RecipeService(AppDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<PagedResult<RecipeDto>> GetAllAsync(RecipeQuery query)
        {
            var baseQuery = _context.Recipes
                .AsNoTracking()
                .Where(r => query.SearchPhrase == null || r.Name.ToLower().Contains(query.SearchPhrase.ToLower()))
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient);

            var recipes = await baseQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .ToListAsync();

            var totalItemsCount = await baseQuery.CountAsync();

            if (recipes == null || !recipes.Any())
            {
                throw new ValidationException("No recipes found in the database.");
            }

            var recipesDtos = recipes.Select(RecipeMapper.EntityToDto).ToList();
            return new PagedResult<RecipeDto>(recipesDtos, totalItemsCount, query.PageSize, query.PageNumber);
        }

        public async Task<RecipeDto> GetByIdAsync(int id)
        {
            var recipe = await _context.Recipes
                .AsNoTracking()
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .Include (r => r.Likes)
                .Include (r => r.Comments)
                .SingleOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                throw new NotFoundException("Recipe not found");
            }

            return RecipeMapper.EntityToDto(recipe);
        }

        public async Task<bool> CreateAsync(RecipeCreateDto recipeCreateDto, string? imageUrl = null)
        {
            if (recipeCreateDto.RecipeIngredientsDto == null || recipeCreateDto.RecipeIngredientsDto.Count == 0)
                throw new ValidationException("Recipe must contain at least one ingredient.");

            var userName = _userContextService.GetUsername();
            if (string.IsNullOrEmpty(userName))
                throw new UnauthorizedException("User not logged in.");

            var ingredientIds = recipeCreateDto.RecipeIngredientsDto
                .Select(ri => ri.IngredientId)
                .Distinct()
                .ToList();

            var ingredients = await _context.Ingredients
                .Where(i => ingredientIds.Contains(i.Id))
                .ToListAsync();

            if (ingredients.Count != ingredientIds.Count)
            {
                var missingIngredientIds = ingredientIds.Except(ingredients.Select(i => i.Id)).ToList();
                throw new ValidationException($"Ingredients not found: {string.Join(", ", missingIngredientIds)}");
            }

            var recipe = RecipeMapper.DtoToEntity(recipeCreateDto, ingredients);
            recipe.AuthorUserName = userName;
            recipe.ImageUrl = imageUrl;

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(int id, string description)
        {
            var existingRecipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (existingRecipe == null)
                throw new NotFoundException("Not found");

            existingRecipe.Description = description;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
                return false;

            _context.Recipes.Remove(recipe);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<decimal> CalculateRecipeCostAsync(int recipeId)
        {
            var totalCost = await _context.RecipeIngredients
                .Include(ri => ri.Ingredient)
                .Where(ri => ri.RecipeId == recipeId)
                .SumAsync(ri => ri.Ingredient.PriceFor100Grams * ((decimal)ri.Quantity / 100));

            return totalCost;
        }

        public async Task<PagedResult<RecipeDto>> GetRecipesWithinBudget(RecipeQuery query)
        {
            var userDto = await _userContextService.GetUserDto();
            if (userDto == null)
                throw new ValidationException("User not found");

            decimal budgetToUse = query.Budget ?? userDto.Budget ?? 0m;

            var baseQuery = _context.Recipes
                .AsNoTracking()
                .Where(r => r.TotalCost <= budgetToUse)
                .Where(r => query.SearchPhrase == null || r.Name.ToLower().Contains(query.SearchPhrase.ToLower()))
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient);

            var totalRecipesCount = await baseQuery.CountAsync();

            var pagedRecipes = await baseQuery
                .OrderBy(r => r.Id)
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .Select(recipe => RecipeMapper.EntityToDto(recipe))
                .ToListAsync();

            if (!pagedRecipes.Any())
                throw new ValidationException("No recipes found within budget");

            return new PagedResult<RecipeDto>(pagedRecipes, totalRecipesCount, query.PageSize, query.PageNumber);
        }

        public async Task<PagedResult<RecipeDto>> GetRecipesUserCanPrepareAsync(RecipeQuery query)
        {
            var userId = _userContextService.GetUserId();
            
            if (userId == null) throw new ValidationException("Brak użytkownika");

            var matchingRecipeIds = await _context.RecipeIngredients
                .GroupJoin(
                    _context.UserIngredients.Where(ui => ui.UserId == userId),
                    ri => ri.IngredientId,
                    ui => ui.IngredientId,
                    (ri, uis) => new { ri, uis }
                )
                .SelectMany(
                    x => x.uis.DefaultIfEmpty(),
                    (x, ui) => new { x.ri, ui }
                )
                .GroupBy(x => x.ri.RecipeId)
                .Select(g => new
                {
                    RecipeId = g.Key,
                    TotalIngredients = g.Count(),
                    MatchedIngredients = g.Count(x => x.ui != null && x.ui.Quantity >= x.ri.Quantity)
                })
                .Where(x => x.TotalIngredients == x.MatchedIngredients)
                .Select(x => x.RecipeId)
                .ToListAsync();

            if (!matchingRecipeIds.Any()) throw new ValidationException("Brak pasujących przepisów");


            var baseQuery = _context.Recipes
                .AsNoTracking()
                .Where(r => matchingRecipeIds.Contains(r.Id))
                .Where(r => query.SearchPhrase == null || r.Name.ToLower().Contains(query.SearchPhrase.ToLower()))
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient);

            var totalRecipesCount = await baseQuery.CountAsync();

            var pagedRecipes = await baseQuery
                .OrderBy(r => r.Id)
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .Select(recipe => RecipeMapper.EntityToDto(recipe))
                .ToListAsync();

            

            return new PagedResult<RecipeDto>(pagedRecipes, totalRecipesCount, query.PageSize, query.PageNumber);
        }

        public async Task<decimal> GetCheapestRecipeCostAsync()
        {
            var cheapest = await _context.Recipes
                .OrderBy(r => r.TotalCost)
                .Select(r => r.TotalCost)
                .FirstOrDefaultAsync();

            return Math.Round(cheapest, 2);

        }
    }
}
