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
                userName = "Richy",
                password = "Pa$$w0rd"
            };

            var loginResponse = await client.PostAsJsonAsync("/api/Account/login", loginPayload);
            loginResponse.EnsureSuccessStatusCode();

            /*
            var result = await loginResponse.Content.ReadAsStringAsync();

            return result;
            */
            if (loginResponse.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
            {
                // Szukamy ciastka o nazwie "accessToken"
                var tokenCookie = cookieHeaders.FirstOrDefault(c => c.Contains("accessToken="));

                if (!string.IsNullOrEmpty(tokenCookie))
                {
                    // Format nagłówka to zazwyczaj: "accessToken=EYJ...; path=/; secure; HttpOnly"
                    // Musimy wyciąć samą wartość tokena (pomiędzy "=" a ";")

                    var startIndex = tokenCookie.IndexOf("=") + 1;
                    var endIndex = tokenCookie.IndexOf(";");

                    if (endIndex == -1) // Jeśli to ostatni parametr i nie ma średnika
                    {
                        endIndex = tokenCookie.Length;
                    }

                    var jwtToken = tokenCookie.Substring(startIndex, endIndex - startIndex);
                    return jwtToken;
                }
            }

            throw new Exception("Nie znaleziono ciasteczka 'accessToken' w odpowiedzi logowania.");
        
        }

        public static async Task AuthorizeClientAsync(HttpClient client)
        {
            var loginPayload = new
            {
                userName = "Richy",
                password = "Pa$$w0rd"
            };
            var response = await client.PostAsJsonAsync("/api/Account/login", loginPayload);
            response.EnsureSuccessStatusCode();

        }

        
    }
}
