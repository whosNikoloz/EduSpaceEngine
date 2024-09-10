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


namespace EduSpaceEngine.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class AnswersController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IConfiguration _configuration;

        public AnswersController(DataDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Retrieves answers for a specific test based on its unique identifier.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        [HttpGet("answers/{testid}")]
        public async Task<ActionResult<TestModel>> GetAnswers(int testId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var test = await _context.Tests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null)
            {
                return NotFound();
            }
            return null;

            //return CreatedAtAction(nameof(GetTest), new { id = test.TestId }, test);
        }

        /// <summary>
        /// Adds an answer to a specific test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <param name="answer">The information of the added answer.</param>
        [HttpPost("{testId}/answers"), Authorize(Roles = "admin")]
        public async Task<ActionResult<TestModel>> AddAnswerToTest(int testId, TestAnswerDto answer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var test = await _context.Tests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null)
            {
                return NotFound();
            }

            if (test.Answers == null)
            {
                test.Answers = new List<TestAnswerModel>();
            }

            var Answer = new TestAnswerModel
            {
                Option_en = answer.Option_en,
                Option_ka = answer.Option_ka,
                IsCorrect = answer.IsCorrect,
                TestId = testId,
            };

            _context.TestAnswers.Add(Answer);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                // You might inform the user about the concurrency issue and prompt for action
            }
            return null;

 //           return CreatedAtAction(nameof(GetTest), new { id = test.TestId }, test);
        }

        /// <summary>
        /// Deletes a specific answer based on its unique identifier.
        /// </summary>
        /// <param name="answerid">The unique identifier of the deleted answer.</param>
        [HttpDelete("answers/{answerid}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAnswers(int answerid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var answer = await _context.TestAnswers.FindAsync(answerid);

                if (answer == null)
                {
                    return NotFound();
                }

                _context.TestAnswers.Remove(answer);
                await _context.SaveChangesAsync();

                return Ok("Deleted");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                // You might inform the user about the concurrency issue and prompt for action
                return BadRequest("Concurrency error occurred while deleting the answer.");
            }
            catch (Exception ex)
            {
                // Log any other unexpected exceptions
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting answer: {ex.Message}");
            }
        }
    }
}
