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
        Task<string> AddCommentAsync(string commentContent, int recipeId);
        Task<string> AddLikeAsync(int recipeId);
        Task<List<Comment>> GetCommentsAsync(int recipeId);
        Task<List<Like>> GetLikesAsync(int recipeId);
        Task<bool> DeleteCommentAsync(int recipeId);
        Task<bool> DeleteLikeAsync(int recipeId);
    }
}
