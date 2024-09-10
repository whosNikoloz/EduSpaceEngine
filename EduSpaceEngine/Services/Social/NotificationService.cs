using EduSpaceEngine.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduSpaceEngine.Services.Social
{
    public class NotificationService : INotificationService
    {

        private readonly DataDbContext _db;
        public NotificationService(DataDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> GetUserNotificationsAsync(int validUserId,  int userId, string JWTRole)
        {

            var user = await _db.Users.Include(u => u.Notifications).FirstOrDefaultAsync(u => u.UserId == validUserId);

            if (user == null)
            {
                return new NotFoundObjectResult("User not found");
            }

            if (userId != validUserId)
            {
                if (JWTRole != "admin")
                {
                    return new UnauthorizedObjectResult("Authorize invalid");
                }
            }

            var notifications = user.Notifications.OrderByDescending(n => n.CreatedAt).ToList();
            if(notifications == null)
            {
                return new NotFoundObjectResult("Notifications not found");
            }
            return new OkObjectResult(notifications);
        }

        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null)
            {
                return new NotFoundObjectResult("Notification not found");
            }

            notification.IsRead = true;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("An error occurred while saving the changes.");
            }

            return new OkObjectResult("Notification marked as read");
        }

        public async Task<IActionResult> MarkNotificationsAsRead(int validUserId, int userId, string JWTRole)
        {

            var user = await _db.Users.Include(u => u.Notifications).FirstOrDefaultAsync(u => u.UserId == validUserId);

            if (user == null)
            {
                return new NotFoundObjectResult("User not found");
            }

            if (userId != validUserId)
            {
                if (JWTRole != "admin")
                {
                    return new UnauthorizedObjectResult("Unauthorized");
                }
            }

            if(user.Notifications == null)
            {
                return new NotFoundObjectResult("Notifications not found");
            }

            var notifications = user.Notifications.OrderByDescending(n => n.CreatedAt).ToList();

            try
            {
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("An error occurred while saving the changes.");
            }
            
            return new OkObjectResult("Notifications marked as read");
        }
    }
}
