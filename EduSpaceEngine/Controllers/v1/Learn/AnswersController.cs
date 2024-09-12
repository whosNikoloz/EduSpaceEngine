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
using EduSpaceEngine.Services.Learn.Test;
using EduSpaceEngine.Dto;


namespace EduSpaceEngine.Controllers.v1.Learn
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerService _answerService;
        public AnswersController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        /// <summary>
        /// Retrieves answers for a specific test based on its unique identifier.
        /// </summary>
        /// <param name="testid">The unique identifier of the test.</param>
        [HttpGet("answer/{testid}")]
        public async Task<ActionResult<TestModel>> GetAnswer(int testid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _answerService.GetAnswerByIdAsync(testid);
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

        [HttpPut("answer/{answerid}")]
        public async Task<ActionResult<TestModel>> UpdateAnswer(int answerid, TestAnswerDto answer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _answerService.UpdateAnswerAsync(answerid, answer);
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
        /// Adds an answer to a specific test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <param name="answer">The information of the added answer.</param>
        [HttpPost("{testId}/answer"), Authorize(Roles = "admin")]
        public async Task<ActionResult<TestModel>> AddAnswerToTest(int testId, TestAnswerDto answer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _answerService.CreateAnswerAsync(answer, testId);
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
        /// Deletes a specific answer based on its unique identifier.
        /// </summary>
        /// <param name="answerid">The unique identifier of the deleted answer.</param>
        [HttpDelete("answer/{answerid}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAnswer(int answerid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _answerService.DeleteAnswerAsync(answerid);
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

        [HttpGet("{testId}/answers")]
        public async Task<ActionResult<TestModel>> GetAnswersByTestId(int testId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _answerService.GetAnswersByTestIdAsync(testId);
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
