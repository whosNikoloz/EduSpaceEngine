using Asp.Versioning;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Services.Learn.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn.Request;
using Microsoft.EntityFrameworkCore;
using EduSpaceEngine.Services.Learn.Course;
using Azure.Core;
using EduSpaceEngine.Dto;


namespace EduSpaceEngine.Controllers.v1.Learn
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("courses")]
        public async Task<IActionResult> Courses()
        {
            var response = await _courseService.GetAllCoursesAsync();

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

        [HttpGet("{levelId}/course")]
        public async Task<IActionResult> CoursesByLevel(int levelId)
        {
            var response = await _courseService.GetCoursesByLevelAsync(levelId);

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



        [HttpGet("courseName/{notFormattedCourseName}")]
        public async Task<IActionResult> CourseFormattedName(string notFormattedCourseName, string lang = "ka")
        {
            var response = await _courseService.GetCourseFormattedNameAsync(notFormattedCourseName, lang);

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
        /// ამოიღებს კონკრეტულ კურსს თავისი უნიკალური იდენტიფიკატორი
        /// </summary>
        /// <param name="courseid">კურსის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("course/{courseName}")]
        public async Task<IActionResult> Course(string courseName, string lang = "ka")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _courseService.GetCourseByName(courseName, lang);
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
        /// Adds a new course.
        /// </summary>
        /// <param name="newCourseModel">დამატებული ახალი კურსის ინფორმაცია.</param>
        [HttpPost("{levelid}/course")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddCourse(CourseDto newCourseModel,int levelid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _courseService.CreateCourseAsync(newCourseModel, levelid);

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
        /// არედაქტირებს არსებულ კურსს.
        /// </summary>
        /// <param name="newcourse">კურსის განახლებული ინფორმაცია.</param>
        /// <param name="courseid">რედაქტირებადი კურსის უნიკალური იდენტიფიკატორი.</param>
        [HttpPut("course/{courseid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditCourse(CourseDto newcourse, int courseid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _courseService.UpdateCourseAsync(courseid, newcourse);
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
        /// შლის კონკრეტულ კურსს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="courseid">კურსის უნიკალური იდენტიფიკატორი, რომელიც უნდა წაიშალოს.</param>
        [HttpDelete("course/{courseid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCourse(int courseid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _courseService.DeleteCourseAsync(courseid);
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
