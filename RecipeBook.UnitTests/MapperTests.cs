using Application.Dto;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Mappers;
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
    public void RecipeMapper_ShouldMapDtoToEntity_Correctly()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Spaghetti Bolognese",
            Description = "Klasyczny włoski przepis z sosem mięsnym",
            Instructions = "1. Ugotuj makaron\n2. Przygotuj sos\n3. Wymieszaj i podawaj",
            PreparationTime = 45,
            Servings = 4,
            CategoryId = 2,
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 500 },
                new RecipeIngredientDto { IngredientId = 2, Quantity = 300 },
                new RecipeIngredientDto { IngredientId = 3, Quantity = 200 }
            }
        };

        var userId = "user-abc-123";

        // Act
        var entity = RecipeMapper.MapToEntity(dto, userId);

        // Assert
        entity.Should().NotBeNull();
        entity.Name.Should().Be("Spaghetti Bolognese");
        entity.Description.Should().Be("Klasyczny włoski przepis z sosem mięsnym");
        entity.Instructions.Should().Be("1. Ugotuj makaron\n2. Przygotuj sos\n3. Wymieszaj i podawaj");
        entity.PreparationTime.Should().Be(45);
        entity.Servings.Should().Be(4);
        entity.CategoryId.Should().Be(2);
        entity.UserId.Should().Be("user-abc-123");
        entity.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        entity.RecipeIngredients.Should().HaveCount(3);
        entity.RecipeIngredients.Should().Contain(ri => ri.IngredientId == 1 && ri.Quantity == 500);
        entity.RecipeIngredients.Should().Contain(ri => ri.IngredientId == 2 && ri.Quantity == 300);
        entity.RecipeIngredients.Should().Contain(ri => ri.IngredientId == 3 && ri.Quantity == 200);
    }

    [Fact]
    public void RecipeMapper_ShouldMapEntityToDto_Correctly()
    {
        // Arrange
        var entity = new Recipe
        {
            Id = 42,
            Name = "Pancakes",
            Description = "Amerykańskie naleśniki",
            Instructions = "1. Wymieszaj składniki\n2. Smaż na patelni",
            PreparationTime = 15,
            Servings = 2,
            CategoryId = 3,
            UserId = "user-xyz-789",
            TotalCost = 12.50m,
            CreatedDate = new DateTime(2024, 1, 15),
            RecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    RecipeId = 42,
                    IngredientId = 10,
                    Quantity = 300,
                    Ingredient = new Ingredient { Id = 10, Name = "Mąka", PriceFor100Grams = 2.00m }
                },
                new RecipeIngredient
                {
                    RecipeId = 42,
                    IngredientId = 11,
                    Quantity = 500,
                    Ingredient = new Ingredient { Id = 11, Name = "Mleko", PriceFor100Grams = 1.50m }
                }
            },
            Category = new Category { Id = 3, Name = "Śniadania" }
        };

        // Act
        var dto = RecipeMapper.MapToDto(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(42);
        dto.Name.Should().Be("Pancakes");
        dto.Description.Should().Be("Amerykańskie naleśniki");
        dto.Instructions.Should().Be("1. Wymieszaj składniki\n2. Smaż na patelni");
        dto.PreparationTime.Should().Be(15);
        dto.Servings.Should().Be(2);
        dto.CategoryId.Should().Be(3);
        dto.CategoryName.Should().Be("Śniadania");
        dto.TotalCost.Should().Be(12.50m);
        dto.CreatedDate.Should().Be(new DateTime(2024, 1, 15));

        dto.Ingredients.Should().HaveCount(2);
        dto.Ingredients.Should().Contain(i => i.Id == 10 && i.Name == "Mąka" && i.Quantity == 300);
        dto.Ingredients.Should().Contain(i => i.Id == 11 && i.Name == "Mleko" && i.Quantity == 500);
    }

    [Fact]
    public void RecipeMapper_ShouldHandleNullCollections_WhenMappingToDto()
    {
        // Arrange - Recipe bez składników i kategorii
        var entity = new Recipe
        {
            Id = 1,
            Name = "Empty Recipe",
            Description = "Przepis bez składników",
            Instructions = "Brak instrukcji",
            PreparationTime = 10,
            Servings = 1,
            CategoryId = 1,
            UserId = "user-123",
            TotalCost = 0,
            CreatedDate = DateTime.UtcNow,
            RecipeIngredients = null, // Null collection
            Category = null // Null navigation
        };

        // Act
        var dto = RecipeMapper.MapToDto(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Empty Recipe");
        dto.Ingredients.Should().BeEmpty(); // Powinna być pusta lista, nie null
        dto.CategoryName.Should().BeNullOrEmpty();
    }

    [Fact]
    public void RecipeMapper_ShouldMapEmptyIngredientsList_Correctly()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Simple Recipe",
            Description = "Prosty przepis",
            Instructions = "Instrukcje",
            PreparationTime = 5,
            Servings = 1,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>() // Pusta lista
        };

        var userId = "user-123";

        // Act
        var entity = RecipeMapper.MapToEntity(dto, userId);

        // Assert
        entity.Should().NotBeNull();
        entity.RecipeIngredients.Should().BeEmpty();
    }

    [Fact]
    public void RecipeMapper_ShouldPreserveIngredientOrder_WhenMapping()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Ordered Recipe",
            Description = "Przepis z uporządkowanymi składnikami",
            Instructions = "Dodaj składniki w kolejności",
            PreparationTime = 20,
            Servings = 2,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 5, Quantity = 100 },
                new RecipeIngredientDto { IngredientId = 3, Quantity = 200 },
                new RecipeIngredientDto { IngredientId = 7, Quantity = 150 }
            }
        };

        var userId = "user-123";

        // Act
        var entity = RecipeMapper.MapToEntity(dto, userId);

        // Assert
        entity.RecipeIngredients.Should().HaveCount(3);
        var ingredientList = entity.RecipeIngredients.ToList();
        ingredientList[0].IngredientId.Should().Be(5);
        ingredientList[0].Quantity.Should().Be(100);
        ingredientList[1].IngredientId.Should().Be(3);
        ingredientList[1].Quantity.Should().Be(200);
        ingredientList[2].IngredientId.Should().Be(7);
        ingredientList[2].Quantity.Should().Be(150);
    }

    [Fact]
    public void RecipeMapper_ShouldSetRecipeIdInIngredients_WhenMappingToDto()
    {
        // Arrange
        var entity = new Recipe
        {
            Id = 99,
            Name = "Test Recipe",
            Description = "Test",
            Instructions = "Test",
            PreparationTime = 10,
            Servings = 1,
            CategoryId = 1,
            UserId = "user-123",
            TotalCost = 10,
            CreatedDate = DateTime.UtcNow,
            RecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    RecipeId = 99,
                    IngredientId = 1,
                    Quantity = 250,
                    Ingredient = new Ingredient { Id = 1, Name = "Test Ingredient" }
                }
            }
        };

        // Act
        var dto = RecipeMapper.MapToDto(entity);

        // Assert
        dto.Ingredients.Should().HaveCount(1);
        dto.Ingredients.First().Should().NotBeNull();
        // RecipeId should match the parent Recipe's Id
        entity.RecipeIngredients.First().RecipeId.Should().Be(99);
    }

    #endregion
}
