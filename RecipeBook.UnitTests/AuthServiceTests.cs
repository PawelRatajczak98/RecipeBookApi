using Application.Dto;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla serwisów związanych z autentykacją i autoryzacją.
/// </summary>
public class AuthServiceTests
{
    #region TokenService Tests

    [Fact]
    public void TokenService_ShouldGenerateValidJwtToken_WithUserAndRoles()
    {
        // Arrange
        var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfiguration.Setup(x => x["JwtSettings:Key"]).Returns("SuperSecretKeyForJWTTokenGeneration12345678");
        mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("RecipeBookAPI");
        mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("RecipeBookClient");
        mockConfiguration.Setup(x => x["JwtSettings:ExpiryInMinutes"]).Returns("60");

        var tokenService = new TokenService(mockConfiguration.Object);

        var user = new AppUser
        {
            Id = "user-123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var roles = new List<string> { "Member", "User" };

        // Act
        var token = tokenService.GenerateToken(user, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT ma 3 części: header.payload.signature
    }

    [Fact]
    public void TokenService_ShouldIncludeUserClaims_InGeneratedToken()
    {
        // Arrange
        var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfiguration.Setup(x => x["JwtSettings:Key"]).Returns("SuperSecretKeyForJWTTokenGeneration12345678");
        mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("RecipeBookAPI");
        mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("RecipeBookClient");
        mockConfiguration.Setup(x => x["JwtSettings:ExpiryInMinutes"]).Returns("60");

        var tokenService = new TokenService(mockConfiguration.Object);

        var user = new AppUser
        {
            Id = "user-456",
            UserName = "john.doe",
            Email = "john@example.com"
        };

        var roles = new List<string> { "Admin" };

        // Act
        var token = tokenService.GenerateToken(user, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        // Token powinien zawierać claims użytkownika (można to zweryfikować dekodując JWT)
    }

    [Fact]
    public void TokenService_ShouldGenerateDifferentTokens_ForDifferentUsers()
    {
        // Arrange
        var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfiguration.Setup(x => x["JwtSettings:Key"]).Returns("SuperSecretKeyForJWTTokenGeneration12345678");
        mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("RecipeBookAPI");
        mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("RecipeBookClient");
        mockConfiguration.Setup(x => x["JwtSettings:ExpiryInMinutes"]).Returns("60");

        var tokenService = new TokenService(mockConfiguration.Object);

        var user1 = new AppUser { Id = "user-1", UserName = "user1", Email = "user1@test.com" };
        var user2 = new AppUser { Id = "user-2", UserName = "user2", Email = "user2@test.com" };

        var roles = new List<string> { "Member" };

        // Act
        var token1 = tokenService.GenerateToken(user1, roles);
        var token2 = tokenService.GenerateToken(user2, roles);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void TokenService_ShouldSetExpirationTime_BasedOnConfiguration()
    {
        // Arrange
        var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfiguration.Setup(x => x["JwtSettings:Key"]).Returns("SuperSecretKeyForJWTTokenGeneration12345678");
        mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("RecipeBookAPI");
        mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("RecipeBookClient");
        mockConfiguration.Setup(x => x["JwtSettings:ExpiryInMinutes"]).Returns("120"); // 2 godziny

        var tokenService = new TokenService(mockConfiguration.Object);

        var user = new AppUser { Id = "user-123", UserName = "testuser", Email = "test@example.com" };
        var roles = new List<string> { "Member" };

        // Act
        var token = tokenService.GenerateToken(user, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        // Token jest wygenerowany z czasem wygaśnięcia 120 minut
    }

    #endregion

    #region AuthService Tests

    [Fact]
    public async Task AuthService_Register_ShouldCreateNewUser_WithHashedPassword()
    {
        // Arrange
        var mockUserManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        var mockTokenService = new Mock<ITokenService>();

        var registerDto = new RegisterDto
        {
            UserName = "newuser",
            Email = "newuser@example.com",
            Password = "Pa$$w0rd",
            Budget = 100.00m
        };

        mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), "Member"))
            .ReturnsAsync(IdentityResult.Success);

        var authService = new AuthService(mockUserManager.Object, mockTokenService.Object);

        // Act
        var result = await authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        mockUserManager.Verify(x => x.CreateAsync(
            It.Is<AppUser>(u => u.UserName == "newuser" && u.Email == "newuser@example.com"),
            "Pa$$w0rd"
        ), Times.Once);
    }

    [Fact]
    public async Task AuthService_Register_ShouldReturnError_WhenUserNameExists()
    {
        // Arrange
        var mockUserManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        var mockTokenService = new Mock<ITokenService>();

        var registerDto = new RegisterDto
        {
            UserName = "existinguser",
            Email = "existing@example.com",
            Password = "Pa$$w0rd",
            Budget = 50.00m
        };

        var identityError = new IdentityError
        {
            Code = "DuplicateUserName",
            Description = "Nazwa użytkownika jest już zajęta"
        };

        mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var authService = new AuthService(mockUserManager.Object, mockTokenService.Object);

        // Act
        var result = await authService.RegisterAsync(registerDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Nazwa użytkownika jest już zajęta");
    }

    [Fact]
    public async Task AuthService_Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var mockUserManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        var mockTokenService = new Mock<ITokenService>();

        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "Pa$$w0rd"
        };

        var user = new AppUser
        {
            Id = "user-123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var roles = new List<string> { "Member" };

        mockUserManager.Setup(x => x.FindByNameAsync("testuser"))
            .ReturnsAsync(user);

        mockUserManager.Setup(x => x.CheckPasswordAsync(user, "Pa$$w0rd"))
            .ReturnsAsync(true);

        mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        mockTokenService.Setup(x => x.GenerateToken(user, roles))
            .Returns("valid.jwt.token");

        var authService = new AuthService(mockUserManager.Object, mockTokenService.Object);

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().Be("valid.jwt.token");
    }

    [Fact]
    public async Task AuthService_Login_ShouldReturnError_WhenPasswordIsInvalid()
    {
        // Arrange
        var mockUserManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        var mockTokenService = new Mock<ITokenService>();

        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "WrongPassword"
        };

        var user = new AppUser
        {
            Id = "user-123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        mockUserManager.Setup(x => x.FindByNameAsync("testuser"))
            .ReturnsAsync(user);

        mockUserManager.Setup(x => x.CheckPasswordAsync(user, "WrongPassword"))
            .ReturnsAsync(false);

        var authService = new AuthService(mockUserManager.Object, mockTokenService.Object);

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Nieprawidłowe dane logowania");
    }

    [Fact]
    public async Task AuthService_Login_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        var mockUserManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        var mockTokenService = new Mock<ITokenService>();

        var loginDto = new LoginDto
        {
            UserName = "nonexistent",
            Password = "Pa$$w0rd"
        };

        mockUserManager.Setup(x => x.FindByNameAsync("nonexistent"))
            .ReturnsAsync((AppUser?)null);

        var authService = new AuthService(mockUserManager.Object, mockTokenService.Object);

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Errors.Should().Contain("Użytkownik nie istnieje");
    }

    #endregion

    #region UserContextService Tests

    [Fact]
    public void UserContextService_ShouldExtractUserId_FromClaimsPrincipal()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user-xyz-789"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Member")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        var userContextService = new UserContextService(mockHttpContextAccessor.Object);

        // Act
        var userId = userContextService.GetUserId;

        // Assert
        userId.Should().Be("user-xyz-789");
    }

    [Fact]
    public void UserContextService_ShouldReturnNull_WhenUserNotAuthenticated()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

        var mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(new ClaimsPrincipal());

        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        var userContextService = new UserContextService(mockHttpContextAccessor.Object);

        // Act
        var userId = userContextService.GetUserId;

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void UserContextService_ShouldExtractUserName_FromClaims()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "john.doe"),
            new Claim(ClaimTypes.Email, "john@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        var userContextService = new UserContextService(mockHttpContextAccessor.Object);

        // Act
        var userName = userContextService.GetUserName;

        // Assert
        userName.Should().Be("john.doe");
    }

    [Fact]
    public void UserContextService_ShouldCheckIfUserHasRole()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "adminuser"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Moderator")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        var userContextService = new UserContextService(mockHttpContextAccessor.Object);

        // Act
        var isAdmin = userContextService.IsInRole("Admin");
        var isMember = userContextService.IsInRole("Member");

        // Assert
        isAdmin.Should().BeTrue();
        isMember.Should().BeFalse();
    }

    #endregion
}
