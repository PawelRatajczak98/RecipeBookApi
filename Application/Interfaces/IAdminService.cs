using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAdminService
    {
        Task<List<UserDetailsForAdminDto>> GetUsersWithRolesAsync();
        Task<bool> GenerateRandomRecipes(int amount, CancellationToken cancellationToken);
        Task<string> EditRolesAsync(string id, string roles);
        Task<bool> RecalculateRecipes();
    }
}
