using RecipeBook.Integration.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace RecipeBook.Integration.Tests
{
    public class SharedContainerRecipeTest : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly IntegrationTestWebAppFactory _factory;
        private readonly HttpClient Client;
        private readonly ITestOutputHelper output;
        public SharedContainerRecipeTest(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            Client = _factory.CreateClient();
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(GetRangeFrom1To87))]
        public async Task GetRecipeCost_EveryoneCounted_ToFindAnomalies(int recipeId)
        {
            // Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);
            output.WriteLine($"Testing recipe with ID: {recipeId}");
            // Act
            var response = await Client.GetAsync($"/api/Recipes/{recipeId}/cost");
            var json = await response.Content.ReadAsStringAsync();
            output.WriteLine(json);
            var recipeCost = JsonSerializer.Deserialize<decimal>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            output.WriteLine($"Recipe ID: {recipeId}, Cost: {recipeCost}");
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static IEnumerable<object[]> GetRangeFrom1To87()
        {
            for (int i = 1; i <= 87; i++)
            {
                yield return new object[] { i };
            }
        }
    }
}
