using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace RecipeBook.Integration.Tests.Helpers
{
    public static class TestAuthHelper 
    {
        public static async Task<string> LoginUserAndRetrieveTokenAsync(HttpClient client)
        {
            ///Richy ma trochę produktów już w bazie, także jest na sztywno wklejony
            var loginPayload = new
            {
                username = "Richy",
                password = "Pa$$w0rd"
            };

            var loginResponse = await client.PostAsJsonAsync("/api/Account/loginSwagGiveJwt", loginPayload);
            loginResponse.EnsureSuccessStatusCode();

            var result = await loginResponse.Content.ReadAsStringAsync();

            return result;
        }

        public static async Task AuthorizeClientAsync(HttpClient client)
        {
            var jwt = await LoginUserAndRetrieveTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        
    }
}
