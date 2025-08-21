using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Domain.Interfaces;
using Application.DTO.UserIngredient;
using FluentValidation;

namespace Api.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserIngredientsController : ControllerBase
    {
        private readonly IUserIngredientService _userIngredientService;
        private readonly IValidator<UserIngredientCreateDto> _createValidator;
        private readonly IValidator<UserIngredientUpdateDto> _updateValidator;

        public UserIngredientsController(IUserIngredientService userIngredientService
            , IValidator<UserIngredientCreateDto> createValidator
            , IValidator<UserIngredientUpdateDto> updateValidator)
        {
            _userIngredientService = userIngredientService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userIngredientService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserIngredientCreateDto userIngredientCreateDto)
        {
            var validationResult = await _createValidator.ValidateAsync(userIngredientCreateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var wasCreated = await _userIngredientService.CreateAsync(userIngredientCreateDto);
            return wasCreated ? Created("/api/useringredients", userIngredientCreateDto) : BadRequest("Failed to create user ingredient");
        }

        [HttpPatch("{ingredientId}")]
        public async Task<IActionResult> Patch(int ingredientId, [FromBody] UserIngredientUpdateDto userIngredientUpdateDto)
        {
            if (ingredientId != userIngredientUpdateDto.IngredientId)
            {
                return BadRequest("Route id does not match body id");
            }

            var validationResult = await _updateValidator.ValidateAsync(userIngredientUpdateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var result = await _userIngredientService.UpdateAsync(ingredientId,userIngredientUpdateDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _userIngredientService.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
