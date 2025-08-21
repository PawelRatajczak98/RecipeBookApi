using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeFeedbacksService : ControllerBase
    {
        private readonly IRecipeFeedbackService _recipeFeedbackService;
        public RecipeFeedbacksService(IRecipeFeedbackService recipeFeedbackService)
        {
            _recipeFeedbackService = recipeFeedbackService;
        }
        
        [HttpGet]
        [Route("comments")]
        public async Task<IActionResult> GetComments([FromRoute]int recipeId)
        {
            var comments = await _recipeFeedbackService.GetCommentsAsync(recipeId);
            return Ok(comments);
        }

        [HttpPost]
        [Route("comments")]
        public async Task<IActionResult> PostComment([FromRoute]string commentContent, int recipeId)
        {
            var comment = await _recipeFeedbackService.AddCommentAsync(commentContent, recipeId);
            return Ok(comment);
        }

        [HttpDelete]
        [Route("comments")]
        public async Task<IActionResult> DeleteComment([FromRoute] int recipeId)
        {
            var result = await _recipeFeedbackService.DeleteCommentAsync(recipeId);
            return Ok(result);
        }

        [HttpGet]
        [Route("likes")]
        public async Task<IActionResult> GetLikes([FromRoute] int recipeId)
        {
            var likes = await _recipeFeedbackService.GetLikesAsync(recipeId);
            return Ok(likes);
        }

        [HttpPost]
        [Route("likes")]
        public async Task<IActionResult> PostLike([FromRoute] int recipeId)
        {
            var like = await _recipeFeedbackService.AddLikeAsync(recipeId);
            return Ok(like);
        }

        [HttpDelete]
        [Route("likes")]
        public async Task<IActionResult> DeleteLike([FromRoute] int recipeId)
        {
            var result = await _recipeFeedbackService.DeleteLikeAsync(recipeId);
            return Ok(result);
        }
    }
}
