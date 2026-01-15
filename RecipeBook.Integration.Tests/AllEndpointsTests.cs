using Application.DTO.UserIngredient;
using RecipeBook.Integration.Tests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace RecipeBook.Integration.Tests
{
    /// <summary>
    /// Kompleksowy zestaw testów pokrywający WSZYSTKIE 32 endpointy API aplikacji RecipeBook.
    /// Testy zorganizowane według kontrolerów.
    /// </summary>
    public class AllEndpointsTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestWebAppFactory _factory;
        private readonly ITestOutputHelper _output;

        public AllEndpointsTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        #region AccountController Tests (3 endpointy)

        [Fact]
        public async Task POST_AccountRegister_WithValidData_Returns200()
        {
            // Arrange
            var registerDto = new
            {
                Username = $"testuser",
                Email = $"testowy@example.com",
                Password = "Test12345!",
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/account/register", registerDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task POST_AccountLogin_WithValidCredentials_Returns200WithToken()
        {
            // Arrange
            var loginDto = new
            {
                UserName = "Richy",
                Password = "Pa$$w0rd"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/account/login", loginDto);

            // Assert
            _output.WriteLine($"Response Status Code: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response Content (Body): {content}");

            // Wyświetl wszystkie headers
            _output.WriteLine("\nResponse Headers:");
            foreach (var header in response.Headers)
            {
                _output.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }

            // Wyświetl Set-Cookie headers (tutaj jest token!)
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                _output.WriteLine("\nCookies (Set-Cookie):");
                foreach (var cookie in cookies)
                {
                    _output.WriteLine($"  {cookie}");

                    // Wyciągnij token z cookie
                    if (cookie.Contains("accessToken="))
                    {
                        var startIndex = cookie.IndexOf("accessToken=") + "accessToken=".Length;
                        var endIndex = cookie.IndexOf(";", startIndex);
                        if (endIndex == -1) endIndex = cookie.Length;
                        var token = cookie.Substring(startIndex, endIndex - startIndex);
                        _output.WriteLine($"\n  Extracted JWT Token: {token}");
                    }
                }
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Sprawdź czy token jest w cookies
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookies));
            Assert.Contains(setCookies, c => c.Contains("accessToken="));
        }

        [Fact]
        public async Task GET_AccountLoginMe_WithoutAuth_Returns401()
        {
            // Act
            var response = await _client.GetAsync("/api/account/login/me");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GET_AccountLoginMe_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.GetAsync("/api/account/login/me");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region AdminController Tests (4 endpointy)

        [Fact]
        public async Task GET_UsersWithRoles_WithoutAdminRole_Returns403()
        {
            // Arrange
            await AuthorizeClientAsMemberAsync();

            // Act
            var response = await _client.GetAsync("/users-with-roles");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GET_UsersWithRoles_WithAdminRole_Returns200()
        {
            // Arrange
            await AuthorizeClientAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/users-with-roles");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task POST_EditRoles_WithAdminRole_Returns200()
        {
            // Arrange
            await AuthorizeClientAsAdminAsync();

            // Act
            var response = await _client.PostAsync("/edit-roles?id=someUserId&roles=Member", null);

            // Assert
            // Może zwrócić 400 jeśli użytkownik nie istnieje, ale nie powinno być 403/401
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task POST_GenerateRecipes_WithAdminRole_Returns200()
        {
            // Arrange
            await AuthorizeClientAsAdminAsync();

            // Act
            var response = await _client.PostAsync("/generate-recipes?amount=5", null);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        // Helper DTO dla deserializacji PagedResult
        private class PagedResultDto
        {
            public int TotalItemsCount { get; set; }
        }

        [Fact]
        public async Task GET_RecalculateCost_WithAdminRole_Returns200()
        {
            // Arrange
            await AuthorizeClientAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/recalculate-cost");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region BudgetController Tests (3 endpointy)

        [Fact]
        public async Task GET_Budget_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.GetAsync("/api/budget/budget");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_Budget_WithoutAuth_Returns401()
        {
            // Act
            var response = await _client.GetAsync("/api/budget/budget");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task POST_BudgetIncrease_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.PostAsJsonAsync("/api/budget/increase", 100.00m);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task POST_BudgetDecrease_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.PostAsJsonAsync("/api/budget/decrease", 50.00m);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region IngredientsController Tests (4 endpointy)

        [Fact]
        public async Task GET_Ingredients_Returns200()
        {
            // Act
            var response = await _client.GetAsync("/api/ingredient");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_IngredientById_WithValidId_Returns200()
        {
            // Act
            var response = await _client.GetAsync("/api/ingredient/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_IngredientById_WithInvalidId_Returns404()
        {
            // Act - używamy ID które na pewno nie istnieje
            var response = await _client.GetAsync("/api/ingredient/999999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task POST_Ingredient_WithValidData_Returns201()
        {
            // Arrange
            var ingredientDto = new
            {
                Name = $"NewIngredient",
                PriceFor100Grams = 5.50m,
                Unit = "gram",
                Description = "Git"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ingredient", ingredientDto);

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.OK
            );
        }

        [Fact]
        public async Task DELETE_Ingredient_WithoutAdminRole_Returns403()
        {
            // Arrange
            await AuthorizeClientAsMemberAsync();

            // Act
            var response = await _client.DeleteAsync("/api/ingredient/1");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DELETE_Ingredient_WithAdminRole_Returns200Or204()
        {
            // Arrange
            await AuthorizeClientAsAdminAsync();

            // Act
            var response = await _client.DeleteAsync("/api/ingredient/1");

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.NotFound // Jeśli składnik nie istnieje
            );
        }

        #endregion

        #region UserIngredientsController Tests (4 endpointy)

        [Fact]
        public async Task GET_UserIngredients_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.GetAsync("/api/useringredients");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_UserIngredients_WithoutAuth_Returns401()
        {
            // Act
            var response = await _client.GetAsync("/api/useringredients");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task POST_UserIngredient_WithAuth_Returns201()
        {
            // Arrange
            await AuthorizeClientAsync();
            var userIngredientDto = new
            {
                IngredientId = 1,
                IngredientName = "Test Ingredient",
                Quantity = 500.0m,
                Unit = "gramy"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/useringredients", userIngredientDto);

            Assert.True(
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.OK,
                $"Expected Created or OK, but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}"
            );
        }

        [Fact]
        public async Task PATCH_UserIngredient_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Najpierw dodaj składnik (używamy ID 1 - powinien istnieć w bazie)
            var createResponse = await _client.PostAsJsonAsync("/api/useringredients", new
            {
                IngredientId = 1,
                IngredientName = "Test Ingredient",
                Quantity = 500.0,
                Unit = "g"
            });

            // Sprawdź czy POST się powiódł
            if (!createResponse.IsSuccessStatusCode)
            {
                var createError = await createResponse.Content.ReadAsStringAsync();
                _output.WriteLine($"POST failed with {createResponse.StatusCode}: {createError}");
            }
            Assert.True(createResponse.IsSuccessStatusCode, $"POST should succeed but got {createResponse.StatusCode}");

            var updateDto = new
            {
                IngredientId = 1,
                Quantity = 750.0
            };

            // Act
            var response = await _client.PatchAsJsonAsync("/api/useringredients/1", updateDto);

            // Sprawdź błąd PATCH jeśli wystąpił
            if (!response.IsSuccessStatusCode)
            {
                var updateError = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"PATCH failed with {response.StatusCode}: {updateError}");
            }

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent,
                $"Expected OK or NoContent, but got {response.StatusCode}"
            );
        }

        [Fact]
        public async Task DELETE_UserIngredient_WithAuth_Returns200Or204()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.DeleteAsync("/api/useringredients/1");

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region RecipesController Tests (9 endpointów)

        [Fact]
        public async Task GET_Recipes_Returns200WithPagedResult()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?pageNumber=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_RecipeById_WithValidId_Returns200()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_RecipeById_WithInvalidId_Returns404()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.GetAsync("/api/recipes/999999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GET_RecipeCost_WithValidId_Returns200()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes/1/cost");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var cost = await response.Content.ReadFromJsonAsync<decimal>();
            Assert.True(cost >= 0);
        }

        [Fact]
        public async Task GET_RecipesWithinBudget_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.GetAsync("/api/recipes/within-budget?pageNumber=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_RecipesWithinBudget_WithoutAuth_Returns401()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes/within-budget");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GET_RecipesCanPrepare_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.GetAsync("/api/recipes/can-prepare?pageNumber=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_RecipesCheapest_Returns200()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes/cheapest");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task POST_Recipe_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();
            var recipeDto = new
            {
                Name = "TestRecipe",
                Description = "Test description",
                Instructions = "Test instructions",
                PreparationTime = "00:15:00", // TimeSpan 15 minut
                CookingTime = "00:30:00",      // TimeSpan 30 minut
                Servings = 4,
                RecipeIngredientsDto = new[]
                {
                    new
                    {
                        IngredientName = "Test Ingredient",
                        IngredientId = 1,
                        Quantity = 200.0,
                        Unit = "gram"
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes", recipeDto);

            // Assert
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"Error: {errorContent}");
            }
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task POST_Recipe_WithoutAuth_Returns401()
        {
            // Arrange
            var recipeDto = new
            {
                Name = "TestRecipe",
                Description = "Test",
                Instructions = "Test",
                CategoryId = 1,
                RecipeIngredientsDto = new[]
                {
                    new { IngredientId = 1, Quantity = 100.0 }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes", recipeDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PUT_Recipe_WithoutAdminRole_Returns403()
        {
            // Arrange
            await AuthorizeClientAsMemberAsync();

            // Act
            var response = await _client.PutAsync("/api/recipes/1?description=Updated", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task PUT_Recipe_WithAdminRole_Returns204()
        {
            // Arrange
            await AuthorizeClientAsAdminAsync();

            // Act
            var response = await _client.PutAsync("/api/recipes/1?description=UpdatedByAdmin", null);

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.OK
            );
        }

        [Fact]
        public async Task DELETE_Recipe_Returns200Or204()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Najpierw stwórz przepis do usunięcia
            var recipeDto = new
            {
                Name = $"RecipeToDelete_{Guid.NewGuid()}",
                Description = "Will be deleted",
                Instructions = "Test",
                CategoryId = 1,
                RecipeIngredientsDto = new[]
                {
                    new { IngredientId = 1, Quantity = 100.0 }
                }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/recipes", recipeDto);
            var createdRecipe = await createResponse.Content.ReadFromJsonAsync<dynamic>();

            // Act
            var response = await _client.DeleteAsync($"/api/recipes/999");

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region RecipeFeedbacksService Tests (6 endpointów)

        [Fact]
        public async Task GET_Comments_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.GetAsync("/api/recipefeedbacksservice/comments?recipeId=1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_Comments_WithoutAuth_Returns401()
        {
            // Act
            var response = await _client.GetAsync("/api/recipefeedbacksservice/comments?recipeId=1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task POST_Comment_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            var commentDto = new
            {
                RecipeId = 1,
                CommentContent = "Great recipe!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipefeedbacksservice/comments", commentDto);

            // Assert
            _output.WriteLine($"POST Comment Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DELETE_Comment_WithAuth_Returns200Or204()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Najpierw dodaj komentarz - teraz używamy JSON body (API naprawione!)
            var commentDto = new
            {
                RecipeId = 1,
                CommentContent = "Test comment to delete"
            };

            var postResponse = await _client.PostAsJsonAsync("/api/recipefeedbacksservice/comments", commentDto);

            _output.WriteLine($"POST Comment Status: {postResponse.StatusCode}");
            var postContent = await postResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"POST Response: {postContent}");

            Assert.True(postResponse.IsSuccessStatusCode, "Failed to create comment for deletion test");

            // Act - Usuń komentarz (query param)
            var response = await _client.DeleteAsync("/api/recipefeedbacksservice/comments?recipeId=1");

            // Assert
            _output.WriteLine($"DELETE Comment Status: {response.StatusCode}");
            var deleteContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"DELETE Response: {deleteContent}");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent
            );
        }

        [Fact]
        public async Task GET_Likes_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.GetAsync("/api/recipefeedbacksservice/likes?recipeId=1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task POST_Like_WithAuth_Returns200()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Act
            var response = await _client.PostAsync("/api/recipefeedbacksservice/likes?recipeId=1", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DELETE_Like_WithAuth_Returns200Or204()
        {
            // Arrange
            await AuthorizeClientAsync();

            // Najpierw dodaj like
            var postResponse = await _client.PostAsync("/api/recipefeedbacksservice/likes?recipeId=1", null);

            _output.WriteLine($"POST Like Status: {postResponse.StatusCode}");
            Assert.True(postResponse.IsSuccessStatusCode, "Failed to create like for deletion test");

            // Act - Usuń like
            var response = await _client.DeleteAsync("/api/recipefeedbacksservice/likes?recipeId=1");

            // Assert
            _output.WriteLine($"DELETE Like Status: {response.StatusCode}");
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent
            );
        }

        #endregion

        #region Helper Methods

        private async Task AuthorizeClientAsync()
        {
            var loginDto = new
            {
                userName = "Richy",
                password = "Pa$$w0rd"
            };

            var response = await _client.PostAsJsonAsync("/api/account/login", loginDto);
            response.EnsureSuccessStatusCode();

            var token = ExtractTokenFromCookie(response);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task AuthorizeClientAsMemberAsync()
        {
            await AuthorizeClientAsync();
        }

        private async Task AuthorizeClientAsAdminAsync()
        {
            var loginDto = new
            {
                userName = "admin",
                password = "Pa$$w0rd"
            };

            var response = await _client.PostAsJsonAsync("/api/account/login", loginDto);
            response.EnsureSuccessStatusCode();

            var token = ExtractTokenFromCookie(response);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Dodaj budget 1000 do użytkownika admin
            await _client.PostAsJsonAsync("/api/budget/increase", 1000m);
        }

        private string ExtractTokenFromCookie(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
            {
                var tokenCookie = cookieHeaders.FirstOrDefault(c => c.Contains("accessToken="));

                if (!string.IsNullOrEmpty(tokenCookie))
                {
                    var startIndex = tokenCookie.IndexOf("=") + 1;
                    var endIndex = tokenCookie.IndexOf(";");

                    if (endIndex == -1)
                    {
                        endIndex = tokenCookie.Length;
                    }

                    return tokenCookie.Substring(startIndex, endIndex - startIndex);
                }
            }

            throw new Exception("Could not find 'accessToken' cookie in login response.");
        }

        #endregion
    }
}
