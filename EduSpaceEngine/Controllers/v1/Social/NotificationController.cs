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
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <param name="userId">მომხმარებლის ID, რომლისთვისაც უნდა მიიღოთ შეტყობინებები.</param>
        [HttpGet("notifications/{userId}"), Authorize]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _notificationService.GetUserNotificationsAsync(userId, int.Parse(UserId), JWTRole);

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

            var response = await _notificationService.MarkNotificationsAsRead(userId, int.Parse(requestingUserId), requestingUserRole);
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
