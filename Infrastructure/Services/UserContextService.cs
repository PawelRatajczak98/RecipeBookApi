using Application.DTO.User;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Services
{
    
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;

        public UserContextService(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public string GetUserId()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext is not null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            return result;
        }

        public string GetUsername()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext is not null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
            return result;
        }

       public ClaimsPrincipal? User
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if ( user is null)
                {
                    throw new InvalidOperationException("User is null");
                }
                return user;
            }
        }

       public async Task<UserDto> GetUserDto()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _dbContext.Users.FindAsync(userId);
            return new UserDto
            {
                Budget = user.Budget,
                UserName = user.UserName,
            };
        }
    }
}