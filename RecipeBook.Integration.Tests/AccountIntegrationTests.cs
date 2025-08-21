using Microsoft.Extensions.Logging;
using RecipeBook.Integration.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace RecipeBook.Integration.Tests
{
    public class AccountIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly IntegrationTestWebAppFactory _factory;
        private readonly ITestOutputHelper _output;
        
        public AccountIntegrationTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
        {
            _httpClient = factory.CreateClient();
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task Login_ShouldReturnJwtToken()
        {

            var jwtToken = await TestAuthHelper.LoginUserAndRetrieveTokenAsync(_httpClient);

            _output.WriteLine($"JWT: {jwtToken}");

            Assert.False(string.IsNullOrEmpty(jwtToken));
            Assert.Contains(".", jwtToken);
        }
  
        [Fact]
        public async Task Login_ShouldAccessJwtToken()
        {
           // Arrange         
            var jwt = await TestAuthHelper.LoginUserAndRetrieveTokenAsync(_httpClient);
            _output.WriteLine($"Retrieved JWT: {jwt}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _httpClient.GetAsync("/api/Account/validateJwt");

            // Assert
            _output.WriteLine($"Response Status Code: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response Content: {content}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("valid", content, StringComparison.OrdinalIgnoreCase);
        }
    }
}
