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
using GreenDonut;
using Azure;


namespace EduSpaceEngine.Controllers.v1.Learn
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;
        public TestController(ITestService testService)
        {
            _testService = testService;
        }

        /// <summary>
        /// ამოიღებს ტესტების სიას.
        /// </summary>
        [HttpGet("Tests")]
        public async Task<ActionResult<IEnumerable<TestModel>>> GetTests()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _testService.GetAllTestAsync();

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
        /// ამოიღებს კონკრეტულ ტესტს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="id">ტესტის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("Tests/{id}")]
        public async Task<ActionResult<TestModel>> GetTest(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _testService.GetTestByIdAsync(id);

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
        /// ამატებს ახალ ტესტს.
        /// </summary>
        /// <param name="test">დამატებული ახალი ტესტის ინფორმაცია.</param>
        [HttpPost("Tests/{LearnId}"), Authorize(Roles = "admin")]
        public async Task<ActionResult<TestModel>> PostTest(TestDto test, int LearnId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _testService.CreateTestlAsync(test, LearnId);
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
        /// არედაქტირებს არსებულ ტესტს.
        /// </summary>
        /// <param name="id">რედაქტირებადი ტესტის უნიკალური იდენტიფიკატორი.</param>
        /// <param name="test">ტესტის განახლებული ინფორმაცია.</param>
        [HttpPut("Tests/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PutTest(int id, TestDto test)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _testService.UpdateTestAsync(id, test);
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
        /// შლის კონკრეტულ ტესტს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="id">წაშლილი ტესტის უნიკალური იდენტიფიკატორი.</param>
        [HttpDelete("Tests/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _testService.DeleteTestAsync(id);
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
