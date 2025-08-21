using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeBook.Integration.Tests.Helpers
{   /// <summary> Base class for isolated test
    public abstract class IsolatedTestBase : IAsyncLifetime
    {
        protected IntegrationTestWebAppFactory Factory { get; private set; }
        protected HttpClient Client { get; private set; }
        
        public async Task InitializeAsync()
        {
            Factory = new IntegrationTestWebAppFactory();
            await Factory.InitializeAsync();
            Client = Factory.CreateClient();
        }
        public async Task DisposeAsync()
        {
            await Factory.DisposeAsync();
        }
    }
}
