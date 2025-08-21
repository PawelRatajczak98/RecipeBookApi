using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuthService
    {
        Task <bool> RegisterAsync (RegisterRequestDto registerRequestDto);
        Task <string> LoginAsync (LoginRequestDto loginRequestDto);
    }
}
