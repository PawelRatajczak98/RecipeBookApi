using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Exceptions;

namespace Infrastructure.Services
{
    
    public class BudgetService : IBudgetService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;
        public BudgetService(AppDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }
        public async Task<bool> IncreaseBudgetAsync(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ValidationException("Value must be greater than 0");
            }
            var user = await _context.Users.FindAsync(_userContextService.GetUserId());
            if (user == null)
            {
                throw new UnauthorizedException("User not found");
            }
            user.Budget += amount;
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DecreaseBudgetAsync(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ValidationException("Value msut be lower than 0");
            }
            var user = await _context.Users.FindAsync(_userContextService.GetUserId());
           
            if (user == null)
            {
                throw new UnauthorizedException("User not found");
            }
            
            if(user.Budget < amount)
            {
                return false;
            }
            user.Budget -= amount;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal?> GetBudgetAsync()
        {
            var userId = _userContextService.GetUserId();
            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ValidationException("Check user info");
            }
            var budget = user.Budget;
            return budget.HasValue ? budget.Value : 0;
        }

        
    }
}
