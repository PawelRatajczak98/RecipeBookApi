using Application.DTO.Comment;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")] // Będzie: api/comments
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly IRecipeFeedbackService _service;

        public CommentsController(IRecipeFeedbackService service)
        {
            _service = service;
        }

        [HttpGet] // GET api/comments?recipeId=5
        public async Task<IActionResult> Get([FromQuery] int recipeId)
        {
            var comments = await _service.GetCommentsAsync(recipeId);
            return Ok(comments);
        }

        [HttpPost] // POST api/comments
        public async Task<IActionResult> Create([FromBody] CommentCreateDto dto)
        {
            try
            {
                var result = await _service.AddCommentAsync(dto.CommentContent, dto.RecipeId);
                return Ok(result);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{recipeId}")] 
        public async Task<IActionResult> Delete(int recipeId)
        {
            var result = await _service.DeleteCommentAsync(recipeId);
            return result ? NoContent() : NotFound();
        }
    }
}
