using Asp.Versioning;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Services.Learn.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn.Request;
using EduSpaceEngine.Services.Learn.Subject;
using EduSpaceEngine.Dto;
using Azure;


namespace EduSpaceEngine.Controllers.v1.Learn
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        [HttpGet("subjects")]
        public async Task<ActionResult<IEnumerable<SubjectModel>>> Subjects()
        {
            var response = await _subjectService.GetAllSubjectsAsync();

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
        /// ამოიღებს კონკრეტულ საგანს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="subjectid">სუბიექტის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("subject/{subjectid}")]
        public async Task<IActionResult> Subject(int subjectid)
        {
            var response = await _subjectService.GetSubjectByIdAsync(subjectid);


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

        [HttpGet("{CourseId}/courses")]
        public async Task<IActionResult> SubjectByCourse(int CourseId)
        {
            var response = await _subjectService.GetSubjectsByCourseIdAsync(CourseId);

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
        /// ამატებს ახალ საგანს.
        /// </summary>
        /// <param name="newsubject">დამატებული ახალი თემის ინფორმაცია.</param>
        /// <param name="coursename">კურსის სახელწოდება, რომელსაც ეკუთვნის საგანი.</param>
        [HttpPost("subject"), Authorize(Roles = "admin")]
        public async Task<IActionResult> AddSubject(SubjectDto newsubject, int CourseId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _subjectService.CreateSubjectAsync(newsubject, CourseId);

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
        /// არედაქტირებს არსებულ საგანს.
        /// </summary>
        /// <param name="newsubject">განახლებული ინფორმაცია თემისთვის.</param>
        /// <param name="subjectid">რედაქტირებადი საგნის უნიკალური იდენტიფიკატორი.</param>
        [HttpPut("subject/{subjectid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditSubject(SubjectDto newsubject, int subjectid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _subjectService.UpdateSubjectAsync(subjectid, newsubject);


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
        /// შლის კონკრეტულ საგანს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="subjectid">წაშლილი საგნის უნიკალური იდენტიფიკატორი.</param>
        [HttpDelete("subject/{subjectid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteSubject(int subjectid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _subjectService.DeleteSubjectAsync(subjectid);

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

    }
}
