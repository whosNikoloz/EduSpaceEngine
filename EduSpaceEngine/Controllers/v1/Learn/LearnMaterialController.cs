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
    public class LearnMateriaController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IConfiguration _configuration;

        public LearnMateriaController(DataDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// ამოიღებს სასწავლო მასალის ჩამონათვალს.
        /// </summary>
        [HttpGet("LearnMaterials")]
        public async Task<ActionResult<IEnumerable<LearnModel>>> GetLearns()
        {
            return await _context.Learn.Include(t => t.Test).ThenInclude(t => t.Answers).ToListAsync();
        }


        /// <summary>
        /// ამოიღებს კონკრეტულ სასწავლო მასალას მისი უნიკალური იდენტიფიკატორით.
        /// </summary>
        /// <param name="id">სასწავლო მასალის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("LearnMaterialByLesson/{LessonId}")]
        public async Task<ActionResult> GetLearnmaterial(int LessonId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lessons = await _context.Learn
                .Include(t => t.Test)
                    .ThenInclude(t => t.Answers) // Include Answers
                .Where(t => t.LessonId == LessonId)
                .ToListAsync();

            if (lessons == null || lessons.Count == 0)
            {
                return NotFound();
            }

            return Ok(lessons);
        }

        /// <summary>
        /// ამოიღებს კონკრეტულ სასწავლო მასალას მისი უნიკალური იდენტიფიკატორით.
        /// </summary>
        /// <param name="id">სასწავლო მასალის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("LearnMaterial/{id}")]
        public async Task<ActionResult<LearnModel>> GetLearn(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var learn = await _context.Learn
                .Include(t => t.Test)
                    .ThenInclude(t => t.Answers) // Include Answers
                .FirstOrDefaultAsync(t => t.LearnId == id);

            if (learn == null)
            {
                return NotFound();
            }

            return learn;
        }

        /// <summary>
        /// ამატებს ახალ სასწავლო მასალას.
        /// </summary>
        /// <param name="learn">დამატებული ახალი სასწავლო მასალის ინფორმაცია.</param>
        /// <param name="subjectname">საგნის სახელწოდება, რომელსაც მიეკუთვნება სასწავლო მასალა.</param>
        /// <param name="coursename">კურსის სახელწოდება, რომელსაც მიეკუთვნება სასწავლო მასალა.</param>
        [HttpPost("LearnMaterial"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PostLearn(LearnMaterialDto learn, int LessonId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lesson = await _context.Lessons.FirstOrDefaultAsync(u => u.LessonId == LessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            if (_context.Learn.Any(u => u.LearnName_ka == learn.LearnName_ka))
            {
                return BadRequest("LearnMaterial Already Exists");
            }

            var Learn = new LearnModel
            {
                LearnName_ka = learn.LearnName_ka,
                LearnName_en = learn.LearnName_en,
                Content_en = learn.Content_en,
                Content_ka = learn.Content_ka,
                Code = learn.Code,
                LessonId = lesson.LessonId,
            };

            _context.Learn.Add(Learn);
            await _context.SaveChangesAsync();

            return Ok(Learn);
        }

        /// <summary>
        /// ასწორებს არსებულ სასწავლო მასალას.
        /// </summary>
        /// <param name="id">რედაქტირებადი სასწავლო მასალის უნიკალური იდენტიფიკატორი.</param>
        /// <param name="learn">სასწავლო მასალის განახლებული ინფორმაცია.</param>
        [HttpPut("LearnMaterial/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PutLearn(int id, LearnModel learn)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != learn.LearnId)
            {
                return BadRequest();
            }

            _context.Entry(learn).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// შლის კონკრეტულ სასწავლო მასალას მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="id">სასწავლო მასალის უნიკალური იდენტიფიკატორი, რომელიც უნდა წაიშალოს.</param>
        [HttpDelete("LearnMaterial/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteLearn(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var learn = await _context.Learn.FindAsync(id);

                if (learn == null)
                {
                    return NotFound();
                }

                _context.Learn.Remove(learn);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting learn material: {ex.Message}");
            }
        }

    }
}
