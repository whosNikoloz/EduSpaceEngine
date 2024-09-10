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


namespace EduSpaceEngine.Controllers.v1.Learn
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class TestController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IConfiguration _configuration;

        public TestController(DataDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// ამოიღებს ტესტების სიას.
        /// </summary>
        [HttpGet("Tests")]
        public async Task<ActionResult<IEnumerable<TestModel>>> GetTests()
        {
            return await _context.Tests.Include(t => t.Answers).ToListAsync();
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

            var test = await _context.Tests.Include(t => t.Answers).FirstOrDefaultAsync(t => t.TestId == id);

            if (test == null)
            {
                return NotFound();
            }

            return test;
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

            var learn = await _context.Learn.FirstOrDefaultAsync(u => u.LearnId == LearnId);

            if (learn == null)
            {
                return NotFound("Learn not found");
            }

            var testModel = new TestModel
            {
                Instruction_en = test.Instruction_en,
                Instruction_ka = test.Instruction_ka,
                Code = test.Code,
                Question_en = test.Question_en,
                Question_ka = test.Question_ka,
                Hint_en = test.Hint_en,
                Hint_ka = test.Hint_ka,
                LearnId = learn.LearnId,
            };

            _context.Tests.Add(testModel);
            await _context.SaveChangesAsync();

            // Set the Learn's TestId to the newly created test's ID
            learn.TestId = testModel.TestId;

            // Update the Learn entity with the TestId change
            _context.Learn.Update(learn);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTest", new { id = testModel.TestId }, testModel);
        }

        /// <summary>
        /// არედაქტირებს არსებულ ტესტს.
        /// </summary>
        /// <param name="id">რედაქტირებადი ტესტის უნიკალური იდენტიფიკატორი.</param>
        /// <param name="test">ტესტის განახლებული ინფორმაცია.</param>
        [HttpPut("Tests/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PutTest(int id, TestModel test)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != test.TestId)
            {
                return BadRequest();
            }

            _context.Entry(test).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
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
            try
            {
                var test = await _context.Tests.Include(t => t.Answers)
                                               .FirstOrDefaultAsync(t => t.TestId == id);

                if (test == null)
                {
                    return NotFound();
                }

                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting test: {ex.Message}");
            }
        }




    }
}
