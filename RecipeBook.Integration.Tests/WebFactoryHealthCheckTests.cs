using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeBook.Integration.Tests
{
    public class WebFactoryHealthCheckTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly IntegrationTestWebAppFactory _factory;

        public WebFactoryHealthCheckTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateClient();
        }

        [Fact]
        public async Task WebFactory_StartsSuccessfullyAndReturnsOk()
        {
            var response = await _httpClient.GetAsync("/api/recipes");
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content));
        }

        [Fact]
        public async Task DatabaseContainer_IsReachableAndCanConnect()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var canConnect = await dbContext.Database.CanConnectAsync();
                Assert.True(canConnect);
            }
        }
    }
}
