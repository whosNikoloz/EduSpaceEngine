using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using EduSpaceEngine.Services.Social;
using EduSpaceEngine.Dto.Social;
using EduSpaceEngine.Dto;

namespace EduSpaceEngine.Controllers.v1.Social
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        public PostController(IPostService postService)
        {
            _postService = postService;
        }


        [HttpGet("posts")]
        public async Task<ActionResult<IEnumerable<object>>> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Await the result from GetPostsAsync
            var result = await _postService.GetPostsAsync(page, pageSize);

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




        [HttpGet("lastpost")]
        public async Task<ActionResult<object>> GetLastPost()
        {
            var response = await _postService.GetLastPostAsync();
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
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
            var response = await _postService.GetPostAsync(postId);
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
        [HttpGet("{subject}/posts")]
        public async Task<IActionResult> PostsWithSubjeect(string subject, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _postService.GetPostsWithSubjectAsync(subject, page, pageSize);

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
        [HttpPost("posts"), Authorize]
        public async Task<IActionResult> CreatePost(PostDto Post)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _postService.CreatePostAsync(Post, int.Parse(userId), JWTRole);

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
        [HttpPut("posts"), Authorize]
        public async Task<IActionResult> EditPost(PostDto EditedPost, int postId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _postService.EditPostAsync(EditedPost, postId, int.Parse(userId), JWTRole);
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
        [HttpDelete("posts/{postId}"), Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // JWT id check
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; // JWT Role

            var response = await _postService.DeletePostAsync(postId, int.Parse(userId), JWTRole);
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
