using Application.DTO.Ingredient;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientController : ControllerBase
    {
        private readonly IIngredientService _ingredientService;

        public IngredientController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var ingredients = await _ingredientService.GetAllAsync();
            return Ok(ingredients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var ingredient = await _ingredientService.GetByIdAsync(id);
            return ingredient != null ? Ok(ingredient) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]IngredientCreateDto ingredient)
        {
            var createdIngredient = await _ingredientService.CreateAsync(ingredient);
            return Created($"/api/ingredients/{createdIngredient.Id}", createdIngredient);
        }


        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _ingredientService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
