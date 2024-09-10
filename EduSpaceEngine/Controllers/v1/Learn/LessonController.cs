using Asp.Versioning;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Services.Learn.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn.Request;
using EduSpaceEngine.Data;
using Microsoft.EntityFrameworkCore;
using EduSpaceEngine.Services.Learn.Lesson;
using EduSpaceEngine.Dto;
using GreenDonut;


namespace EduSpaceEngine.Controllers.v1.Learn
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet("Lessons")]
        public async Task<ActionResult<IEnumerable<LessonModel>>> Lessons()
        {
            var response = await _lessonService.GetAllLessonsAsync();

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

                case ConflictObjectResult conflict:
                    res.status = false;
                    res.result = conflict.Value?.ToString();
                    return Conflict(res);

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

        [HttpGet("Lesson/{lessonid}")]
        public async Task<IActionResult> Lesson(int lessonid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _lessonService.GetLessonByIdAsync(lessonid);

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

                case ConflictObjectResult conflict:
                    res.status = false;
                    res.result = conflict.Value?.ToString();
                    return Conflict(res);

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

        [HttpPost("Lesson"), Authorize(Roles = "admin")]
        public async Task<IActionResult> AddLesson(LessonDto newlesson, string subjectname_en)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var response = await _lessonService.CreateLessonAsync(newlesson, subjectname_en);

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

                case ConflictObjectResult conflict:
                    res.status = false;
                    res.result = conflict.Value?.ToString();
                    return Conflict(res);

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

        [HttpPut("Lessons/{lessonid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditLesson(LessonDto newlesson, int lessonid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _lessonService.UpdateLessonAsync(lessonid, newlesson);
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

                case ConflictObjectResult conflict:
                    res.status = false;
                    res.result = conflict.Value?.ToString();
                    return Conflict(res);

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

        [HttpDelete("Lessons/{lesson}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteLesson(int lessonid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _lessonService.DeleteLessonAsync(lessonid);
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

                case ConflictObjectResult conflict:
                    res.status = false;
                    res.result = conflict.Value?.ToString();
                    return Conflict(res);

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
