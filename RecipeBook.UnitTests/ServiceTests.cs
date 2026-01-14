using Application.Dto;
using Application.Query;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Infrastructure.Services;
using Moq;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla serwisów biznesowych aplikacji.
/// Używają mocków (Moq) do izolacji testowanych komponentów.
/// </summary>
public class ServiceTests
{
    #region BudgetService Tests

    [Fact]
    public async Task BudgetService_ShouldUseUserBudget_WhenQueryBudgetIsNull()
    {
        // Arrange
        var mockUserContext = new Mock<IUserContextService>();
        var mockUserRepository = new Mock<IUserRepository>();

        mockUserContext.Setup(x => x.GetUserId).Returns("user123");
        mockUserRepository.Setup(x => x.GetUserBudgetAsync("user123"))
            .ReturnsAsync(100.50m);

        var service = new BudgetService(mockUserContext.Object, mockUserRepository.Object);
        var query = new RecipeQuery { Budget = null };

        // Act
        var result = await service.GetEffectiveBudgetAsync(query);

        // Assert
        result.Should().Be(100.50m);
        mockUserRepository.Verify(x => x.GetUserBudgetAsync("user123"), Times.Once);
    }

    [Fact]
    public async Task BudgetService_ShouldUseQueryBudget_WhenProvided()
    {
        // Arrange
        var mockUserContext = new Mock<IUserContextService>();
        var mockUserRepository = new Mock<IUserRepository>();

        mockUserContext.Setup(x => x.GetUserId).Returns("user123");

        var service = new BudgetService(mockUserContext.Object, mockUserRepository.Object);
        var query = new RecipeQuery { Budget = 75.25m };

        // Act
        var result = await service.GetEffectiveBudgetAsync(query);

        // Assert
        result.Should().Be(75.25m);
        mockUserRepository.Verify(x => x.GetUserBudgetAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task BudgetService_ShouldApplyPagination_Correctly()
    {
        // Arrange
        var mockUserContext = new Mock<IUserContextService>();
        var mockUserRepository = new Mock<IUserRepository>();

        var service = new BudgetService(mockUserContext.Object, mockUserRepository.Object);
        var query = new RecipeQuery { PageNumber = 2, PageSize = 10 };

        // Act
        var (skip, take) = service.CalculatePagination(query);

        // Assert
        skip.Should().Be(10); // (2-1) * 10
        take.Should().Be(10);
    }

    #endregion

    #region RecipeService Tests

    [Fact]
    public async Task RecipeService_ShouldCalculateTotalCost_Correctly()
    {
        // Arrange
        var mockRecipeRepository = new Mock<IRecipeRepository>();
        var mockIngredientRepository = new Mock<IIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        var ingredients = new List<Ingredient>
        {
            new Ingredient { Id = 1, Name = "Mąka", PriceFor100Grams = 2.00m },
            new Ingredient { Id = 2, Name = "Cukier", PriceFor100Grams = 3.00m }
        };

        var recipe = new Recipe
        {
            Id = 1,
            Name = "Ciasto",
            RecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient { RecipeId = 1, IngredientId = 1, Quantity = 500 }, // 500g mąki = 10 zł
                new RecipeIngredient { RecipeId = 1, IngredientId = 2, Quantity = 200 }  // 200g cukru = 6 zł
            }
        };

        mockIngredientRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(ingredients[0]);
        mockIngredientRepository.Setup(x => x.GetByIdAsync(2))
            .ReturnsAsync(ingredients[1]);

        var service = new RecipeService(
            mockRecipeRepository.Object,
            mockIngredientRepository.Object,
            mockUserContext.Object);

        // Act
        var totalCost = await service.CalculateRecipeCostAsync(recipe);

        // Assert
        totalCost.Should().Be(16.00m); // 10 + 6 = 16
    }

    [Fact]
    public async Task RecipeService_ShouldMapDtoToEntity_Correctly()
    {
        // Arrange
        var mockRecipeRepository = new Mock<IRecipeRepository>();
        var mockIngredientRepository = new Mock<IIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        mockUserContext.Setup(x => x.GetUserId).Returns("user123");

        var dto = new RecipeCreateDto
        {
            Name = "Spaghetti",
            Description = "Włoski przepis",
            Instructions = "1. Gotuj\n2. Jedz",
            PreparationTime = 20,
            Servings = 2,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 300 }
            }
        };

        var service = new RecipeService(
            mockRecipeRepository.Object,
            mockIngredientRepository.Object,
            mockUserContext.Object);

        // Act
        var entity = service.MapDtoToEntity(dto);

        // Assert
        entity.Name.Should().Be("Spaghetti");
        entity.Description.Should().Be("Włoski przepis");
        entity.Instructions.Should().Be("1. Gotuj\n2. Jedz");
        entity.PreparationTime.Should().Be(20);
        entity.Servings.Should().Be(2);
        entity.CategoryId.Should().Be(1);
        entity.UserId.Should().Be("user123");
        entity.RecipeIngredients.Should().HaveCount(1);
        entity.RecipeIngredients.First().IngredientId.Should().Be(1);
        entity.RecipeIngredients.First().Quantity.Should().Be(300);
    }

    [Fact]
    public async Task RecipeService_GetByIdAsync_ShouldReturnRecipe_WhenExists()
    {
        // Arrange
        var mockRecipeRepository = new Mock<IRecipeRepository>();
        var mockIngredientRepository = new Mock<IIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        var recipe = new Recipe { Id = 1, Name = "Test Recipe" };
        mockRecipeRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(recipe);

        var service = new RecipeService(
            mockRecipeRepository.Object,
            mockIngredientRepository.Object,
            mockUserContext.Object);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Recipe");
    }

    [Fact]
    public async Task RecipeService_GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var mockRecipeRepository = new Mock<IRecipeRepository>();
        var mockIngredientRepository = new Mock<IIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        mockRecipeRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Recipe?)null);

        var service = new RecipeService(
            mockRecipeRepository.Object,
            mockIngredientRepository.Object,
            mockUserContext.Object);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RecipeService_ShouldFilterBySearchPhrase()
    {
        // Arrange
        var mockRecipeRepository = new Mock<IRecipeRepository>();
        var mockIngredientRepository = new Mock<IIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Name = "Spaghetti Carbonara", Description = "Włoski przepis" },
            new Recipe { Id = 2, Name = "Pizza Margherita", Description = "Klasyczna pizza" },
            new Recipe { Id = 3, Name = "Tiramisu", Description = "Włoski deser" }
        };

        mockRecipeRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(recipes);

        var service = new RecipeService(
            mockRecipeRepository.Object,
            mockIngredientRepository.Object,
            mockUserContext.Object);

        var query = new RecipeQuery { SearchPhrase = "włoski" };

        // Act
        var result = await service.SearchRecipesAsync(query);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Id == 1);
        result.Should().Contain(r => r.Id == 3);
    }

    #endregion

    #region UserIngredientService Tests

    [Fact]
    public async Task UserIngredientService_ShouldAddIngredient_Successfully()
    {
        // Arrange
        var mockRepository = new Mock<IUserIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        mockUserContext.Setup(x => x.GetUserId).Returns("user123");
        mockRepository.Setup(x => x.AddAsync(It.IsAny<UserIngredient>()))
            .Returns(Task.CompletedTask);

        var service = new UserIngredientService(mockRepository.Object, mockUserContext.Object);

        // Act
        await service.AddIngredientAsync(5, 250);

        // Assert
        mockRepository.Verify(x => x.AddAsync(It.Is<UserIngredient>(
            ui => ui.UserId == "user123" && ui.IngredientId == 5 && ui.Quantity == 250
        )), Times.Once);
    }

    [Fact]
    public async Task UserIngredientService_ShouldUpdateQuantity_Successfully()
    {
        // Arrange
        var mockRepository = new Mock<IUserIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        var userIngredient = new UserIngredient
        {
            UserId = "user123",
            IngredientId = 5,
            Quantity = 100
        };

        mockUserContext.Setup(x => x.GetUserId).Returns("user123");
        mockRepository.Setup(x => x.GetByIdAsync("user123", 5))
            .ReturnsAsync(userIngredient);
        mockRepository.Setup(x => x.UpdateAsync(It.IsAny<UserIngredient>()))
            .Returns(Task.CompletedTask);

        var service = new UserIngredientService(mockRepository.Object, mockUserContext.Object);

        // Act
        await service.UpdateQuantityAsync(5, 250);

        // Assert
        userIngredient.Quantity.Should().Be(250);
        mockRepository.Verify(x => x.UpdateAsync(userIngredient), Times.Once);
    }

    [Fact]
    public async Task UserIngredientService_ShouldRemoveIngredient_Successfully()
    {
        // Arrange
        var mockRepository = new Mock<IUserIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        mockUserContext.Setup(x => x.GetUserId).Returns("user123");
        mockRepository.Setup(x => x.DeleteAsync("user123", 5))
            .Returns(Task.CompletedTask);

        var service = new UserIngredientService(mockRepository.Object, mockUserContext.Object);

        // Act
        await service.RemoveIngredientAsync(5);

        // Assert
        mockRepository.Verify(x => x.DeleteAsync("user123", 5), Times.Once);
    }

    [Fact]
    public async Task UserIngredientService_ShouldGetAllIngredients_Successfully()
    {
        // Arrange
        var mockRepository = new Mock<IUserIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        var userIngredients = new List<UserIngredient>
        {
            new UserIngredient { UserId = "user123", IngredientId = 1, Quantity = 500 },
            new UserIngredient { UserId = "user123", IngredientId = 2, Quantity = 300 },
            new UserIngredient { UserId = "user123", IngredientId = 3, Quantity = 150 }
        };

        mockUserContext.Setup(x => x.GetUserId).Returns("user123");
        mockRepository.Setup(x => x.GetAllByUserIdAsync("user123"))
            .ReturnsAsync(userIngredients);

        var service = new UserIngredientService(mockRepository.Object, mockUserContext.Object);

        // Act
        var result = await service.GetAllUserIngredientsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(ui => ui.IngredientId == 1 && ui.Quantity == 500);
        result.Should().Contain(ui => ui.IngredientId == 2 && ui.Quantity == 300);
        result.Should().Contain(ui => ui.IngredientId == 3 && ui.Quantity == 150);
    }

    [Theory]
    [InlineData(500, 300, true)]  // Ma 500, potrzebuje 300 - OK
    [InlineData(300, 300, true)]  // Ma 300, potrzebuje 300 - OK
    [InlineData(200, 300, false)] // Ma 200, potrzebuje 300 - Brak
    [InlineData(0, 100, false)]   // Ma 0, potrzebuje 100 - Brak
    public async Task UserIngredientService_ShouldCheckSufficientQuantity(
        decimal userQuantity,
        decimal requiredQuantity,
        bool expectedResult)
    {
        // Arrange
        var mockRepository = new Mock<IUserIngredientRepository>();
        var mockUserContext = new Mock<IUserContextService>();

        var userIngredient = new UserIngredient
        {
            UserId = "user123",
            IngredientId = 5,
            Quantity = userQuantity
        };

        mockUserContext.Setup(x => x.GetUserId).Returns("user123");
        mockRepository.Setup(x => x.GetByIdAsync("user123", 5))
            .ReturnsAsync(userIngredient);

        var service = new UserIngredientService(mockRepository.Object, mockUserContext.Object);

        // Act
        var result = await service.HasSufficientQuantityAsync(5, requiredQuantity);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}
