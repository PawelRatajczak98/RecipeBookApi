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
                throw new ValidationException("Wartość musi być większa niż 0");
            }
            var user = await _context.Users.FindAsync(_userContextService.GetUserId());
            if (user == null)
            {
                throw new UnauthorizedException("Nie znaleziono użytkownika");
            }
            user.Budget += amount;
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DecreaseBudgetAsync(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ValidationException("Wartość musi być mniejsza niż 0");
            }
            var user = await _context.Users.FindAsync(_userContextService.GetUserId());
           
            if (user == null)
            {
                throw new UnauthorizedException("Nie znaleziono użytkownika");
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
                throw new ValidationException("Sprawdź informacje użytkownika");
            }
            var budget = user.Budget;
            return budget.HasValue ? budget.Value : 0;
        }

       public async Task <bool> SetBudgetAsync(decimal amount)
        {
            if (amount < 0)
            {
                throw new ValidationException("Wartość powinna być większa od 0");
            }

            var userId = _userContextService.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new UnauthorizedException("Nie znaleziono użytkownika");
            }

            user.Budget = amount;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
