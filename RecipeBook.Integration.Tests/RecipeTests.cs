using RecipeBook.Integration.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using Xunit.Abstractions;
using System.Text.Json;
using Domain.Entities;

namespace RecipeBook.Integration.Tests
{
    public class RecipeTests : IsolatedTestBase
    {
        private readonly ITestOutputHelper output;
        public RecipeTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task GetRecipesAllAsync_ShouldReturnSuccess()
        {
            // Arrange
            // Act
            var response = await Client.GetAsync("/api/Recipes/");
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetRecipesAllAsync_ShouldReturnItems_WhenSucces()
        {
            //Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);

            // Act
            var response = await Client.GetAsync("/api/Recipes/");
            var statusCode = response.StatusCode;
            var responseBody = await response.Content.ReadAsStringAsync();
            
            var recipes = JsonSerializer.Deserialize<List<Application.DTO.Recipe.RecipeDto>>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            output.WriteLine($"Deserialized Recipes Count: {recipes?.Count}");

            //Assert
            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.NotNull(recipes);
        }

        [Fact]
        public async Task GetRecipesByIdAsync_ShouldReturnOnlySpecifiedRecipe()
        {
            //Act
            var response = await Client.GetAsync("/api/Recipes/5");
            var json = await response.Content.ReadAsStringAsync();
            output.WriteLine(json);

            var recipe = JsonSerializer.Deserialize<Application.DTO.Recipe.RecipeDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            /* 
            recipeDto doesn't give IngredientId anymore
            var idOfGivenRecipe = recipe?.IngredientId;
            */

            var nameOfGivenRecipe = recipe?.Name;
            //Assert
            Assert.NotNull(recipe);
            Assert.Equal("Zupa jarzynowa z soczewica", nameOfGivenRecipe);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(3)]
        [InlineData(23)]
        public async Task GetRecipeCostById_ShouldReturnCalculatedValue(int recipeId)
        {
            var response = await Client.GetAsync($"api/Recipes/{recipeId}/cost");

            response.EnsureSuccessStatusCode();
            var cost = await response.Content.ReadFromJsonAsync<decimal>();

            Assert.True(cost > 0m);
        }

        [Fact]
        public async Task GetRecipesWithinBudget_ShouldReturnRecipes_WhenUserAuthenticated()
        {
            // Arrange 
            await TestAuthHelper.AuthorizeClientAsync(Client);
            // Act
            var response = await Client.GetAsync("/api/Recipes/within-budget");
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var recipes = JsonSerializer.Deserialize<List<Application.DTO.Recipe.RecipeDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            output.WriteLine(json);

            Assert.NotNull(recipes);
            Assert.NotEmpty(recipes);
        }

        [Fact]
        public async Task GetRecipesUserCanPrepare_ShouldReturnRecipes_WhenUserAuthenticated()
        {
            // Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);
            // Act
            var response = await Client.GetAsync("/api/Recipes/can-prepare");
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var recipes = JsonSerializer.Deserialize<List<Application.DTO.Recipe.RecipeDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            output.WriteLine(json);
            Assert.NotNull(recipes);
            Assert.NotEmpty(recipes);
        }

        

        [Fact]
        public async Task PostRecipe_ShouldReturnTrue_WhenUserAuthenticated()
        {
            // Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);
            var recipeJson = new
            {
                name = "NewRecipe",
                description = "DescriptionIsRequired",
                recipeIngredientsDto = new[]
                {
                    new { ingredientName = "Masło", ingredientId = 4, quantity = 100, unit = "gram" }
                }
            };
            // Act
            var response = await Client.PostAsJsonAsync("/api/Recipes", recipeJson);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }

        [Fact]
        public async Task PostRecipe_ShouldReturn401_WhenUserNotAuthenticated()
        {
            // Arrange
            var recipeJson = new
            {
                name = "NewRecipe",
                description = "DescriptionIsRequired",
                recipeIngredients = new[]
                {
                    new { ingredientId = 1, quantity = 100, unit = "gram" }
                }
            };
            // Act
            var response = await Client.PostAsJsonAsync("/api/Recipes", recipeJson);
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostRecipe_ShouldReturnBadRequest_WhenRecipeNameIsEmpty()
        {
            // Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);
            var invalidRecipeJson = new
            {
                name = "",
                description = "DescriptionIsRequired",
                recipeIngredients = new[]
                {
                    new { ingredientId = 1, quantity = 100, unit = "gram" }
                }
            };
            // Act
            var response = await Client.PostAsJsonAsync("/api/Recipes", invalidRecipeJson);
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostRecipe_ShouldReturnBadRequest_WhenNoIngredientsProvided()
        {
            // Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);
            var invalidRecipeJson = new
            {
                name = "RecipeWithoutIngredients",
                description = "This recipe has no ingredients",
            };
            // Act
            var response = await Client.PostAsJsonAsync("/api/Recipes", invalidRecipeJson);
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}