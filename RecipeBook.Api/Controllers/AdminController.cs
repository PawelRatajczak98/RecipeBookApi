using Application.DTO;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Infrastructure.Services;
using Domain.Interfaces;

namespace Api.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager, IAdminService adminService)
        {
            _userManager = userManager;
            _adminService = adminService; 
        }

        [HttpGet("users-with-roles")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetUsersByNameWithRolesAsync()
        {
            var users = await _adminService.GetUsersWithRolesAsync();
            return Ok(users);
        }


        [HttpPost("edit-roles")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> EditRolesAsync(string id, string roles)
        {
            var result = await _adminService.EditRolesAsync(id, roles);
            return Ok(result);
        }


        [HttpPost("generate-recipes")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GenerateRecipesAndSave(int amount, CancellationToken cancellationToken)
        {
            if (amount <= 0 || amount > 100_000)
            {
                throw new ValidationException("Amount must be between 1 and 100000.");
            }

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(550));

            try
            {
                await _adminService.GenerateRandomRecipes(amount, cancellationToken);
                return NoContent();
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, "Out of time");
            }

        }

        [HttpGet("recalculate-cost")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> RecalculateRecipeCosts()
        {
            var result = await _adminService.RecalculateRecipes();
            return Ok(result);
        }
    }
}
