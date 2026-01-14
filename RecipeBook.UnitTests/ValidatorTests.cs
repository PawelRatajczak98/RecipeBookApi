using Application.Dto;
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
            PreparationTime = 30,
            Servings = 4,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200 },
                new RecipeIngredientDto { IngredientId = 2, Quantity = 100 }
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
            PreparationTime = 30,
            Servings = 4,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200 }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nazwa przepisu jest wymagana.");
    }

    [Fact]
    public void RecipeCreateDto_WithNameTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = new string('A', 201), // 201 znaków
            Description = "Opis",
            Instructions = "Instrukcje",
            PreparationTime = 30,
            Servings = 4,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200 }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nazwa przepisu nie może przekraczać 200 znaków.");
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
            PreparationTime = 30,
            Servings = 4,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>() // Pusta lista
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Ingredients)
            .WithErrorMessage("Przepis musi zawierać przynajmniej jeden składnik.");
    }

    [Theory]
    [InlineData(0, "Czas przygotowania musi być większy niż 0.")]
    [InlineData(-5, "Czas przygotowania musi być większy niż 0.")]
    [InlineData(0, "Liczba porcji musi być większa niż 0.", 0)]
    [InlineData(30, "Liczba porcji musi być większa niż 0.", -1)]
    public void RecipeCreateDto_WithInvalidValues_ShouldFailValidation(
        int preparationTime,
        string expectedError,
        int servings = 4)
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "Opis",
            Instructions = "Instrukcje",
            PreparationTime = preparationTime,
            Servings = servings,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200 }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public void RecipeCreateDto_WithNullIngredients_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "Opis",
            Instructions = "Instrukcje",
            PreparationTime = 30,
            Servings = 4,
            CategoryId = 1,
            Ingredients = null! // Null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Ingredients);
    }

    [Fact]
    public void RecipeCreateDto_WithInvalidCategoryId_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "Opis",
            Instructions = "Instrukcje",
            PreparationTime = 30,
            Servings = 4,
            CategoryId = 0, // Nieprawidłowe ID
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200 }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("ID kategorii musi być większe niż 0.");
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
            PreparationTime = 30,
            Servings = 4,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200 }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Opis przepisu jest wymagany.");
    }

    [Fact]
    public void RecipeCreateDto_WithEmptyInstructions_ShouldFailValidation()
    {
        // Arrange
        var dto = new RecipeCreateDto
        {
            Name = "Test Recipe",
            Description = "Opis",
            Instructions = "", // Puste instrukcje
            PreparationTime = 30,
            Servings = 4,
            CategoryId = 1,
            Ingredients = new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto { IngredientId = 1, Quantity = 200 }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Instructions)
            .WithErrorMessage("Instrukcje przygotowania są wymagane.");
    }

    #endregion
}
