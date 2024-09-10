using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Social;
using EduSpaceEngine.Model.Social;
using EduSpaceEngine.Model.Social.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduSpaceEngine.Services.Social
{
    public class SocialService : ISocialService
    {
        readonly DataDbContext _db;
        public SocialService(DataDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> CreatePostAsync(PostDto post, int userId, string JWTRole)
        {
            var user = await _db.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId == userId);  //მაძლევს Users პოსტებით

            if (user == null)
            {
                return new NotFoundObjectResult("user not found.");
            }
            if (userId != user.UserId)
            {
                if (JWTRole != "admin")
                {
                    return new UnauthorizedObjectResult("Authorize invalid");
                }
            }

            var NewPost = new PostModel
            {
                Subject = post.Subject,
                Content = post.Content,
                Video = post.Video,
                Picture = post.Picture,
                CreateDate = DateTime.Now,
                UserId = userId,
                User = user,
            };

            user.Posts ??= new List<PostModel>();
            user.Posts.Add(NewPost);

            try
            {
                _db.Posts.Add(NewPost);
                await _db.SaveChangesAsync();
            }catch(Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            return new OkObjectResult(NewPost);
        }

        public async Task<IActionResult> DeletePostAsync(int postId, int userId, string JWTRole)
        {

            var post = await _db.Posts.Include(u => u.User).FirstOrDefaultAsync(u => u.PostId == postId); // Fetch the post

            if (post == null)
            {
                return new NotFoundObjectResult("post not found.");
            }

            var user = await _db.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId == post.User.UserId);

            if (user == null)
            {
                return new NotFoundObjectResult("user not found.");
            }

            if (userId != user.UserId)
            {
                if (JWTRole != "admin")
                {
                    return new BadRequestObjectResult("Authorize invalid");
                }
            }



            // Retrieve comments associated with the post
            //var comments = await _db.Comments.Where(c => c.Post.PostId == postId).ToListAsync();

            // Remove the comments
            //_db.Comments.RemoveRange(comments);


            // Retrieve notifications associated with the post
            // var notifications = await _db.Notifications.Where(n => n.PostId == postId).ToListAsync();

            //_db.Notifications.RemoveRange(notifications);

            try
            {
                // Remove the post
                user.Posts.Remove(post);
                _db.Posts.Remove(post);

                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }

            return new OkObjectResult("Successfully Delete");
        }

        public async Task<IActionResult> EditPostAsync(PostDto editedPost,int postid, int userId, string JWTRole)
        {

            var post = await _db.Posts.Include(u => u.User).FirstOrDefaultAsync(u => u.PostId == postid); // Fetch the post

            if (post == null)
            {
                return new NotFoundObjectResult("post not found.");
            }

            var user = await _db.Users.Include(u => u.Posts).FirstOrDefaultAsync(u => u.UserId == post.User.UserId);

            if (user == null)
            {
                return new NotFoundObjectResult("user not found.");
            }

            if (userId != user.UserId)
            {
                if (JWTRole != "admin")
                {
                    return new BadRequestObjectResult("Authorize invalid");
                }
            }


            try
            {
                user.Posts.Remove(post);



                post.Subject = editedPost.Subject;
                post.Content = editedPost.Content;
                post.Video = editedPost.Video;
                post.Picture = editedPost.Picture;
                post.CreateDate = DateTime.Now;
                post.UserId = userId;
                post.User = user;


                user.Posts ??= new List<PostModel>();
                user.Posts.Add(post);

                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            return new OkObjectResult(post);
        }

        public async Task<object> GetLastPostAsync()
        {
            var lastPost = await _db.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User) // Include the user for each comment
                .OrderByDescending(p => p.CreateDate)
                .FirstOrDefaultAsync();

            if (lastPost == null)
            {
                return new NotFoundObjectResult("lastPost not found.");
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

            if(postResponse == null)
            {
                return new NotFoundObjectResult("postResponse not found.");
            }

            return new OkObjectResult(postResponse);
        }

        public async Task<IActionResult> GetPostAsync(int postId)
        {
            var post = await _db.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User) // Include the user for each comment
                .FirstOrDefaultAsync(p => p.PostId == postId);
            if(post == null)
            {
                return new NotFoundObjectResult("Post not found.");
            }
            return new OkObjectResult(post);
        }

        public async Task<IActionResult> GetPostsAsync(int page, int pageSize)
        {
            var query = _db.Posts
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
            if(responseList.Count == 0)
            {
                return new NotFoundObjectResult("Posts not found.");
            }

            return new OkObjectResult(responseList);
        }

        public async Task<IActionResult> GetPostsWithSubjectAsync(string subject, int page, int pageSize)
        {
            var query = _db.Posts
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

            return new OkObjectResult(responseList);
        }

        public string Test()
        {
            return "Hello World";
        }
    }
}
