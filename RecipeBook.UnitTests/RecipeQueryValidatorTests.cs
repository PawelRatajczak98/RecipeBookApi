using Application.Query;
using Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla RecipeQueryValidator.
/// Weryfikują reguły walidacji dla parametrów zapytania o przepisy.
/// </summary>
public class RecipeQueryValidatorTests
{
    private readonly RecipeQueryValidator _validator;

    public RecipeQueryValidatorTests()
    {
        _validator = new RecipeQueryValidator();
    }

    [Fact]
    public void RecipeQuery_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SearchPhrase = "pasta",
            Budget = 50.00m
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeQuery_WithPageNumberZero_ShouldFailValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 0,
            PageSize = 10
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Fact]
    public void RecipeQuery_WithNegativePageNumber_ShouldFailValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = -1,
            PageSize = 10
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Fact]
    public void RecipeQuery_WithPageNumberOne_ShouldPassValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(25)]
    public void RecipeQuery_WithAllowedPageSize_ShouldPassValidation(int pageSize)
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(12)]
    [InlineData(30)]
    [InlineData(50)]
    [InlineData(100)]
    public void RecipeQuery_WithNotAllowedPageSize_ShouldFailValidation(int pageSize)
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor("Page Size") // Validator używa "Page Size" zamiast "PageSize"
            .WithErrorMessage("PageSize must in [5,10,15,20,25]");
    }

    [Fact]
    public void RecipeQuery_WithNullSearchPhrase_ShouldPassValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SearchPhrase = null
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeQuery_WithEmptySearchPhrase_ShouldPassValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SearchPhrase = ""
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeQuery_WithSearchPhrase_ShouldPassValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SearchPhrase = "spaghetti"
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeQuery_WithNullBudget_ShouldPassValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = 10,
            Budget = null
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeQuery_WithPositiveBudget_ShouldPassValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = 10,
            Budget = 100.50m
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeQuery_WithZeroBudget_ShouldPassValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 1,
            PageSize = 10,
            Budget = 0
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeQuery_WithAllFieldsSet_ShouldPassValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 5,
            PageSize = 25,
            SearchPhrase = "Italian cuisine",
            Budget = 75.00m
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecipeQuery_WithMultipleInvalidFields_ShouldFailValidation()
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = 0,
            PageSize = 13 // Not in allowed list
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        result.ShouldHaveValidationErrorFor("Page Size"); // Validator używa "Page Size" zamiast "PageSize"
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(10, 15)]
    [InlineData(50, 20)]
    [InlineData(100, 25)]
    public void RecipeQuery_WithVariousValidCombinations_ShouldPassValidation(int pageNumber, int pageSize)
    {
        // Arrange
        var query = new RecipeQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
