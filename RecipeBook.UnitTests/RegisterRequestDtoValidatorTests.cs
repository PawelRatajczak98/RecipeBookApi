using Application.DTO;
using Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla RegisterRequestDtoValidator.
/// Weryfikują reguły walidacji dla rejestracji użytkownika.
/// </summary>
public class RegisterRequestDtoValidatorTests
{
    private readonly RegisterRequestDtoValidator _validator;

    public RegisterRequestDtoValidatorTests()
    {
        _validator = new RegisterRequestDtoValidator();
    }

    [Fact]
    public void RegisterRequestDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            UserName = "NewUser",
            Password = "Password123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RegisterRequestDto_WithEmptyUserName_ShouldFailValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            UserName = "",
            Password = "Password123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("UserName is required");
    }

    [Fact]
    public void RegisterRequestDto_WithUserNameTooShort_ShouldFailValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            UserName = "ab", // 2 znaki (min to 3)
            Password = "Password123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("UserName must be at least 3 characters long");
    }

    [Fact]
    public void RegisterRequestDto_WithUserNameTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            UserName = new string('a', 21), // 21 znaków (max to 20)
            Password = "Password123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("UserName cannot exceed 20 characters");
    }

    [Fact]
    public void RegisterRequestDto_WithUserNameBoundaryValues_ShouldPassValidation()
    {
        // Arrange - min length (3)
        var dtoMin = new RegisterRequestDto
        {
            UserName = "abc",
            Password = "Password123"
        };

        // Arrange - max length (20)
        var dtoMax = new RegisterRequestDto
        {
            UserName = new string('a', 20),
            Password = "Password123"
        };

        // Act
        var resultMin = _validator.TestValidate(dtoMin);
        var resultMax = _validator.TestValidate(dtoMax);

        // Assert
        resultMin.ShouldNotHaveValidationErrorFor(x => x.UserName);
        resultMax.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void RegisterRequestDto_WithEmptyPassword_ShouldFailValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            UserName = "NewUser",
            Password = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public void RegisterRequestDto_WithPasswordTooShort_ShouldFailValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            UserName = "NewUser",
            Password = "12345" // 5 znaków (min to 6)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters long");
    }

    [Fact]
    public void RegisterRequestDto_WithPasswordExactlyMinLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            UserName = "NewUser",
            Password = "123456" // Dokładnie 6 znaków
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestDto_WithBothFieldsInvalid_ShouldFailValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            UserName = "ab", // Za krótkie
            Password = "123"  // Za krótkie
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestDto_WithSpecialCharactersInUserName_ShouldPassValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            UserName = "User_123",
            Password = "Password123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
