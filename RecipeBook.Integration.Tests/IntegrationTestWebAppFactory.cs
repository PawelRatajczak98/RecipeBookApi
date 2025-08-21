using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace RecipeBook.Integration.Tests
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithCleanUp(true)
            .WithPortBinding(1433, true)
            .Build();

        public string ConnectionStringForTests { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var derscriptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<AppDbContext>));


                if (derscriptor is not null)
                {
                    services.Remove(derscriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(_dbContainer.GetConnectionString(), x => x.MigrationsAssembly("Infrastructure"));
                    ConnectionStringForTests = _dbContainer.GetConnectionString();
                });

                using var scope = services.BuildServiceProvider().CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Domain.Entities.AppUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Domain.Entities.AppRole>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
                Seed.SeedUsers(userManager, roleManager).GetAwaiter().GetResult();
                Seed.SeedData(dbContext).GetAwaiter().GetResult();
            });

        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

        }

        public async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }


    }
}