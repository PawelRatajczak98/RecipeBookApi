using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [HttpGet("budget")]
        public async Task<IActionResult> GetBudget()
        {
            var budget = await _budgetService.GetBudgetAsync();
            return Ok(budget);
        }

        [HttpPost("increase")]
        public async Task<IActionResult> IncreaseBudget([FromBody]decimal amount)
        {
            var budget = await _budgetService.IncreaseBudgetAsync(amount);
            return Ok(budget);
        }

        [HttpPost("decrease")]
        public async Task<IActionResult> DecreaseBudget([FromBody]decimal amount)
        {
            var budget = await _budgetService.DecreaseBudgetAsync(amount);
            return Ok(budget);
        }
       
    }
}
