using Application.DTO;
using Domain.Entities;
using Domain.Interfaces;
using FluentValidation;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserContextService _userContextService;
        private readonly IRecipeGeneratorService _recipeGeneratorService;
        private readonly UserManager<AppUser> _userManager;

        public AdminService(AppDbContext dbContext, IUserContextService userContextService, IRecipeGeneratorService recipeGeneratorService, UserManager<AppUser> userManager )
        {
            _dbContext = dbContext;
            _userContextService = userContextService;
            _recipeGeneratorService = recipeGeneratorService;
            _userManager = userManager;
        }

        public async Task<string> EditRolesAsync(string id, string roles)
        {
            if (string.IsNullOrEmpty(roles))
                return "You must select at least one role";

            var selectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return "User not found";

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded)
                return "Failed to add to roles";

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded)
                return "Failed to remove from roles";

            return $"Successfully changed user role to {roles}";
        }

        public async Task<bool> GenerateRandomRecipes(int amount, CancellationToken cancellationToken)
        {
            await _recipeGeneratorService.GenerateAndSaveAsync(amount, cancellationToken);
            return true;
        }

        public async Task<bool> RecalculateRecipes()
        {
            await _recipeGeneratorService.RecalculateAllRecipesAdminAsync();
            return true;
        }

        public async Task<List<UserDetailsForAdminDto>> GetUsersWithRolesAsync()
        {
            var users = await _userManager.Users
                .SelectMany(x => x.UserRoles.Select(r => new UserDetailsForAdminDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    RoleName = r.Role.Name,
                    Budget = (decimal)x.Budget
                }))
                .ToListAsync();

            return users;
        }
    }
}
