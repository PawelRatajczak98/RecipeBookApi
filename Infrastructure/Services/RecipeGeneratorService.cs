using Application.DTO.Recipe;
using Application.DTO.RecipeIngredient;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RecipeGeneratorService : IRecipeGeneratorService
    {
        private readonly AppDbContext _dbContext;
        private readonly Random _random;
        private readonly IRecipeService _recipeService;
        private static int saveCounter = 0;
        private readonly List<Ingredient> _allIngredients;

        public RecipeGeneratorService(AppDbContext dbContext, Random random, IRecipeService recipeService)
        {
            _dbContext = dbContext;
            _random = random;
            _recipeService = recipeService;
            _allIngredients = _dbContext.Ingredients.AsNoTracking().ToList();
        }


        public async Task<bool> GenerateAndSaveAsync(int count, CancellationToken cancellationToken)
        {
            int lastReportedPercent = 0;
            int reportStep = 10;

            if (_allIngredients.Count == 0)
            {
                throw new ValidationException("Składniki nie zostały załadowane");
            }
            var ingredientDict = _allIngredients.ToDictionary(i => i.Id, i => i.Name);

            var recipeCreateDtosToSave = new List<RecipeCreateDto>();


            for (int i = 1; i <= count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var dto = HelperForGeneration(i, ingredientDict);
                if (dto == null)
                {
                    throw new NotFoundException("Dto is empty");
                }

                recipeCreateDtosToSave.Add(dto);

                int currentPercent = (i * 100) / count;
                if(currentPercent >= lastReportedPercent + reportStep)
                {
                    Console.WriteLine($"Generated {i}/{count} recipes ({currentPercent}%)");
                    lastReportedPercent = currentPercent - (currentPercent % reportStep);
                }
            }



            foreach (var dto in recipeCreateDtosToSave)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _recipeService.CreateAsync(dto);

                int currentSaveCounter = Interlocked.Increment(ref saveCounter);
                if (currentSaveCounter % 1000 == 0)
                {
                    Console.WriteLine($"Zapisano: {currentSaveCounter}");
                }
            }
            return true;
        }


        public RecipeCreateDto HelperForGeneration(int number, Dictionary<int, string> ingredientDict)
        {       
            string Name = $"TestowyPrzepisNumer{number}";
            string Description = $"TestowyPrzepisOpisNumer{number}";

            int quantityOfUniqueProducts = _random.Next(1, 6);

            var availableIds = ingredientDict.Keys.ToList();
            
            var selectedIds = new HashSet<int>();

            while (selectedIds.Count < quantityOfUniqueProducts)
            {
                int randomId = availableIds.ElementAt(_random.Next(availableIds.Count));
                selectedIds.Add(randomId);
            }

            Dictionary<int,string> ingredientIdAndName = selectedIds.ToDictionary(id => id, id => ingredientDict[id]);

            if (ingredientIdAndName.Count == 0)
            {
                throw new ValidationException("Słownik pusty");
            }
            
            var recipeIngredientsDto = ingredientIdAndName
                .Select(entry => new RecipeIngredientDto
                {
                    IngredientId = entry.Key,
                    IngredientName = entry.Value,
                    Quantity = Math.Round((decimal)(_random.Next(100, 900) + 1), 2),
                    Unit = "gramy"
                })
                .ToList();


            var recipeCreateDtoToSaveInDb = new RecipeCreateDto
            {
                Name = Name,
                Description = Description,
                RecipeIngredientsDto = recipeIngredientsDto,
                Instructions = "Instructions Test",
                PreparationTime = new TimeSpan(0,15,0),
                CookingTime = new TimeSpan (0,30,0),
                Servings = 4
            };



            return recipeCreateDtoToSaveInDb;
        }

        public async Task<bool> RecalculateAllRecipesAdminAsync()
        {
            string sql = @"
            UPDATE Recipes
            SET TotalCost = (
            SELECT SUM(RI.Quantity * I.PriceFor100Grams / 100.0)
            FROM RecipeIngredients AS RI
            JOIN Ingredients AS I ON RI.IngredientId = I.IngredientId
            WHERE RI.RecipeId = Recipes.IngredientId
            );";
            await _dbContext.Database.ExecuteSqlRawAsync(sql);
            return true;
        }


    }
}
