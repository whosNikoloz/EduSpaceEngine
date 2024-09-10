using EduSpaceEngine.Model.Social.Request;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Social
{
    public interface INotificationService
    {
        Task<IActionResult> GetUserNotificationsAsync(int validUserId,  int userId, string JWTRole);
        Task<IActionResult> MarkNotificationsAsRead(int validUserId, int userId, string JWTRole);
        Task<IActionResult> MarkNotificationAsRead(int notificationId);
    }
}
