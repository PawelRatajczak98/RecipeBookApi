using Application.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserContextService
    {
        string? GetUserId();
        string GetUsername();
        ClaimsPrincipal? User { get; }
        Task <UserDto> GetUserDto();
    }
}
