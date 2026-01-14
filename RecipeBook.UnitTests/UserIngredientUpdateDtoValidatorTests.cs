using Application.DTO.UserIngredient;
using Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla UserIngredientUpdateDtoValidator.
/// Weryfikują reguły walidacji dla aktualizacji składnika użytkownika.
/// </summary>
public class UserIngredientUpdateDtoValidatorTests
{
    private readonly UserIngredientUpdateDtoValidator _validator;

    public UserIngredientUpdateDtoValidatorTests()
    {
        _validator = new UserIngredientUpdateDtoValidator();
    }

    [Fact]
    public void UserIngredientUpdateDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = 500
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UserIngredientUpdateDto_WithZeroQuantity_ShouldPassValidation()
    {
        // Arrange - W odróżnieniu od Create, Update pozwala na 0 (usunięcie składnika)
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = 0
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void UserIngredientUpdateDto_WithNegativeQuantity_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = -10
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than or equal to zero");
    }

    [Fact]
    public void UserIngredientUpdateDto_WithQuantityEqualToOneMillion_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = 1000000
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity cannot exceed 1 million");
    }

    [Fact]
    public void UserIngredientUpdateDto_WithQuantityJustBelowOneMillion_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = 999999
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void UserIngredientUpdateDto_WithVerySmallPositiveQuantity_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = 0.5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void UserIngredientUpdateDto_WithQuantityOverOneMillion_ShouldFailValidation()
    {
        // Arrange
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = 1500000
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(100)]
    [InlineData(5000)]
    [InlineData(999999)]
    public void UserIngredientUpdateDto_WithValidQuantityRange_ShouldPassValidation(decimal quantity)
    {
        // Arrange
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = quantity
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    [InlineData(1000000)]
    [InlineData(2000000)]
    public void UserIngredientUpdateDto_WithInvalidQuantityRange_ShouldFailValidation(decimal quantity)
    {
        // Arrange
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = quantity
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void UserIngredientUpdateDto_BoundaryValueZero_ShouldPassValidation()
    {
        // Arrange - Sprawdzenie wartości brzegowej (dozwolone od 0)
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = 0
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UserIngredientUpdateDto_BoundaryValueJustBelowMax_ShouldPassValidation()
    {
        // Arrange - Sprawdzenie wartości brzegowej (999999 < 1000000)
        var dto = new UserIngredientUpdateDto
        {
            IngredientId = 5,
            Quantity = 999999.99m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
