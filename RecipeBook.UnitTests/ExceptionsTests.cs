using Application.Exceptions;
using FluentAssertions;
using System.Net;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla custom exceptions.
/// Weryfikują poprawność HTTP status codes i hierarchii dziedziczenia.
/// </summary>
public class ExceptionsTests
{
    #region ApiException Tests

    [Fact]
    public void ApiException_ShouldStoreStatusCodeAndMessage()
    {
        // Arrange
        var statusCode = 500;
        var message = "Internal server error";
        var details = "Stack trace details";

        // Act
        var exception = new ApiException(statusCode, message, details);

        // Assert
        exception.StatusCode.Should().Be(500);
        exception.Message.Should().Be("Internal server error");
        exception.Details.Should().Be("Stack trace details");
    }

    [Fact]
    public void ApiException_WithoutDetails_ShouldStoreNullDetails()
    {
        // Arrange & Act
        var exception = new ApiException(404, "Not found");

        // Assert
        exception.StatusCode.Should().Be(404);
        exception.Message.Should().Be("Not found");
        exception.Details.Should().BeNull();
    }

    [Fact]
    public void ApiException_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new ApiException(500, "Error");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    #endregion

    #region ValidationException Tests

    [Fact]
    public void ValidationException_ShouldHaveStatusCode400()
    {
        // Arrange & Act
        var exception = new ValidationException("Validation failed");

        // Assert
        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.StatusCode.Should().Be(400);
    }

    [Fact]
    public void ValidationException_ShouldStoreMessageAndDetails()
    {
        // Arrange & Act
        var exception = new ValidationException("Invalid input", "Field X is required");

        // Assert
        exception.Message.Should().Be("Invalid input");
        exception.Details.Should().Be("Field X is required");
    }

    [Fact]
    public void ValidationException_ShouldInheritFromApiException()
    {
        // Arrange & Act
        var exception = new ValidationException("Error");

        // Assert
        exception.Should().BeAssignableTo<ApiException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    #endregion

    #region NotFoundException Tests

    [Fact]
    public void NotFoundException_ShouldHaveStatusCode404()
    {
        // Arrange & Act
        var exception = new NotFoundException("Resource not found");

        // Assert
        exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.StatusCode.Should().Be(404);
    }

    [Fact]
    public void NotFoundException_ShouldStoreMessageAndDetails()
    {
        // Arrange & Act
        var exception = new NotFoundException("Recipe not found", "RecipeId: 123");

        // Assert
        exception.Message.Should().Be("Recipe not found");
        exception.Details.Should().Be("RecipeId: 123");
    }

    [Fact]
    public void NotFoundException_ShouldInheritFromApiException()
    {
        // Arrange & Act
        var exception = new NotFoundException("Not found");

        // Assert
        exception.Should().BeAssignableTo<ApiException>();
    }

    #endregion

    #region UnauthorizedException Tests

    [Fact]
    public void UnauthorizedException_ShouldHaveStatusCode401()
    {
        // Arrange & Act
        var exception = new UnauthorizedException("Unauthorized access");

        // Assert
        exception.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        exception.StatusCode.Should().Be(401);
    }

    [Fact]
    public void UnauthorizedException_ShouldStoreMessageAndDetails()
    {
        // Arrange & Act
        var exception = new UnauthorizedException("Token expired", "JWT validation failed");

        // Assert
        exception.Message.Should().Be("Token expired");
        exception.Details.Should().Be("JWT validation failed");
    }

    [Fact]
    public void UnauthorizedException_ShouldInheritFromApiException()
    {
        // Arrange & Act
        var exception = new UnauthorizedException("Unauthorized");

        // Assert
        exception.Should().BeAssignableTo<ApiException>();
    }

    #endregion

    #region ForbiddenException Tests

    [Fact]
    public void ForbiddenException_ShouldHaveStatusCode403()
    {
        // Arrange & Act
        var exception = new ForbiddenException("Access forbidden");

        // Assert
        exception.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        exception.StatusCode.Should().Be(403);
    }

    [Fact]
    public void ForbiddenException_ShouldStoreMessageAndDetails()
    {
        // Arrange & Act
        var exception = new ForbiddenException("Insufficient permissions", "Admin role required");

        // Assert
        exception.Message.Should().Be("Insufficient permissions");
        exception.Details.Should().Be("Admin role required");
    }

    [Fact]
    public void ForbiddenException_ShouldInheritFromApiException()
    {
        // Arrange & Act
        var exception = new ForbiddenException("Forbidden");

        // Assert
        exception.Should().BeAssignableTo<ApiException>();
    }

    #endregion

    #region OperationCanceledException Tests

    [Fact]
    public void OperationCanceledException_ShouldHaveStatusCode408()
    {
        // Arrange & Act
        var exception = new Application.Exceptions.OperationCanceledException("Request timeout");

        // Assert
        exception.StatusCode.Should().Be((int)HttpStatusCode.RequestTimeout);
        exception.StatusCode.Should().Be(408);
    }

    [Fact]
    public void OperationCanceledException_ShouldStoreMessageAndDetails()
    {
        // Arrange & Act
        var exception = new Application.Exceptions.OperationCanceledException(
            "Operation timed out",
            "Exceeded 30 seconds");

        // Assert
        exception.Message.Should().Be("Operation timed out");
        exception.Details.Should().Be("Exceeded 30 seconds");
    }

    [Fact]
    public void OperationCanceledException_ShouldInheritFromApiException()
    {
        // Arrange & Act
        var exception = new Application.Exceptions.OperationCanceledException("Timeout");

        // Assert
        exception.Should().BeAssignableTo<ApiException>();
    }

    #endregion

    #region Status Code Verification

    [Fact]
    public void AllExceptions_ShouldHaveCorrectStatusCodes()
    {
        // Arrange & Act
        var validationEx = new ValidationException("Test");
        var unauthorizedEx = new UnauthorizedException("Test");
        var forbiddenEx = new ForbiddenException("Test");
        var notFoundEx = new NotFoundException("Test");

        // Assert
        validationEx.StatusCode.Should().Be(400);
        unauthorizedEx.StatusCode.Should().Be(401);
        forbiddenEx.StatusCode.Should().Be(403);
        notFoundEx.StatusCode.Should().Be(404);
    }

    #endregion

    #region Inheritance Hierarchy

    [Fact]
    public void AllCustomExceptions_ShouldInheritFromApiException()
    {
        // Arrange
        var validationEx = new ValidationException("Test");
        var notFoundEx = new NotFoundException("Test");
        var unauthorizedEx = new UnauthorizedException("Test");
        var forbiddenEx = new ForbiddenException("Test");
        var timeoutEx = new Application.Exceptions.OperationCanceledException("Test");

        // Assert
        validationEx.Should().BeAssignableTo<ApiException>();
        notFoundEx.Should().BeAssignableTo<ApiException>();
        unauthorizedEx.Should().BeAssignableTo<ApiException>();
        forbiddenEx.Should().BeAssignableTo<ApiException>();
        timeoutEx.Should().BeAssignableTo<ApiException>();
    }

    [Fact]
    public void AllCustomExceptions_ShouldInheritFromBaseException()
    {
        // Arrange
        var apiEx = new ApiException(500, "Test");
        var validationEx = new ValidationException("Test");
        var notFoundEx = new NotFoundException("Test");

        // Assert
        apiEx.Should().BeAssignableTo<Exception>();
        validationEx.Should().BeAssignableTo<Exception>();
        notFoundEx.Should().BeAssignableTo<Exception>();
    }

    #endregion
}
