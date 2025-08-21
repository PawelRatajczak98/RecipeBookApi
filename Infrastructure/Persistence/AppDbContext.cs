using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<UserIngredient> UserIngredients { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Category> Categories { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Budget)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Budget)
                .HasDefaultValue(0m);

            modelBuilder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            modelBuilder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
            
            modelBuilder.Entity<Ingredient>()
                .Property(i => i.PriceFor100Grams)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<RecipeIngredient>()
                .Property(ui => ui.Quantity)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId);

            modelBuilder.Entity<UserIngredient>()
                .HasKey(ui => new { ui.UserId, ui.IngredientId });
            
            modelBuilder.Entity<UserIngredient>()
                .Property(ui => ui.Quantity)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<UserIngredient>()
                .HasOne(ui=>ui.User)
                .WithMany(u=>u.UserIngredients)
                .HasForeignKey(ui => ui.UserId);

            modelBuilder.Entity<UserIngredient>()
                .HasOne(ui => ui.Ingredient)
                .WithMany(i=>i.UserIngredients)
                .HasForeignKey(ui => ui.IngredientId);

            modelBuilder.Entity<Comment>()
                .HasKey(c => new { c.RecipeId, c.UserId });

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Recipe)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.RecipeId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.RecipeId, l.UserId });

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Recipe)
                .WithMany(r => r.Likes)
                .HasForeignKey(l => l.RecipeId);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId);
            
            modelBuilder.Entity<Recipe>()
                .Property(r => r.TotalCost)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);


            modelBuilder.Entity<Rating>()
                .HasKey(ra => new { ra.UserId, ra.RecipeId });

            modelBuilder.Entity<Rating>()
                .HasOne(ra => ra.Recipe)
                .WithMany(r => r.Ratings)
                .HasForeignKey(ra => ra.RecipeId);

            modelBuilder.Entity<Rating>()
                .HasOne(ra => ra.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(ra => ra.UserId);

        }
        public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
        {
            public AppDbContext CreateDbContext(string[] args)
            {              
                var basePath = Directory.GetCurrentDirectory();

                var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlServer(
                    configuration.GetConnectionString("DockerConnection"),
                    x => x.MigrationsAssembly("Infrastructure"));
                return new AppDbContext(optionsBuilder.Options);
            }
        }
    }
}
