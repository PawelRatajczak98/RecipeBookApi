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
                throw new ValidationException("Pusta zawartość");
            }

            var userId = _userContextService.GetUserId();
            if (userId is null)
            {
                throw new UnauthorizedException("Nie znaleziono użytkownika");
            }

            var comment = new Comment
            {
                Content = commentContent,
                RecipeId = recipeId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
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
                throw new UnauthorizedException("Nie znaleziono użytkownika");
            }
            // Klucz kompozytowy: RecipeId (int), UserId (string) - kolejność zgodna z HasKey w AppDbContext
            var comment = await _context.Comments.FindAsync(recipeId, userId);
            if (comment == null)
            {
                throw new ValidationException("Nie znaleziono komentarza");
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
                throw new UnauthorizedException("Nie znaleziono użytkownika");
            }
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.RecipeId == recipeId);
            if (existingLike != null)
            {
                throw new ValidationException("Już polubiłeś ten przepis");
            }

            var like = new Like
            {
                UserId = userId,
                RecipeId = recipeId
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return "Polubiono przepis";
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
                throw new UnauthorizedException("Nie znaleziono użytkownika");
            }
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.RecipeId == recipeId);
            if (like == null)
            {
                throw new ValidationException("Nie znaleziono polubienia");
            }
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
