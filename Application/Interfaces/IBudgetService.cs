using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Domain.Interfaces
{
    public interface IBudgetService
    {     
            Task<bool> IncreaseBudgetAsync(decimal amount);
            Task<bool> DecreaseBudgetAsync(decimal amount);
            Task<decimal?> GetBudgetAsync();
    }
}
