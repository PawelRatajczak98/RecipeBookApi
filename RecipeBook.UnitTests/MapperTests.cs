using Application.DTO.Recipe;
using Application.DTO.RecipeIngredient;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Mappings;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla mapperów.
/// Weryfikują poprawność mapowania między DTO a encjami domenowymi.
/// </summary>
public class MapperTests
{
    #region RecipeMapper Tests

    [Fact]
    public void RecipeMapper_DtoToEntity_ShouldMapCorrectly()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Spaghetti Bolognese",
            Description = "Klasyczny włoski przepis z sosem mięsnym",
            Instructions = "1. Ugotuj makaron\n2. Przygotuj sos\n3. Wymieszaj i podawaj",
            PreparationTime = TimeSpan.FromMinutes(20),
            CookingTime = TimeSpan.FromMinutes(25),
            Servings = 4,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 500, Unit = "g" },
                new RecipeIngredientDto { IngredientId = 2, Quantity = 300, Unit = "g" },
                new RecipeIngredientDto { IngredientId = 3, Quantity = 200, Unit = "ml" }
            }
        };

        var ingredientsFromDb = new List<Ingredient>
        {
            new Ingredient { Id = 1, Name = "Makaron", PriceFor100Grams = 2.00m, Categories = new List<Category> { new Category { Id = 1, Name = "Makarony" } } },
            new Ingredient { Id = 2, Name = "Mięso mielone", PriceFor100Grams = 8.00m, Categories = new List<Category> { new Category { Id = 2, Name = "Mięso" } } },
            new Ingredient { Id = 3, Name = "Pomidory", PriceFor100Grams = 3.00m, Categories = new List<Category> { new Category { Id = 3, Name = "Warzywa" } } }
        };

        // Act
        var entity = RecipeMapper.DtoToEntity(dto, ingredientsFromDb);

        // Assert
        entity.Should().NotBeNull();
        entity.Name.Should().Be("Spaghetti Bolognese");
        entity.Description.Should().Be("Klasyczny włoski przepis z sosem mięsnym");
        entity.Instructions.Should().Be("1. Ugotuj makaron\n2. Przygotuj sos\n3. Wymieszaj i podawaj");
        entity.PreparationTime.Should().Be(TimeSpan.FromMinutes(20));
        entity.CookingTime.Should().Be(TimeSpan.FromMinutes(25));
        entity.TotalTime.Should().Be(TimeSpan.FromMinutes(45));
        entity.Servings.Should().Be(4);

        // Sprawdź kategorie
        entity.Categories.Should().HaveCount(3);
        entity.Categories.Select(c => c.Name).Should().Contain(new[] { "Makarony", "Mięso", "Warzywa" });

        // Sprawdź składniki
        entity.RecipeIngredients.Should().HaveCount(3);
        entity.RecipeIngredients.Should().Contain(ri => ri.Ingredient.Id == 1 && ri.Quantity == 500 && ri.Unit == "g");
        entity.RecipeIngredients.Should().Contain(ri => ri.Ingredient.Id == 2 && ri.Quantity == 300 && ri.Unit == "g");
        entity.RecipeIngredients.Should().Contain(ri => ri.Ingredient.Id == 3 && ri.Quantity == 200 && ri.Unit == "ml");

        // Sprawdź obliczony koszt (500g * 2.00/100g + 300g * 8.00/100g + 200ml * 3.00/100ml = 10 + 24 + 6 = 40)
        entity.TotalCost.Should().Be(40.00m);
    }

    [Fact]
    public void RecipeMapper_DtoToEntity_ShouldThrowException_WhenDtoIsNull()
    {
        // Arrange
        RecipeCreateDto dto = null;
        var ingredientsFromDb = new List<Ingredient>();

        // Act & Assert
        Action act = () => RecipeMapper.DtoToEntity(dto, ingredientsFromDb);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Recipe data cannot be null*");
    }

    [Fact]
    public void RecipeMapper_DtoToEntity_ShouldThrowException_WhenIngredientNotFound()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "Test",
            Instructions = "Test",
            PreparationTime = TimeSpan.FromMinutes(10),
            CookingTime = TimeSpan.FromMinutes(15),
            Servings = 2,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 999, Quantity = 100, Unit = "g" } // Nie istnieje
            }
        };

        var ingredientsFromDb = new List<Ingredient>();

        // Act & Assert
        Action act = () => RecipeMapper.DtoToEntity(dto, ingredientsFromDb);
        act.Should().Throw<Exception>()
            .WithMessage("Ingredient with ID 999 not found");
    }

    [Fact]
    public void RecipeMapper_DtoToEntity_ShouldHandleEmptyIngredientsList()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Simple Recipe",
            Description = "Prosty przepis bez składników",
            Instructions = "Instrukcje",
            PreparationTime = TimeSpan.FromMinutes(5),
            CookingTime = TimeSpan.FromMinutes(5),
            Servings = 1,
            RecipeIngredientsDto = new List<RecipeIngredientDto>()
        };

        var ingredientsFromDb = new List<Ingredient>();

        // Act
        var entity = RecipeMapper.DtoToEntity(dto, ingredientsFromDb);

        // Assert
        entity.Should().NotBeNull();
        entity.RecipeIngredients.Should().BeEmpty();
        entity.TotalCost.Should().Be(0);
        entity.Categories.Should().BeEmpty();
    }

    [Fact]
    public void RecipeMapper_DtoToEntity_ShouldRemoveDuplicateCategories()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "Test",
            Instructions = "Test",
            PreparationTime = TimeSpan.FromMinutes(10),
            CookingTime = TimeSpan.FromMinutes(10),
            Servings = 2,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 100, Unit = "g" },
                new RecipeIngredientDto { IngredientId = 2, Quantity = 200, Unit = "g" }
            }
        };

        var sharedCategory = new Category { Id = 1, Name = "Warzywa" };
        var ingredientsFromDb = new List<Ingredient>
        {
            new Ingredient { Id = 1, Name = "Pomidor", PriceFor100Grams = 3.00m, Categories = new List<Category> { sharedCategory } },
            new Ingredient { Id = 2, Name = "Ogórek", PriceFor100Grams = 2.50m, Categories = new List<Category> { sharedCategory } }
        };

        // Act
        var entity = RecipeMapper.DtoToEntity(dto, ingredientsFromDb);

        // Assert
        entity.Categories.Should().HaveCount(1);
        entity.Categories.First().Name.Should().Be("Warzywa");
    }

    [Fact]
    public void RecipeMapper_EntityToDto_ShouldMapCorrectly()
    {
        // Arrange
        var entity = new Recipe
        {
            Id = 42,
            Name = "Pancakes",
            Description = "Amerykańskie naleśniki",
            TotalCost = 12.50m,
            RecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    Ingredient = new Ingredient { Id = 10, Name = "Mąka" },
                    Quantity = 300,
                    Unit = "g"
                },
                new RecipeIngredient
                {
                    Ingredient = new Ingredient { Id = 11, Name = "Mleko" },
                    Quantity = 500,
                    Unit = "ml"
                }
            }
        };

        // Act
        var dto = RecipeMapper.EntityToDto(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(42);
        dto.Name.Should().Be("Pancakes");
        dto.Description.Should().Be("Amerykańskie naleśniki");
        dto.TotalCost.Should().Be(12.50m);

        dto.RecipeIngredients.Should().HaveCount(2);
        dto.RecipeIngredients.Should().Contain(i => i.IngredientId == 10 && i.IngredientName == "Mąka" && i.Quantity == 300 && i.Unit == "g");
        dto.RecipeIngredients.Should().Contain(i => i.IngredientId == 11 && i.IngredientName == "Mleko" && i.Quantity == 500 && i.Unit == "ml");
    }

    [Fact]
    public void RecipeMapper_EntityToDto_ShouldThrowException_WhenEntityIsNull()
    {
        // Arrange
        Recipe entity = null;

        // Act & Assert
        Action act = () => RecipeMapper.EntityToDto(entity);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Recipe cannot be null*");
    }

    [Fact]
    public void RecipeMapper_EntityToDto_ShouldHandleEmptyRecipeIngredients()
    {
        // Arrange
        var entity = new Recipe
        {
            Id = 1,
            Name = "Empty Recipe",
            Description = "Przepis bez składników",
            TotalCost = 0,
            RecipeIngredients = new List<RecipeIngredient>()
        };

        // Act
        var dto = RecipeMapper.EntityToDto(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Empty Recipe");
        dto.RecipeIngredients.Should().BeEmpty();
    }

    #endregion

    #region RecipeIngredientMapper Tests

    [Fact]
    public void RecipeIngredientMapper_EntityToDto_ShouldMapCorrectly()
    {
        // Arrange
        var recipeIngredient = new RecipeIngredient
        {
            RecipeId = 1,
            IngredientId = 5,
            Quantity = 250,
            Unit = "g",
            Ingredient = new Ingredient
            {
                Id = 5,
                Name = "Mąka",
                PriceFor100Grams = 2.00m
            }
        };

        // Act
        var dto = Application.Mappings.RecipeIngredientMapper.EntityToDto(recipeIngredient);

        // Assert
        dto.Should().NotBeNull();
        dto.IngredientId.Should().Be(5);
        dto.IngredientName.Should().Be("Mąka");
        dto.Quantity.Should().Be(250);
        dto.Unit.Should().Be("g");
    }

    [Fact]
    public void RecipeIngredientMapper_EntityToDto_ShouldReturnNull_WhenEntityIsNull()
    {
        // Arrange
        RecipeIngredient recipeIngredient = null;

        // Act
        var dto = Application.Mappings.RecipeIngredientMapper.EntityToDto(recipeIngredient);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void RecipeIngredientMapper_EntityToDto_ShouldMapWithDifferentUnits()
    {
        // Arrange
        var recipeIngredient = new RecipeIngredient
        {
            RecipeId = 2,
            IngredientId = 10,
            Quantity = 500,
            Unit = "ml",
            Ingredient = new Ingredient
            {
                Id = 10,
                Name = "Mleko",
                PriceFor100Grams = 1.50m
            }
        };

        // Act
        var dto = Application.Mappings.RecipeIngredientMapper.EntityToDto(recipeIngredient);

        // Assert
        dto.Should().NotBeNull();
        dto.IngredientId.Should().Be(10);
        dto.IngredientName.Should().Be("Mleko");
        dto.Quantity.Should().Be(500);
        dto.Unit.Should().Be("ml");
    }

    #endregion

    #region UserIngredientMapper Tests

    [Fact]
    public void UserIngredientMapper_MapToDto_ShouldMapCorrectly()
    {
        // Arrange
        var userIngredient = new UserIngredient
        {
            UserId = "user123",
            IngredientId = 5,
            IngredientName = "Mąka",
            Quantity = 1000,
            Unit = "g",
            Ingredient = new Ingredient
            {
                Id = 5,
                Name = "Mąka",
                PriceFor100Grams = 2.00m
            }
        };

        // Act
        var dto = Infrastructure.Mappings.UserIngredientMapper.MapToDto(userIngredient);

        // Assert
        dto.Should().NotBeNull();
        dto.IngredientId.Should().Be(5);
        dto.IngredientName.Should().Be("Mąka");
        dto.Quantity.Should().Be(1000);
        dto.Unit.Should().Be("g");
        // TotalPrice = 1000 * 2.00 / 100 = 20.00
        dto.TotalPrice.Should().Be(20.00m);
    }

    [Fact]
    public void UserIngredientMapper_MapToDto_ShouldCalculateTotalPrice_Correctly()
    {
        // Arrange
        var userIngredient = new UserIngredient
        {
            UserId = "user456",
            IngredientId = 10,
            IngredientName = "Cukier",
            Quantity = 500,
            Unit = "g",
            Ingredient = new Ingredient
            {
                Id = 10,
                Name = "Cukier",
                PriceFor100Grams = 3.50m
            }
        };

        // Act
        var dto = Infrastructure.Mappings.UserIngredientMapper.MapToDto(userIngredient);

        // Assert
        dto.Should().NotBeNull();
        // TotalPrice = 500 * 3.50 / 100 = 17.50
        dto.TotalPrice.Should().Be(17.50m);
    }

    [Fact]
    public void UserIngredientMapper_MapToDto_ShouldReturnZeroPrice_WhenIngredientIsNull()
    {
        // Arrange
        var userIngredient = new UserIngredient
        {
            UserId = "user789",
            IngredientId = 15,
            IngredientName = "Test Ingredient",
            Quantity = 300,
            Unit = "g",
            Ingredient = null // Null ingredient
        };

        // Act
        var dto = Infrastructure.Mappings.UserIngredientMapper.MapToDto(userIngredient);

        // Assert
        dto.Should().NotBeNull();
        dto.IngredientId.Should().Be(15);
        dto.Quantity.Should().Be(300);
        dto.TotalPrice.Should().Be(0);
        dto.IngredientName.Should().BeNull();
    }

    [Fact]
    public void UserIngredientMapper_MapToDto_ShouldReturnNull_WhenEntityIsNull()
    {
        // Arrange
        UserIngredient userIngredient = null;

        // Act
        var dto = Infrastructure.Mappings.UserIngredientMapper.MapToDto(userIngredient);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void UserIngredientMapper_MapToDtoList_ShouldMapMultipleItems()
    {
        // Arrange
        var userIngredients = new List<UserIngredient>
        {
            new UserIngredient
            {
                UserId = "user123",
                IngredientId = 1,
                IngredientName = "Mąka",
                Quantity = 500,
                Unit = "g",
                Ingredient = new Ingredient { Id = 1, Name = "Mąka", PriceFor100Grams = 2.00m }
            },
            new UserIngredient
            {
                UserId = "user123",
                IngredientId = 2,
                IngredientName = "Cukier",
                Quantity = 300,
                Unit = "g",
                Ingredient = new Ingredient { Id = 2, Name = "Cukier", PriceFor100Grams = 3.00m }
            },
            new UserIngredient
            {
                UserId = "user123",
                IngredientId = 3,
                IngredientName = "Mleko",
                Quantity = 1000,
                Unit = "ml",
                Ingredient = new Ingredient { Id = 3, Name = "Mleko", PriceFor100Grams = 1.50m }
            }
        };

        // Act
        var dtos = Infrastructure.Mappings.UserIngredientMapper.MapToDtoList(userIngredients);

        // Assert
        dtos.Should().NotBeNull();
        dtos.Should().HaveCount(3);
        dtos.Should().Contain(d => d.IngredientId == 1 && d.Quantity == 500 && d.TotalPrice == 10.00m);
        dtos.Should().Contain(d => d.IngredientId == 2 && d.Quantity == 300 && d.TotalPrice == 9.00m);
        dtos.Should().Contain(d => d.IngredientId == 3 && d.Quantity == 1000 && d.TotalPrice == 15.00m);
    }

    [Fact]
    public void UserIngredientMapper_MapToDtoList_ShouldReturnEmptyList_WhenInputIsNull()
    {
        // Arrange
        IEnumerable<UserIngredient> userIngredients = null;

        // Act
        var dtos = Infrastructure.Mappings.UserIngredientMapper.MapToDtoList(userIngredients);

        // Assert
        dtos.Should().NotBeNull();
        dtos.Should().BeEmpty();
    }

    [Fact]
    public void UserIngredientMapper_MapToDtoList_ShouldReturnEmptyList_WhenInputIsEmpty()
    {
        // Arrange
        var userIngredients = new List<UserIngredient>();

        // Act
        var dtos = Infrastructure.Mappings.UserIngredientMapper.MapToDtoList(userIngredients);

        // Assert
        dtos.Should().NotBeNull();
        dtos.Should().BeEmpty();
    }

    [Fact]
    public void UserIngredientMapper_MapToEntity_ShouldMapCorrectly()
    {
        // Arrange
        var dto = new Application.DTO.UserIngredient.UserIngredientCreateDto
        {
            IngredientId = 7,
            IngredientName = "Pomidory",
            Quantity = 400,
            Unit = "g"
        };
        var userId = "user999";

        // Act
        var entity = Infrastructure.Mappings.UserIngredientMapper.MapToEntity(dto, userId);

        // Assert
        entity.Should().NotBeNull();
        entity.IngredientId.Should().Be(7);
        entity.IngredientName.Should().Be("Pomidory");
        entity.Quantity.Should().Be(400);
        entity.Unit.Should().Be("g");
        entity.UserId.Should().Be("user999");
    }

    [Fact]
    public void UserIngredientMapper_MapToEntity_ShouldReturnNull_WhenDtoIsNull()
    {
        // Arrange
        Application.DTO.UserIngredient.UserIngredientCreateDto dto = null;
        var userId = "user123";

        // Act
        var entity = Infrastructure.Mappings.UserIngredientMapper.MapToEntity(dto, userId);

        // Assert
        entity.Should().BeNull();
    }

    [Fact]
    public void UserIngredientMapper_MapToEntity_ShouldMapWithDifferentUserId()
    {
        // Arrange
        var dto = new Application.DTO.UserIngredient.UserIngredientCreateDto
        {
            IngredientId = 12,
            IngredientName = "Ser",
            Quantity = 200,
            Unit = "g"
        };
        var userId = "admin-user-456";

        // Act
        var entity = Infrastructure.Mappings.UserIngredientMapper.MapToEntity(dto, userId);

        // Assert
        entity.Should().NotBeNull();
        entity.UserId.Should().Be("admin-user-456");
        entity.IngredientId.Should().Be(12);
        entity.IngredientName.Should().Be("Ser");
    }

    #endregion
}
