using Application.DTO.Recipe;
using Application.Exceptions;
using Application.Query;
using Application.Validators;
using Domain.Entities;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OperationCanceledException = Application.Exceptions.OperationCanceledException;
using ValidationException = FluentValidation.ValidationException;


namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly IValidator<RecipeCreateDto> _validator;
        public RecipesController(IValidator<RecipeCreateDto> validator, IRecipeService recipeService)
        {
            _recipeService = recipeService;
            _validator = validator;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]RecipeCreateDto recipeCreateDto)
        {
            ValidationResult validationResult = await _validator.ValidateAsync(recipeCreateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var isCreated = await _recipeService.CreateAsync(recipeCreateDto);
            return Ok(isCreated);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] RecipeQuery recipeQuery)
        {
            if (recipeQuery.PageNumber < 1) recipeQuery.PageNumber = 1;
            if (recipeQuery.PageSize <= 0) recipeQuery.PageSize = 10 ;

            var recipes = await _recipeService.GetAllAsync(recipeQuery);
            return Ok(recipes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var recipe = await _recipeService.GetByIdAsync(id);
            return recipe != null ? Ok(recipe) : NotFound();
        }

        [HttpGet("{recipeId}/cost")]
        public async Task<IActionResult> GetRecipeCost(int recipeId)
        {
            try
            {
                var cost = await _recipeService.CalculateRecipeCostAsync(recipeId);
                return Ok(cost);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("within-budget")]
        [Authorize]
        public async Task<IActionResult> GetRecipesWithinBudget([FromQuery] RecipeQuery recipeQuery)
        {
            if(recipeQuery.PageNumber < 1) recipeQuery.PageNumber = 1;
            if(recipeQuery.PageSize <= 0) recipeQuery.PageSize= 10 ;
            var recipes = await _recipeService.GetRecipesWithinBudget(recipeQuery);
            return Ok(recipes);
        }

        [HttpGet("can-prepare")]
        [Authorize]
        public async Task<IActionResult> GetRecipesUserCanPrepare([FromQuery] RecipeQuery recipeQuery)
        {
            if(recipeQuery.PageNumber < 1) recipeQuery.PageNumber = 1;         
            if(recipeQuery.PageSize <= 0) recipeQuery.PageSize = 10 ;
            var recipes = await _recipeService.GetRecipesUserCanPrepareAsync(recipeQuery);
            return Ok(recipes);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Put(int id, string description)
        {
            try
            {
                var recipe = await _recipeService.UpdateAsync(id, description);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _recipeService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
        [HttpGet("cheapest")]
        public async Task<ActionResult<decimal>> GetCheapestRecipeCost()
        {
            try
            {
                var cheapestCost = await _recipeService.GetCheapestRecipeCostAsync();
                return Ok(cheapestCost);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
