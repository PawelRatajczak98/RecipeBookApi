using Application.DTO.Ingredient;
using Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla IngredientCreateDtoValidator.
/// Weryfikują reguły walidacji dla tworzenia składnika.
/// </summary>
public class IngredientCreateDtoValidatorTests
{
    private readonly IngredientCreateDtoValidator _validator;

    public IngredientCreateDtoValidatorTests()
    {
        _validator = new IngredientCreateDtoValidator();
    }

    [Fact]
    public void IngredientCreateDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "Mąka",
            Description = "Mąka pszenna",
            PriceFor100Grams = 2.50m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void IngredientCreateDto_WithEmptyName_ShouldFailValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "",
            Description = "Test description",
            PriceFor100Grams = 2.50m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void IngredientCreateDto_WithNameTooShort_ShouldFailValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "ab", // 2 znaki (min to 3)
            Description = "Test description",
            PriceFor100Grams = 2.50m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void IngredientCreateDto_WithNameTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = new string('a', 24), // 24 znaki (max to 23)
            Description = "Test description",
            PriceFor100Grams = 2.50m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void IngredientCreateDto_WithNameBoundaryValues_ShouldPassValidation()
    {
        // Arrange - min length (3)
        var dtoMin = new IngredientCreateDto
        {
            Name = "abc",
            Description = "Test",
            PriceFor100Grams = 1.0m
        };

        // Arrange - max length (23)
        var dtoMax = new IngredientCreateDto
        {
            Name = new string('a', 23),
            Description = "Test",
            PriceFor100Grams = 1.0m
        };

        // Act
        var resultMin = _validator.TestValidate(dtoMin);
        var resultMax = _validator.TestValidate(dtoMax);

        // Assert
        resultMin.ShouldNotHaveValidationErrorFor(x => x.Name);
        resultMax.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void IngredientCreateDto_WithEmptyDescription_ShouldFailValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "Mąka",
            Description = "",
            PriceFor100Grams = 2.50m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void IngredientCreateDto_WithDescriptionTooShort_ShouldFailValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "Mąka",
            Description = "a", // 1 znak (min to 2)
            PriceFor100Grams = 2.50m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void IngredientCreateDto_WithDescriptionTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "Mąka",
            Description = new string('a', 51), // 51 znaków (max to 50)
            PriceFor100Grams = 2.50m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void IngredientCreateDto_WithDescriptionBoundaryValues_ShouldPassValidation()
    {
        // Arrange - min length (2)
        var dtoMin = new IngredientCreateDto
        {
            Name = "Test",
            Description = "ab",
            PriceFor100Grams = 1.0m
        };

        // Arrange - max length (50)
        var dtoMax = new IngredientCreateDto
        {
            Name = "Test",
            Description = new string('a', 50),
            PriceFor100Grams = 1.0m
        };

        // Act
        var resultMin = _validator.TestValidate(dtoMin);
        var resultMax = _validator.TestValidate(dtoMax);

        // Assert
        resultMin.ShouldNotHaveValidationErrorFor(x => x.Description);
        resultMax.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void IngredientCreateDto_WithZeroPrice_ShouldFailValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "Mąka",
            Description = "Test description",
            PriceFor100Grams = 0
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriceFor100Grams);
    }

    [Fact]
    public void IngredientCreateDto_WithNegativePrice_ShouldFailValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "Mąka",
            Description = "Test description",
            PriceFor100Grams = -5.00m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriceFor100Grams);
    }

    [Fact]
    public void IngredientCreateDto_WithVerySmallPositivePrice_ShouldPassValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "Test",
            Description = "Test description",
            PriceFor100Grams = 0.01m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PriceFor100Grams);
    }

    [Fact]
    public void IngredientCreateDto_WithAllFieldsInvalid_ShouldFailValidation()
    {
        // Arrange
        var dto = new IngredientCreateDto
        {
            Name = "ab",           // Za krótkie
            Description = "a",      // Za krótkie
            PriceFor100Grams = 0    // Nieprawidłowa cena
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.PriceFor100Grams);
    }
}
