using Application.DTO.Comment;
using Application.DTO.Like;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRecipeFeedbackService
    {
        Task<CommentDto> AddCommentAsync(string commentContent, int recipeId);
        Task<List<CommentDto>> GetCommentsAsync(int recipeId);
        Task<bool> DeleteCommentAsync(int recipeId);

        Task<string> AddLikeAsync(int recipeId);
        Task<LikeSummaryDto> GetLikesSummaryAsync(int recipeId);
        Task<bool> DeleteLikeAsync(int recipeId);
    }
}
