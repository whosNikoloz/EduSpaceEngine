using EduSpaceEngine.Model.Social.Request;
using EduSpaceEngine.Model.Social;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduSpaceEngine.Data;
using Microsoft.EntityFrameworkCore;
using EduSpaceEngine.Model.Learn.Test;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using EduSpaceEngine.Hubs;
using System.Collections.Concurrent;
using Asp.Versioning;

namespace EduSpaceEngine.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SocialController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _NothubContext;
        private readonly IHubContext<CommentHub> _ComhubContext;

        public SocialController(DataDbContext context, IConfiguration configuration, IHubContext<NotificationHub> NothubContext, IHubContext<CommentHub> ComhubContext)
        {
            _configuration = configuration;
            _context = context;
            _NothubContext = NothubContext;
            _ComhubContext = ComhubContext;
        }


        private readonly ConcurrentDictionary<string, string> userConnectionMap = NotificationHub.userConnectionMap;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> userPostConnectionMap = CommentHub.userPostConnectionMap;

        // ---------- Posts ----------

        /// <summary>
        /// მიიღეთ ყველა პოსტის სია.
        /// </summary>

        [HttpGet("Posts")]
        public async Task<ActionResult<IEnumerable<object>>> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User) // Include the user for each comment
                .OrderByDescending(p => p.CreateDate) // Sort by creation date in descending order
                .AsQueryable();

            // Calculate the number of items to skip
            int skip = (page - 1) * pageSize;

            // Apply pagination
            var posts = await query.Skip(skip).Take(pageSize).ToListAsync();

            var responseList = new List<object>();

            foreach (var post in posts)
            {
                var postResponse = new
                {
                    postId = post.PostId,
                    content = post.Content,
                    video = post.Video,
                    picture = post.Picture,
                    subject = post.Subject,
                    createdAt = post.CreateDate,
                    user = new
                    {
                        userId = post.User.UserId,
                        firstname = post.User.FirstName,
                        lastname = post.User.LastName,
                        username = post.User.UserName,
                        picture = post.User.Picture,
                    },
                    comments = post.Comments.Select(comment => new
                    {
                        commentId = comment.CommentId,
                        commentContent = comment.Content,
                        commentPicture = comment.Picture,
                        commentVideo = comment.Video,
                        commentCreatedAt = comment.CreatedAt,
                        commentUser = new
                        {
                            userId = comment.User.UserId,
                            firstname = comment.User.FirstName,
                            lastname = comment.User.LastName,
                            username = comment.User.UserName,
                            picture = comment.User.Picture,
                        }
                    }).ToList()
                };

                responseList.Add(postResponse);
            }

            return Ok(responseList);
        }




        [HttpGet("LastPost")]
        public async Task<ActionResult<object>> GetLastPost()
        {
            var lastPost = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User) // Include the user for each comment
                .OrderByDescending(p => p.CreateDate)
                .FirstOrDefaultAsync();

            if (lastPost == null)
            {
                return NotFound(); // No posts found
            }

            var postResponse = new
            {
                postId = lastPost.PostId,
                content = lastPost.Content,
                video = lastPost.Video,
                picture = lastPost.Picture,
                subject = lastPost.Subject,
                createdAt = lastPost.CreateDate,
                user = new
                {
                    userId = lastPost.User.UserId,
                    firstname = lastPost.User.FirstName,
                    lastname = lastPost.User.LastName,
                    username = lastPost.User.UserName,
                    picture = lastPost.User.Picture,
                },
                comments = lastPost.Comments.Select(comment => new
                {
                    commentId = comment.CommentId,
                    commentContent = comment.Content,
                    commentPicture = comment.Picture,
                    commentVideo = comment.Video,
                    commentCreatedAt = comment.CreatedAt,
                    commentUser = new
                    {
                        userId = comment.User.UserId,
                        firstname = comment.User.FirstName,
                        lastname = comment.User.LastName,
                        username = comment.User.UserName,
                        picture = comment.User.Picture,
                    }
                }).ToList()
            };

            return Ok(postResponse);
        }




        /// <summary>
        /// მიიღეთ კონკრეტული პოსტი მისი ID-ით.
        /// </summary>
        /// <param name="postId">მოსაბრუნებელი პოსტის ID.</param>
        [HttpGet("Post/{postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
            return Ok(await _context.Posts.FirstOrDefaultAsync(u => u.PostId == postId));

        }

        /// <summary>
        /// მიიღეთ პოსტები კონკრეტული თემით.
        /// </summary>
        /// <param name="subject">პოსტების გაფილტვრის საგანი.</param>
        [HttpGet("Posts/{subject}")]
        public async Task<IActionResult> PostsWithSubjeect(string subject, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.Posts
                .Where(u => u.Subject == subject)
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User) // Include the user for each comment
                .OrderByDescending(p => p.CreateDate) // Sort by creation date in descending order
                .AsQueryable();
            int skip = (page - 1) * pageSize;

            // Apply pagination
            var posts = await query.Skip(skip).Take(pageSize).ToListAsync();

            var responseList = new List<object>();

            foreach (var post in posts)
            {
                var postResponse = new
                {
                    postId = post.PostId,
                    content = post.Content,
                    video = post.Video,
                    picture = post.Picture,
                    subject = post.Subject,
                    createdAt = post.CreateDate,
                    user = new
                    {
                        userId = post.User.UserId,
                        firstname = post.User.FirstName,
                        lastname = post.User.LastName,
                        username = post.User.UserName,
                        picture = post.User.Picture,
                    },
                    comments = post.Comments.Select(comment => new
                    {
                        commentId = comment.CommentId,
                        commentContent = comment.Content,
                        commentPicture = comment.Picture,
                        commentVideo = comment.Video,
                        commentCreatedAt = comment.CreatedAt,
                        commentUser = new
                        {
                            userId = comment.User.UserId,
                            firstname = comment.User.FirstName,
                            lastname = comment.User.LastName,
                            username = comment.User.UserName,
                            picture = comment.User.Picture,
                        }
                    }).ToList()
                };

                responseList.Add(postResponse);
            }

            return Ok(responseList);

        }

        /// <summary>
        /// შექმენით ახალი პოსტი მომხმარებლისთვის.
        /// </summary>
        /// <param name="Post">პოსტის ინფორმაციის შექმნა.</param>
        /// <param name="Userid">პოსტის შემქმნელი მომხმარებლის ID.</param>
        [HttpPost("Posts/"), Authorize]
        public async Task<IActionResult> CreatePost(PostRequestModel Post)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _context.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId.ToString() == userId);  //მაძლევს Users პოსტებით

            if (user == null)
            {
                return BadRequest("User Not Found");
            }
            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }

            var NewPost = new PostModel
            {
                Subject = Post.Subject,
                Content = Post.Content,
                Video = Post.Video,
                Picture = Post.Picture,
                CreateDate = DateTime.Now,
                User = user,
            };

            user.Posts ??= new List<PostModel>();
            user.Posts.Add(NewPost);

            _context.Posts.Add(NewPost);
            await _context.SaveChangesAsync();



            return Ok(NewPost);
        }

        /// <summary>
        /// არსებული პოსტის რედაქტირება.
        /// </summary>
        /// <param name="EditedPost">რედაქტირებული პოსტის ინფორმაცია.</param>
        [HttpPut("Posts"), Authorize]
        public async Task<IActionResult> EditPost(EditPostRequestModel EditedPost)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var post = await _context.Posts.Include(u => u.User).FirstOrDefaultAsync(u => u.PostId == EditedPost.PostId);  //მაძლევს Users პოსტებით

            if (post == null)
            {
                return NotFound("Post Not Found");
            }


            var user = await _context.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId == post.User.UserId);  //მაძლევს Users პოსტებით



            if (user == null)
            {
                return BadRequest("User Not Found");
            }

            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }


            user.Posts.Remove(post);



            post.Subject = EditedPost.Subject;
            post.Content = EditedPost.Content;
            post.Video = EditedPost.Video;
            post.Picture = EditedPost.Picture;
            post.CreateDate = DateTime.Now;
            post.User = user;



            user.Posts ??= new List<PostModel>();
            user.Posts.Add(post);

            await _context.SaveChangesAsync();



            return Ok(post);
        }

        /// <summary>
        /// პოსტის წაშლა მისი ID-ით.
        /// </summary>
        /// <param name="postId">წაშლილი პოსტის ID.</param>
        [HttpDelete("Posts/{postId}"), Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // JWT id check
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; // JWT Role

            var post = await _context.Posts.Include(u => u.User).FirstOrDefaultAsync(u => u.PostId == postId); // Fetch the post

            if (post == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId == post.User.UserId);

            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }

            if (user == null)
            {
                return NotFound();
            }

            // Retrieve comments associated with the post
            var comments = await _context.Comments.Where(c => c.Post.PostId == postId).ToListAsync();

            // Remove the comments
            _context.Comments.RemoveRange(comments);


            // Retrieve notifications associated with the post
            var notifications = await _context.Notifications.Where(n => n.PostId == postId).ToListAsync();

            _context.Notifications.RemoveRange(notifications);

            // Remove the post
            user.Posts.Remove(post);
            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();

            return Ok("Successfully Deleted");
        }



        // ---------- Comments ----------


        /// <summary>
        /// შექმენით ახალი კომენტარი პოსტისთვის.
        /// </summary>
        /// <param name="comment">შესამუშავებელი კომენტარის ინფორმაცია.</param>
        /// <param name="postid">პოსტის ID, რომელთანაც ასოცირდება კომენტარი.</param>
        /// <param name="userid">კომენტარის შემქმნელი მომხმარებლის ID.</param>
        [HttpPost("Comments/{postid}"), Authorize]
        public async Task<IActionResult> CreateComment(CommentRequestModel comment, int postid)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id check
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = await _context.Posts.Include(u => u.User).FirstOrDefaultAsync(u => u.PostId == postid);

            if (post == null)
            {
                return NotFound(); // Handle if the post doesn't exist
            }

            var user = await _context.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return NotFound(); // Handle if the user doesn't exist
            }

            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Unauthorized");
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

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            // Get the newly created comment from the database with its ID
            var savedComment = await _context.Comments
                .Include(c => c.User) // Assuming there's a navigation property to the user who made the comment
                .FirstOrDefaultAsync(c => c.CommentId == newComment.CommentId);

            if (savedComment == null)
            {
                return NotFound();
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

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(savedComment);
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


        /// <summary>
        /// მიიღეთ კომენტარები კონკრეტული პოსტისთვის.
        /// </summary>
        /// <param name="postId">პოსტის ID კომენტარის მისაღებად.</param>
        [HttpGet("Comments/{postId}")]
        public async Task<IActionResult> GetComments(int postId)
        {

            var comments = await _context.Comments.Where(c => c.Post.PostId == postId).Include(p => p.User).ToListAsync();

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



            return Ok(responseList);

        }



        /// <summary>
        /// წაშალეთ კომენტარი მისი ID-ით და მომხმარებლის ID-ით.
        /// </summary>
        /// <param name="commentId">წაშლილი კომენტარის ID.</param>
        /// <param name="userId">კომენტარს წაშლის მომხმარებლის ID.</param>
        [HttpDelete("Comments/{commentId}"), Authorize]
        public async Task<IActionResult> DeleteComments(int commentId)
        {
            var UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var comment = await _context.Comments.Include(p => p.User).FirstOrDefaultAsync(p => p.CommentId == commentId);

            if (comment.User.UserId.ToString() != UserId)
            {
                return NotFound();
            }


            // Remove associated notifications
            var relatedNotifications = await _context.Notifications
                .Where(n => n.User.UserId.ToString() == UserId && n.Message.Contains($"comment {comment.CommentId}"))
                .ToListAsync();

            _context.Notifications.RemoveRange(relatedNotifications);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok("Removed Successfully");
        }

        /// <summary>
        /// არსებული კომენტარის რედაქტირება.
        /// </summary>
        /// <param name="EditedComment">რედაქტირებული კომენტარის ინფორმაცია.</param>
        [HttpPut("Comments"), Authorize]
        public async Task<IActionResult> EditCommentar(EditCommentReqeustModel EditedComment)
        {


            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var comment = await _context.Comments.Include(u => u.User).FirstOrDefaultAsync(u => u.CommentId == EditedComment.CommentId);  //მაძლევს Users Commentars

            if (comment == null)
            {
                return NotFound("Post Not Found");
            }


            var user = await _context.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId == comment.User.UserId);  //მაძლევს Users პოსტებით



            if (user == null)
            {
                return BadRequest("User Not Found");
            }

            if (userId != user.UserId.ToString() || JWTRole != "admin")
            {
                return BadRequest("Authorize invalid");
            }


            user.Comments.Remove(comment);



            comment.Content = EditedComment.Content;
            comment.Video = EditedComment.Video;
            comment.Picture = EditedComment.Picture;
            comment.CreatedAt = DateTime.Now;
            comment.User = user;



            user.Comments ??= new List<CommentModel>();
            user.Comments.Add(comment);

            await _context.SaveChangesAsync();



            return Ok(comment);
        }


        // --------Notifications




        /// <summary>
        /// მიიღეთ შეტყობინებები კონკრეტული მომხმარებლისთვის.
        /// </summary>
        /// <param name="userId">მომხმარებლის ID, რომლისთვისაც უნდა მიიღოთ შეტყობინებები.</param>
        [HttpGet("Notifications/{userId}"), Authorize]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var user = await _context.Users.Include(u => u.Notifications).FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (UserId != userId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }

            var notifications = user.Notifications.OrderByDescending(n => n.CreatedAt).ToList();
            return Ok(notifications);
        }


        [HttpPut("MarkNotificationsAsRead/{userId}"), Authorize]
        public async Task<IActionResult> MarkNotificationsAsRead(int userId)
        {
            var requestingUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var requestingUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            var user = await _context.Users.Include(u => u.Notifications).FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (requestingUserId != userId.ToString())
            {
                if (requestingUserRole != "admin")
                {
                    return BadRequest("Unauthorized");
                }
            }

            var notifications = user.Notifications.OrderByDescending(n => n.CreatedAt).ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(notifications);
        }

        /// <summary>
        /// მონიშნეთ შეტყობინება წაკითხულად მისი ID-ით.
        /// </summary>
        /// <param name="notificationId">შეტყობინებების ID, რომ მოინიშნოს წაკითხულად.</param>
        [HttpPut("Notifications/{notificationId}")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null)
            {
                return NotFound("Notification not found");
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok("Notification marked as read");
        }
    }
}
