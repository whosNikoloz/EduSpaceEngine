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

namespace EduSpaceEngine.Controllers.v1.Social
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }


        [HttpPost("comments/{postid}"), Authorize]
        public async Task<IActionResult> CreateComment(CommentDto comment, int postid)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id check
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _commentService.CreateCommentAsync(comment, postid, int.Parse(userId), JWTRole);
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
        [HttpGet("{postId}/comments")]
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
        [HttpDelete("comments/{commentId}"), Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _commentService.DeleteCommentAsync(commentId, int.Parse(UserId), JWTRole);
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
        [HttpPut("comments"), Authorize]
        public async Task<IActionResult> EditCommentar(CommentDto EditedComment, int commentid)
        {


            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _commentService.EditCommentAsync(EditedComment, commentid, int.Parse(userId), JWTRole);
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
