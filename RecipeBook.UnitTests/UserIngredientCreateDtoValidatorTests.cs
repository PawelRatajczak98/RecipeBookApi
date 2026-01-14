using Application.DTO.UserIngredient;
using Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla UserIngredientCreateDtoValidator.
/// Weryfikują reguły walidacji dla dodawania składnika użytkownika.
/// </summary>
public class UserIngredientCreateDtoValidatorTests
{
    private readonly UserIngredientCreateDtoValidator _validator;

    public UserIngredientCreateDtoValidatorTests()
    {
        _validator = new UserIngredientCreateDtoValidator();
    }

    [Fact]
    public void UserIngredientCreateDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Mąka",
            Quantity = 1000,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UserIngredientCreateDto_WithZeroIngredientId_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 0,
            IngredientName = "Mąka",
            Quantity = 1000,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientId)
            .WithErrorMessage("Ingredient ID is required");
    }

    [Fact]
    public void UserIngredientCreateDto_WithZeroQuantity_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Mąka",
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
    public void UserIngredientCreateDto_WithNegativeQuantity_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Mąka",
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
    public void UserIngredientCreateDto_WithQuantityEqualToOneMillion_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Mąka",
            Quantity = 1000000,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Million is too much");
    }

    [Fact]
    public void UserIngredientCreateDto_WithQuantityJustBelowOneMillion_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Mąka",
            Quantity = 999999,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void UserIngredientCreateDto_WithVerySmallPositiveQuantity_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Sól",
            Quantity = 0.5m,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void UserIngredientCreateDto_WithEmptyUnit_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Mąka",
            Quantity = 1000,
            Unit = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage("Unit is required");
    }

    [Fact]
    public void UserIngredientCreateDto_WithNullUnit_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Mąka",
            Quantity = 1000,
            Unit = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Unit);
    }

    [Fact]
    public void UserIngredientCreateDto_WithUnitTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Mąka",
            Quantity = 1000,
            Unit = new string('a', 51) // 51 znaków (max to 50)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage("Unit cannot exceed 50 characters");
    }

    [Fact]
    public void UserIngredientCreateDto_WithUnitExactlyMaxLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Mąka",
            Quantity = 1000,
            Unit = new string('a', 50) // Dokładnie 50 znaków
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Unit);
    }

    [Fact]
    public void UserIngredientCreateDto_WithAllFieldsInvalid_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 0,
            IngredientName = "Mąka",
            Quantity = -10,
            Unit = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IngredientId);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
        result.ShouldHaveValidationErrorFor(x => x.Unit);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100)]
    [InlineData(5000)]
    [InlineData(999999)]
    public void UserIngredientCreateDto_WithValidQuantityRange_ShouldPassValidation(decimal quantity)
    {
        // Arrange
        var dto = new UserIngredientCreateDto
        {
            IngredientId = 5,
            IngredientName = "Test",
            Quantity = quantity,
            Unit = "g"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }
}
