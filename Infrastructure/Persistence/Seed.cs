using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence;

    public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync()) return;
        
        var users = new List<AppUser>
        {
            new() {UserName = "Bob", Budget = 10},
            new() {UserName = "Michael", Budget = 5},
            new() {UserName = "LeBron", Budget = 20},
        };

        var roles = new List<AppRole>
        {
            new() {Name = "Member"},
            new() {Name = "Admin"},
            new() {Name = "Moderator"},
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        foreach (var user in users)
        {
            await userManager.CreateAsync(user, "Pa$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin = new AppUser { UserName = "admin" };
        
        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);

        var richUser = new AppUser { Id = "b97940b4-3a52-4a0b-8e2b-f28a3a0e1a1a", UserName = "Richy",Budget = 7};
        await userManager.CreateAsync(richUser, "Pa$$w0rd");
        await userManager.AddToRoleAsync(richUser, "Member");
    }

    public static async Task SeedData(DbContext context)
    {
        var connection = context.Database.GetDbConnection();
        
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT COUNT(*) FROM Ingredients";
        var count = (int?)await checkCommand.ExecuteScalarAsync();

        if (count.HasValue && count.Value > 0)
        {
            return;
        }

        var basePath = AppContext.BaseDirectory;

        var path = Path.Combine(basePath, "Persistence", "SeedDataInserts.sql");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Script file not found at {path}");
        
        var sqlScript = await File.ReadAllTextAsync(path);

        await using var command = connection.CreateCommand();
        command.CommandText = sqlScript;

        await command.ExecuteNonQueryAsync();
    }
}
