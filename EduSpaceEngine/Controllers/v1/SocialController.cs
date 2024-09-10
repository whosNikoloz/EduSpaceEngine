using EduSpaceEngine.Model.Social.Request;
using EduSpaceEngine.Model.Social;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using EduSpaceEngine.Hubs;
using System.Collections.Concurrent;
using Asp.Versioning;
using EduSpaceEngine.Services.Social;
using EduSpaceEngine.Dto.Social;
using Azure;
using EduSpaceEngine.Dto;
using GreenDonut;

namespace EduSpaceEngine.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SocialController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _NothubContext;
        private readonly IHubContext<CommentHub> _ComhubContext;
        private readonly ISocialService _socialService;
        private readonly ICommentService _commentService;
        private readonly INotificationService _notificationService;
        public SocialController(IHubContext<NotificationHub> NothubContext, IHubContext<CommentHub> ComhubContext, ISocialService socialService , ICommentService commentService, INotificationService notificationService)
        {
            _NothubContext = NothubContext;
            _ComhubContext = ComhubContext;
            _socialService = socialService;
            _commentService = commentService;
            _notificationService = notificationService;
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
            // Await the result from GetPostsAsync
            var result = await _socialService.GetPostsAsync(page, pageSize);

            var res = new ResponseModel();

            switch (result)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }




        [HttpGet("LastPost")]
        public async Task<ActionResult<object>> GetLastPost()
        {
            var response = await _socialService.GetLastPostAsync();
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }




        /// <summary>
        /// მიიღეთ კონკრეტული პოსტი მისი ID-ით.
        /// </summary>
        /// <param name="postId">მოსაბრუნებელი პოსტის ID.</param>
        [HttpGet("Post/{postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
            var response = await _socialService.GetPostAsync(postId);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }

        }

        /// <summary>
        /// მიიღეთ პოსტები კონკრეტული თემით.
        /// </summary>
        /// <param name="subject">პოსტების გაფილტვრის საგანი.</param>
        [HttpGet("Posts/{subject}")]
        public async Task<IActionResult> PostsWithSubjeect(string subject, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _socialService.GetPostsWithSubjectAsync(subject, page, pageSize);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }

        /// <summary>
        /// შექმენით ახალი პოსტი მომხმარებლისთვის.
        /// </summary>
        /// <param name="Post">პოსტის ინფორმაციის შექმნა.</param>
        /// <param name="Userid">პოსტის შემქმნელი მომხმარებლის ID.</param>
        [HttpPost("Posts/"), Authorize]
        public async Task<IActionResult> CreatePost(PostDto Post)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _socialService.CreatePostAsync(Post, Int32.Parse(userId), JWTRole);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }



        }

        /// <summary>
        /// არსებული პოსტის რედაქტირება.
        /// </summary>
        /// <param name="EditedPost">რედაქტირებული პოსტის ინფორმაცია.</param>
        [HttpPut("Posts"), Authorize]
        public async Task<IActionResult> EditPost(PostDto EditedPost , int postId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _socialService.EditPostAsync(EditedPost, postId, Int32.Parse(userId), JWTRole);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
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

            var response = await _socialService.DeletePostAsync(postId, Int32.Parse(userId), JWTRole);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }



        // ---------- Comments ----------


        /// <summary>
        /// შექმენით ახალი კომენტარი პოსტისთვის.
        /// </summary>
        /// <param name="comment">შესამუშავებელი კომენტარის ინფორმაცია.</param>
        /// <param name="postid">პოსტის ID, რომელთანაც ასოცირდება კომენტარი.</param>
        /// <param name="userid">კომენტარის შემქმნელი მომხმარებლის ID.</param>
        [HttpPost("Comments/{postid}"), Authorize]
        public async Task<IActionResult> CreateComment(CommentDto comment, int postid)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id check
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _commentService.CreateCommentAsync(comment, postid, Int32.Parse(userId), JWTRole);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }

        /*private string GetConnectionIdForUserId(string userId)
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
*/

        /// <summary>
        /// მიიღეთ კომენტარები კონკრეტული პოსტისთვის.
        /// </summary>
        /// <param name="postId">პოსტის ID კომენტარის მისაღებად.</param>
        [HttpGet("Comments/{postId}")]
        public async Task<IActionResult> GetComments(int postId)
        {

            var response = await _commentService.GetCommentsAsync(postId);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }



        /// <summary>
        /// წაშალეთ კომენტარი მისი ID-ით და მომხმარებლის ID-ით.
        /// </summary>
        /// <param name="commentId">წაშლილი კომენტარის ID.</param>
        /// <param name="userId">კომენტარს წაშლის მომხმარებლის ID.</param>
        [HttpDelete("Comments/{commentId}"), Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

           var response = await _commentService.DeleteCommentAsync(commentId, Int32.Parse(UserId), JWTRole);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }

        /// <summary>
        /// არსებული კომენტარის რედაქტირება.
        /// </summary>
        /// <param name="EditedComment">რედაქტირებული კომენტარის ინფორმაცია.</param>
        [HttpPut("Comments"), Authorize]
        public async Task<IActionResult> EditCommentar(CommentDto EditedComment , int commentid)
        {


            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _commentService.EditCommentAsync(EditedComment, commentid, Int32.Parse(userId), JWTRole);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
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

            var response = await _notificationService.GetUserNotificationsAsync(userId, Int32.Parse(UserId), JWTRole);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }


        [HttpPut("MarkNotificationsAsRead/{userId}"), Authorize]
        public async Task<IActionResult> MarkNotificationsAsRead(int userId)
        {
            var requestingUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var requestingUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            var response = await _notificationService.MarkNotificationsAsRead(userId, Int32.Parse(requestingUserId), requestingUserRole);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }

        /// <summary>
        /// მონიშნეთ შეტყობინება წაკითხულად მისი ID-ით.
        /// </summary>
        /// <param name="notificationId">შეტყობინებების ID, რომ მოინიშნოს წაკითხულად.</param>
        [HttpPut("Notifications/{notificationId}")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var response = await _notificationService.MarkNotificationAsRead(notificationId);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }
    }
}
