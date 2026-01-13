using Application.DTO;
using Application.DTO.User;
using Azure;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<AppUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto registerRequestDto)
        {

            var user = new AppUser
            {
                UserName = registerRequestDto.UserName,
                Email = registerRequestDto.Email,
                /// PasswordHashing is handled by UserManager
            };

            var result = await _userManager.CreateAsync(user, registerRequestDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Member");
                return true;
            }
            return false;
        }

        public async Task<string> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.FindByNameAsync(loginRequestDto.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequestDto.Password))
            {
                throw new Exception("Błędna nazwa użytkownika lub hasło");
            }
            
            string token = await _tokenService.CreateJWTToken(user);
            
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Błąd generowania Tokenu");
            }
            return token;
        }
    }
    
}
