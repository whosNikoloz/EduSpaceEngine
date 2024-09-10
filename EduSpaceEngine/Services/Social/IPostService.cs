using EduSpaceEngine.Dto.User;
using EduSpaceEngine.Dto;
using EduSpaceEngine.Model;
using Microsoft.AspNetCore.Mvc;
using EduSpaceEngine.Model.Social.Request;
using EduSpaceEngine.Dto.Social;

namespace EduSpaceEngine.Services.Social
{
    public interface  IPostService
    {
        string Test();
        Task<IActionResult> GetPostsAsync(int page, int pageSize);
        Task<object> GetLastPostAsync();
        Task<IActionResult> GetPostAsync(int postId);
        Task<IActionResult> GetPostsWithSubjectAsync(string subject, int page, int pageSize);
        Task<IActionResult> CreatePostAsync(PostDto post, int userId, string JWTRole);
        Task<IActionResult> EditPostAsync(PostDto editedPost,int postId, int userId, string JWTRole);
        Task<IActionResult> DeletePostAsync(int postId, int userId, string JWTRole);
    }
}
