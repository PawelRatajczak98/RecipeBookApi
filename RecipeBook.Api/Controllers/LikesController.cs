using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")] // Będzie: api/likes
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly IRecipeFeedbackService _service;

        public LikesController(IRecipeFeedbackService service)
        {
            _service = service;
        }

        [HttpGet("{recipeId}")] // GET api/likes/5
        public async Task<IActionResult> Get(int recipeId)
        {
            var summary = await _service.GetLikesSummaryAsync(recipeId);
            return Ok(summary);
        }

        [HttpPost("{recipeId}")] // POST api/likes/5
        public async Task<IActionResult> ToggleLike(int recipeId)
        {
            try
            {
                var result = await _service.AddLikeAsync(recipeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{recipeId}")] // DELETE api/likes/5
        public async Task<IActionResult> RemoveLike(int recipeId)
        {
            var result = await _service.DeleteLikeAsync(recipeId);
            return result ? NoContent() : NotFound();
        }
    }
}