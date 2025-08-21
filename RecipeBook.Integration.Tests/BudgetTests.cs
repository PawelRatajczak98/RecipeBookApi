using Azure;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Integration.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Testcontainers;
using Testcontainers.MsSql;
using Xunit;

namespace RecipeBook.Integration.Tests
{
    public class BudgetTests : IsolatedTestBase
    {
        [Fact]
        public async Task GetBudget_ShouldReturn401_WhenUserNoAuthenticated()
        {
            //Arrange
            var response = await Client.GetAsync("/api/Budget/budget");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetBudget_ShouldReturnOk_WhenUserAuthenticated()
        {
            //Arrange
           await TestAuthHelper.AuthorizeClientAsync(Client);

            //Act
            var response = await Client.GetAsync("/api/Budget/budget");

            //
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetBudget_ShouldReturnValue_WhenUserAuthenticated() 
        {
            //Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);

            //Act
            var response = await Client.GetAsync("/api/Budget/budget");
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiValue = await response.Content.ReadFromJsonAsync<decimal>();
            //Assert

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(apiValue);
            Assert.True(apiValue > 0);
        }

        [Fact]
        public async Task GetBudget_ReturnCorrectValueFromDatabase()
        {
            //Arrange - Richy default budget is 500 on create by seeder          

            await TestAuthHelper.AuthorizeClientAsync(Client);

            decimal expectedBudget = 500m;
            decimal actualBudgetFromDb;

            var connectionString = Factory.ConnectionStringForTests;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT Budget FROM AspNetUsers WHERE UserName = 'Richy'", connection))
                {
                    var result = await command.ExecuteScalarAsync();    
                    Assert.NotNull(result);
                    actualBudgetFromDb = Convert.ToDecimal(result);
                }
            }

            var response = await Client.GetAsync("/api/Budget/budget");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiValue = await response.Content.ReadFromJsonAsync<decimal>();

            
            Assert.Equal(expectedBudget, actualBudgetFromDb);
            Assert.Equal(expectedBudget, apiValue);
        }

        [Fact]
        public async Task IncreaseBudget_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var payload = new { amount = 50m };

            // Act
            var response = await Client.PostAsJsonAsync("/api/budget/increase", payload);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task IncreaseBudget_ReturnsOk_WhenUserAuthenticated()
        {
            // Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);
            var payload = 100m;

            // Act
            var response = await Client.PostAsJsonAsync("/api/Budget/increase", payload);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task IncreaseBudget_ReturnsOk_And_CorrectValue_WhenUserAuthenticated()
        {
            //Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);
            var payload = 100m;
            var valueBefore = await Client.GetAsync("/api/Budget/budget/");
            bool correctDiff = false;
            decimal diff = 0;

            //Act
            var response = await Client.PostAsJsonAsync("/api/Budget/increase", payload);
            var valueAfterIncrease = await Client.GetAsync("/api/Budget/budget/");

            diff = (await valueAfterIncrease.Content.ReadFromJsonAsync<decimal>()) - (await valueBefore.Content.ReadFromJsonAsync<decimal>());
            if (diff == payload)
            {
                correctDiff = true;
            }

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(correctDiff);
        }

        [Fact]
        public async Task DecreaseBudget_ReturnsOk_WhenUserAuthenticated()
        {
            // Arrange
            await TestAuthHelper.AuthorizeClientAsync(Client);
            var payload = 50m;
            // Act
            var response = await Client.PostAsJsonAsync("/api/Budget/decrease", payload);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
