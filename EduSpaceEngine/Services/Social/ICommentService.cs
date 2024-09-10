using EduSpaceEngine.Dto.Social;
using EduSpaceEngine.Model.Social.Request;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Social
{
    public interface ICommentService
    {
        Task<IActionResult> CreateCommentAsync(CommentDto comment, int postid , int userId , string JWTRole);
        Task<IActionResult> GetCommentsAsync(int postId);
        Task<IActionResult> DeleteCommentAsync(int commentId, int userId, string JWTRole);
        Task<IActionResult> EditCommentAsync(CommentDto EditedComment,int commentId, int userId, string JWTRole);
    }
}
