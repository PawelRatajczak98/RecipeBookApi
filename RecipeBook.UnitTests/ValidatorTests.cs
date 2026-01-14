using Application.DTO.Recipe;
using Application.DTO.RecipeIngredient;
using Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla walidatorów FluentValidation.
/// Weryfikują poprawność reguł walidacji dla DTO aplikacji.
/// </summary>
public class ValidatorTests
{
    private readonly RecipeCreateDtoValidator _validator;

    public ValidatorTests()
    {
        _validator = new RecipeCreateDtoValidator();
    }

    #region RecipeCreateDtoValidator Tests

    [Fact]
    public void RecipeCreateDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Spaghetti Carbonara",
            Description = "Klasyczny włoski przepis",
            Instructions = "1. Ugotuj makaron\n2. Przygotuj sos\n3. Wymieszaj",
            PreparationTime = TimeSpan.FromMinutes(20),
            CookingTime = TimeSpan.FromMinutes(15),
            Servings = 4,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200, Unit = "g" },
                new RecipeIngredientDto { IngredientId = 2, Quantity = 100, Unit = "g" }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeCreateDto_WithEmptyName_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "",
            Description = "Opis",
            Instructions = "Instrukcje",
            PreparationTime = TimeSpan.FromMinutes(30),
            CookingTime = TimeSpan.FromMinutes(10),
            Servings = 4,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200, Unit = "g" }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Recipe name is required.");
    }

    [Fact]
    public void RecipeCreateDto_WithNameTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = new string('A', 101), // 101 znaków
            Description = "Opis",
            Instructions = "Instrukcje",
            PreparationTime = TimeSpan.FromMinutes(30),
            CookingTime = TimeSpan.FromMinutes(10),
            Servings = 4,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200, Unit = "g" }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Recipe name cannot exceed 100 characters.");
    }

    [Fact]
    public void RecipeCreateDto_WithEmptyDescription_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "", // Pusty opis
            Instructions = "Instrukcje",
            PreparationTime = TimeSpan.FromMinutes(30),
            CookingTime = TimeSpan.FromMinutes(10),
            Servings = 4,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200, Unit = "g" }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Recipe description is required.");
    }

    [Fact]
    public void RecipeCreateDto_WithDescriptionTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = new string('A', 501), // 501 znaków
            Instructions = "Instrukcje",
            PreparationTime = TimeSpan.FromMinutes(30),
            CookingTime = TimeSpan.FromMinutes(10),
            Servings = 4,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200, Unit = "g" }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Recipe description cannot exceed 500 characters.");
    }

    [Fact]
    public void RecipeCreateDto_WithNoIngredients_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "Opis",
            Instructions = "Instrukcje",
            PreparationTime = TimeSpan.FromMinutes(30),
            CookingTime = TimeSpan.FromMinutes(10),
            Servings = 4,
            RecipeIngredientsDto = new List<RecipeIngredientDto>() // Pusta lista
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecipeIngredientsDto)
            .WithErrorMessage("At least one ingredient is required.");
    }

    [Fact]
    public void RecipeCreateDto_WithInvalidIngredientId_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "Opis",
            Instructions = "Instrukcje",
            PreparationTime = TimeSpan.FromMinutes(30),
            CookingTime = TimeSpan.FromMinutes(10),
            Servings = 4,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 0, Quantity = 200, Unit = "g" } // Invalid ID
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecipeIngredientsDto)
            .WithErrorMessage("Each ingredient must have a valid ID and quantity greater than zero.");
    }

    [Fact]
    public void RecipeCreateDto_WithInvalidIngredientQuantity_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "Opis",
            Instructions = "Instrukcje",
            PreparationTime = TimeSpan.FromMinutes(30),
            CookingTime = TimeSpan.FromMinutes(10),
            Servings = 4,
            RecipeIngredientsDto = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 0, Unit = "g" } // Invalid quantity
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecipeIngredientsDto)
            .WithErrorMessage("Each ingredient must have a valid ID and quantity greater than zero.");
    }

    #endregion
}
