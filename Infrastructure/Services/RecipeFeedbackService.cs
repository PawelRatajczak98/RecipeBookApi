using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    
    public class RecipeFeedbackService : IRecipeFeedbackService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;

        public RecipeFeedbackService(AppDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<string> AddCommentAsync(string commentContent, int recipeId)
        {
            if (commentContent == null)
            {
                throw new ValidationException("Empty comment");
            }

            var comment = new Comment
            {
                Content = commentContent,
                RecipeId = recipeId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return $"{commentContent} added to destinated recipe.";
        }

        public async Task<List<Comment>> GetCommentsAsync(int recipeId)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.RecipeId == recipeId)
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<bool> DeleteCommentAsync(int recipeId)
        {
            var userId = _userContextService.GetUserId();
            if (userId is null)
            {
                throw new UnauthorizedException("User not found");
            }
            var comment = await _context.Comments.FindAsync(userId,recipeId);
            if (comment == null)
            {
                throw new ValidationException("Comment not found.");
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> AddLikeAsync(int recipeId)
        {
            var userId = _userContextService.GetUserId();
            if (userId is null)
            {
                throw new UnauthorizedException("User not found");
            }
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.RecipeId == recipeId);
            if (existingLike != null)
            {
                throw new ValidationException("You already liked this recipe");
            }

            var like = new Like
            {
                UserId = userId,
                RecipeId = recipeId
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return "Recipe liked";
        }

        public async Task<List<Like>> GetLikesAsync(int recipeId)
        {
            return await _context.Likes
                .AsNoTracking()
                .Where(l => l.RecipeId == recipeId)
                .Include(l => l.User)
                .ToListAsync();
        }

        public async Task<bool> DeleteLikeAsync(int recipeId)
        {
            var userId = _userContextService.GetUserId();
            if (userId is null)
            {
                throw new UnauthorizedException("User not found");
            }
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.RecipeId == recipeId);
            if (like == null)
            {
                throw new ValidationException("Like not found.");
            }
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
