using Asp.Versioning;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Services.Learn.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn.Request;
using EduSpaceEngine.Model.Learn.Test;
using Microsoft.EntityFrameworkCore;
using EduSpaceEngine.Data;
using EduSpaceEngine.Services.Learn.LearnMaterial;
using EduSpaceEngine.Dto;


namespace EduSpaceEngine.Controllers.v1.Learn
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class LearnMateriaController : ControllerBase
    {
        private readonly ILearnMaterialService _learnMaterialService;

        public LearnMateriaController(ILearnMaterialService learnMaterialService)
        {
            _learnMaterialService = learnMaterialService;
        }

        /// <summary>
        /// ამოიღებს სასწავლო მასალის ჩამონათვალს.
        /// </summary>
        [HttpGet("learnmaterials")]
        public async Task<ActionResult<IEnumerable<LearnModel>>> GetLearns()
        {
            var response = await _learnMaterialService.GetAllLearnMaterialAsync();
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
        /// ამოიღებს კონკრეტულ სასწავლო მასალას მისი უნიკალური იდენტიფიკატორით.
        /// </summary>
        /// <param name="id">სასწავლო მასალის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("{lessonid}/learnmaterials")]
        public async Task<ActionResult> GetLearnmaterial(int lessonid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _learnMaterialService.GetLearnMateriasByLessonId(lessonid);
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
        /// ამოიღებს კონკრეტულ სასწავლო მასალას მისი უნიკალური იდენტიფიკატორით.
        /// </summary>
        /// <param name="id">სასწავლო მასალის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("learnmaterial/{learnid}")]
        public async Task<ActionResult<LearnModel>> GetLearn(int learnid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _learnMaterialService.GetLearnMaterialByIdAsync(learnid);
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
        /// ამატებს ახალ სასწავლო მასალას.
        /// </summary>
        /// <param name="learn">დამატებული ახალი სასწავლო მასალის ინფორმაცია.</param>
        /// <param name="subjectname">საგნის სახელწოდება, რომელსაც მიეკუთვნება სასწავლო მასალა.</param>
        /// <param name="coursename">კურსის სახელწოდება, რომელსაც მიეკუთვნება სასწავლო მასალა.</param>
        [HttpPost("learnmaterial"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PostLearn(LearnMaterialDto learn, int LessonId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _learnMaterialService.CreateLearnMaterialAsync(learn, LessonId);
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
        /// ასწორებს არსებულ სასწავლო მასალას.
        /// </summary>
        /// <param name="id">რედაქტირებადი სასწავლო მასალის უნიკალური იდენტიფიკატორი.</param>
        /// <param name="learn">სასწავლო მასალის განახლებული ინფორმაცია.</param>
        [HttpPut("learnmaterial/{learnid}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PutLearn(int learnid, LearnMaterialDto learn)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _learnMaterialService.UpdateLearnMaterialAsync(learnid, learn);
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
        /// შლის კონკრეტულ სასწავლო მასალას მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="id">სასწავლო მასალის უნიკალური იდენტიფიკატორი, რომელიც უნდა წაიშალოს.</param>
        [HttpDelete("learnmaterial/{learnid}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteLearn(int learnid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _learnMaterialService.DeleteLearnMaterialAsync(learnid);
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
