using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Social;
using EduSpaceEngine.Hubs;
using EduSpaceEngine.Model.Social;
using EduSpaceEngine.Model.Social.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace EduSpaceEngine.Services.Social
{
    public class CommentService : ICommentService
    {
        private readonly DataDbContext _db;
        private readonly IHubContext<CommentHub> _ComhubContext;
        private readonly IHubContext<NotificationHub> _NothubContext;
        public CommentService(DataDbContext db, IHubContext<NotificationHub> NothubContext, IHubContext<CommentHub> comhubContext)
        {
            _NothubContext = NothubContext;
            _db = db;
            _ComhubContext = comhubContext;

        }

        private readonly ConcurrentDictionary<string, string> userConnectionMap = NotificationHub.userConnectionMap;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> userPostConnectionMap = CommentHub.userPostConnectionMap;
        public async Task<IActionResult> CreateCommentAsync(CommentDto comment, int postid, int userId, string JWTRole)
        {
            var post = await _db.Posts.Include(u => u.User).FirstOrDefaultAsync(u => u.PostId == postid);

            if (post == null)
            {
                return new NotFoundObjectResult("Post Not Found"); // Handle if the post doesn't exist
            }

            var user = await _db.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return new NotFoundObjectResult("User Not Found");
            }

            if (userId != user.UserId)
            {
                if (JWTRole != "admin")
                {
                    return new UnauthorizedObjectResult("Unauthorized");
                }
            }

            var newComment = new CommentModel
            {
                Content = comment.Content,
                Picture = comment.Picture,
                Video = comment.Video,
                CreatedAt = DateTime.Now,
                Post = post,
                User = user,
            };

            _db.Comments.Add(newComment);
            await _db.SaveChangesAsync();

            // Get the newly created comment from the database with its ID
            var savedComment = await _db.Comments
                .Include(c => c.User) // Assuming there's a navigation property to the user who made the comment
                .FirstOrDefaultAsync(c => c.CommentId == newComment.CommentId);

            if (savedComment == null)
            {
                return new NotFoundObjectResult("savedComment Not Found");
            }

            var mappedComment = new
            {
                commentId = savedComment.CommentId,
                commentContent = savedComment.Content,
                commentPicture = savedComment.Picture,
                commentVideo = savedComment.Video,
                commentCreatedAt = savedComment.CreatedAt,
                commentUser = new
                {
                    userId = savedComment.User.UserId,
                    firstname = savedComment.User.FirstName,
                    lastname = savedComment.User.LastName,
                    username = savedComment.User.UserName,
                    picture = savedComment.User.Picture,
                }
            };


            // Construct notification
            var notification = new NotificationModel
            {
                Message = $"{post.Content}",
                CreatedAt = DateTime.Now,
                IsRead = false,
                PostId = postid,
                CommentAuthorUsername = user.UserName,
                CommentAuthorPicture = user.Picture,
                User = post.User
            };

            // Send notification to post owner if they are connected
            var connectedPostOwnerUser = GetConnectionIdForUserId(post.User.UserId.ToString());
            if (connectedPostOwnerUser != null)
            {
                await _NothubContext.Clients.Client(connectedPostOwnerUser).SendAsync("ReceiveNotification", notification);
            }

            // Send notification to all connected users on the post
            var connectedUsersOnPost = GetUserIdsAndConnectionIdsForPostId(postid.ToString());
            if (connectedUsersOnPost != null)
            {
                foreach (var u in connectedUsersOnPost)
                {
                    var connectionId = u.Item2;
                    await _ComhubContext.Clients.Client(connectionId).SendAsync("ReceiveComment", mappedComment);
                }
            }
            try
            {
                _db.Notifications.Add(notification);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }


            return new OkObjectResult(savedComment);
        }

        private string GetConnectionIdForUserId(string userId)
        {
            return userConnectionMap.TryGetValue(userId, out var connectionId) ? connectionId : null;
        }

        private List<(string userId, string connectionId)> GetUserIdsAndConnectionIdsForPostId(string postId)
        {
            List<(string userId, string connectionId)> userConnections = new List<(string userId, string connectionId)>();

            foreach (var userDict in userPostConnectionMap)
            {
                if (userDict.Value.ContainsKey(postId))
                {
                    // Get the connection ID for the user ID
                    string connectionId = GetConnectionIdForUserIdCom(userDict.Key);

                    userConnections.Add((userDict.Key, connectionId));
                }
            }

            return userConnections;
        }

        private string GetConnectionIdForUserIdCom(string userId)
        {
            if (userPostConnectionMap.TryGetValue(userId, out ConcurrentDictionary<string, string> connectionMap))
            {
                if (connectionMap.Count > 0)
                {
                    return connectionMap.Values.First();
                }
            }
            return null;
        }

        public async Task<IActionResult> DeleteCommentAsync(int commentId, int userId, string JWTRole)
        {

            var comment = await _db.Comments.Include(p => p.User).FirstOrDefaultAsync(p => p.CommentId == commentId);

            if(comment == null)
            {
                return new NotFoundObjectResult("Comment not found");
            }

            if (comment.User.UserId != userId)
            {
                if(JWTRole != "admin")
                {
                    return new UnauthorizedObjectResult("Unauthorized");
                }
            }

            // Remove associated notifications
            var relatedNotifications = await _db.Notifications
                .Where(n => n.User.UserId == userId && n.Message.Contains($"comment {comment.CommentId}"))
                .ToListAsync();

            try
            {
                _db.Notifications.RemoveRange(relatedNotifications);
                _db.Comments.Remove(comment);
                await _db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult("An error occurred while saving the changes.");
            }
           
            return new OkObjectResult("Removed Successfully");
        }

        public async Task<IActionResult> EditCommentAsync(CommentDto EditedComment,int commentId, int userId, string JWTRole)
        {
            var comment = await _db.Comments.Include(u => u.User).FirstOrDefaultAsync(u => u.CommentId == commentId);  //მაძლევს Users Commentars

            if (comment == null)
            {
                return new NotFoundObjectResult("Post Not Found");
            }


            var user = await _db.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId == comment.User.UserId);  //მაძლევს Users პოსტებით



            if (user == null)
            {
                return new NotFoundObjectResult("User Not Found");
            }

            if (userId != user.UserId || JWTRole != "admin")
            {
                return new UnauthorizedObjectResult("Authorize invalid");
            }


            user.Comments.Remove(comment);



            comment.Content = EditedComment.Content;
            comment.Video = EditedComment.Video;
            comment.Picture = EditedComment.Picture;
            comment.CreatedAt = DateTime.Now;
            comment.User = user;



            user.Comments ??= new List<CommentModel>();
            user.Comments.Add(comment);

            try
            {
                await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("An error occurred while saving the changes.");
            }

            return new OkObjectResult(comment);
        }

        public async Task<IActionResult> GetCommentsAsync(int postId)
        {
            var comments = await _db.Comments.Where(c => c.Post.PostId == postId).Include(p => p.User).ToListAsync();

            var responseList = new List<object>();


            foreach (var comment in comments)
            {
                var postResponse = new
                {
                    commentid = comment.CommentId,
                    content = comment.Content,
                    video = comment.Video,
                    picture = comment.Picture,
                    createdAt = comment.CreatedAt,
                    user = new
                    {
                        userId = comment.User.UserId,
                        username = comment.User.UserName,
                        picture = comment.Picture,
                    },
                };

                responseList.Add(postResponse);
            }

            return new OkObjectResult(responseList);
        }
    }
}
