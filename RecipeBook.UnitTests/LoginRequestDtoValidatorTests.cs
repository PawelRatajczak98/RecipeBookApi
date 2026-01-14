using Application.DTO;
using Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla LoginRequestDtoValidator.
/// Weryfikują reguły walidacji dla logowania użytkownika.
/// </summary>
public class LoginRequestDtoValidatorTests
{
    private readonly LoginRequestDtoValidator _validator;

    public LoginRequestDtoValidatorTests()
    {
        _validator = new LoginRequestDtoValidator();
    }

    [Fact]
    public void LoginRequestDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = "TestUser",
            Password = "SecurePassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LoginRequestDto_WithEmptyUserName_ShouldFailValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = "",
            Password = "SecurePassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("UserName is required");
    }

    [Fact]
    public void LoginRequestDto_WithUserNameTooShort_ShouldFailValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = "ab", // 2 znaki (min to 3)
            Password = "SecurePassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("UserName must be at least 3 characters long");
    }

    [Fact]
    public void LoginRequestDto_WithUserNameTooLong_ShouldFailValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = new string('a', 21), // 21 znaków (max to 20)
            Password = "SecurePassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("UserName cannot exceed 20 characters");
    }

    [Fact]
    public void LoginRequestDto_WithUserNameExactlyMinLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = "abc", // Dokładnie 3 znaki
            Password = "SecurePassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void LoginRequestDto_WithUserNameExactlyMaxLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = new string('a', 20), // Dokładnie 20 znaków
            Password = "SecurePassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void LoginRequestDto_WithEmptyPassword_ShouldFailValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = "TestUser",
            Password = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public void LoginRequestDto_WithPasswordTooShort_ShouldFailValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = "TestUser",
            Password = "12345" // 5 znaków (min to 6)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters long");
    }

    [Fact]
    public void LoginRequestDto_WithPasswordExactlyMinLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = "TestUser",
            Password = "123456" // Dokładnie 6 znaków
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void LoginRequestDto_WithBothFieldsEmpty_ShouldFailValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            UserName = "",
            Password = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
