using Application.DTO.Comment;
using Application.DTO.Like;
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

        public async Task<CommentDto> AddCommentAsync(string commentContent, int recipeId)
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

            var exists = await _context.Comments.AnyAsync(c => c.RecipeId == recipeId && c.UserId == userId);
            if (exists) throw new ValidationException("Już skomentowałeś ten przepis.");

            var comment = new Comment
            {
                Content = commentContent,
                RecipeId = recipeId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var userName = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();

            return new CommentDto
            {
                Content = comment.Content,
                UserName = userName,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<CommentDto>> GetCommentsAsync(int recipeId)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.RecipeId == recipeId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Content = c.Content,
                    UserName = c.User.UserName,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteCommentAsync(int recipeId)
        {
            var userId = _userContextService.GetUserId();
            if (userId is null)
            {
                throw new UnauthorizedException("Nie znaleziono użytkownika");
            }

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

        public async Task<LikeSummaryDto> GetLikesSummaryAsync(int recipeId)
        {
            var userId = _userContextService.GetUserId();
            var count = await _context.Likes.CountAsync(l => l.RecipeId == recipeId);

            var isLikedByMe = false;
            if (userId != null)
            {
                isLikedByMe = await _context.Likes
                    .AnyAsync(l => l.RecipeId == recipeId && l.UserId == userId);
            }

            return new LikeSummaryDto
            {
                TotalLikes = count,
                LikedByCurrentUser = isLikedByMe
            };
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
