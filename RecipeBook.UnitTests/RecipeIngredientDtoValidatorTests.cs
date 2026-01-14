using Application.DTO.RecipeIngredient;
using Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla RecipeIngredientDtoValidator.
/// Weryfikują reguły walidacji dla składników przepisu.
/// </summary>
public class RecipeIngredientDtoValidatorTests
{
    private readonly RecipeIngredientDtoValidator _validator;

    public RecipeIngredientDtoValidatorTests()
    {
        _validator = new RecipeIngredientDtoValidator();
    }

    [Fact]
    public void RecipeIngredientDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = "Mąka",
            IngredientId = 1,
            Quantity = 500,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeIngredientDto_WithEmptyIngredientName_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = "",
            IngredientId = 1,
            Quantity = 500,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientName)
            .WithErrorMessage("Name of ingredient is required");
    }

    [Fact]
    public void RecipeIngredientDto_WithNullIngredientName_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = null,
            IngredientId = 1,
            Quantity = 500,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientName);
    }

    [Fact]
    public void RecipeIngredientDto_WithZeroQuantity_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = "Mąka",
            IngredientId = 1,
            Quantity = 0,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than zero");
    }

    [Fact]
    public void RecipeIngredientDto_WithNegativeQuantity_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = "Mąka",
            IngredientId = 1,
            Quantity = -100,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than zero");
    }

    [Fact]
    public void RecipeIngredientDto_WithVerySmallPositiveQuantity_ShouldPassValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = "Sól",
            IngredientId = 1,
            Quantity = 0.5m,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void RecipeIngredientDto_WithEmptyUnit_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = "Mąka",
            IngredientId = 1,
            Quantity = 500,
            Unit = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage("Unit of measurement is required");
    }

    [Fact]
    public void RecipeIngredientDto_WithNullUnit_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = "Mąka",
            IngredientId = 1,
            Quantity = 500,
            Unit = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Unit);
    }

    [Fact]
    public void RecipeIngredientDto_WithDifferentUnits_ShouldPassValidation()
    {
        // Arrange
        var dtoGrams = new RecipeIngredientDto
        {
            IngredientName = "Mąka",
            IngredientId = 1,
            Quantity = 500,
            Unit = "g"
        };

        var dtoMilliliters = new RecipeIngredientDto
        {
            IngredientName = "Mleko",
            IngredientId = 2,
            Quantity = 250,
            Unit = "ml"
        };

        var dtoPieces = new RecipeIngredientDto
        {
            IngredientName = "Jajka",
            IngredientId = 3,
            Quantity = 2,
            Unit = "szt"
        };

        // Act
        var resultGrams = _validator.TestValidate(dtoGrams);
        var resultMl = _validator.TestValidate(dtoMilliliters);
        var resultPieces = _validator.TestValidate(dtoPieces);

        // Assert
        resultGrams.ShouldNotHaveAnyValidationErrors();
        resultMl.ShouldNotHaveAnyValidationErrors();
        resultPieces.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeIngredientDto_WithAllFieldsInvalid_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = "",
            IngredientId = 1,
            Quantity = 0,
            Unit = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientName);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
        result.ShouldHaveValidationErrorFor(x => x.Unit);
    }

    [Fact]
    public void RecipeIngredientDto_WithLargeQuantity_ShouldPassValidation()
    {
        // Arrange
        var dto = new RecipeIngredientDto
        {
            IngredientName = "Woda",
            IngredientId = 1,
            Quantity = 10000,
            Unit = "ml"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
