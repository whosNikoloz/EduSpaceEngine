using AutoMapper;
using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace EduSpaceEngine.Services.Learn.Test
{
    public class TestService : ITestService
    {
        private readonly DataDbContext _db;
        private readonly IMapper _mapper;

        public TestService(DataDbContext db , IMapper mapper)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task<IActionResult> GetTestsByLearnIdAsync(int learnId)
        {
            var tests = await _db.Tests.Where(t => t.LearnId == learnId).ToListAsync();
            if (tests == null)
            {
                return new NotFoundObjectResult("Tests not found");
            }
            return new OkObjectResult(tests);
        }

        public async Task<IActionResult> CreateTestlAsync(TestDto testDto , int LearnId)
        {
            var learn = await _db.Learn.FirstOrDefaultAsync(u => u.LearnId == LearnId);

            if (learn == null)
            {
                return new NotFoundObjectResult("Learn not found");
            }

            try
            {
                TestModel newTest = _mapper.Map<TestModel>(testDto);
                newTest.LearnId = LearnId;

                _db.Tests.Add(newTest);
                await _db.SaveChangesAsync();

                // Set the Learn's TestId to the newly created test's ID
                learn.TestId = newTest.TestId;

                // Update the Learn entity with the TestId change
                _db.Learn.Update(learn);
                await _db.SaveChangesAsync();

                return new OkObjectResult(newTest);
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult($"Error creating test: {ex.Message}");
            }

        }

        public async Task<IActionResult> DeleteTestAsync(int testId)
        {
            try
            {
                var test = await _db.Tests.Include(t => t.Answers)
                                               .FirstOrDefaultAsync(t => t.TestId == testId);
                if (test == null)
                {
                    return new NotFoundObjectResult("Test not found");
                }

                _db.Tests.Remove(test);
                await _db.SaveChangesAsync();

                return new OkObjectResult("Deleted");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error deleting test: {ex.Message}");
            }
        }

        public async Task<IActionResult> GetAllTestAsync()
        {
            var tests = await _db.Tests.Include(t => t.Answers).ToListAsync();
            if (tests == null)
            {
                return new NotFoundObjectResult("Tests not found");
            }
            return new OkObjectResult(tests);
        }

        public async Task<IActionResult> GetTestByIdAsync(int testId)
        {
            var test = await _db.Tests.Where(o => o.TestId == testId).Include(t => t.Answers).FirstOrDefaultAsync(); 

            if(test == null)
            {
                return new NotFoundObjectResult("Test not found");
            }
            return new OkObjectResult(test);
        }

        public async Task<IActionResult> UpdateTestAsync(int testId, TestDto testDto)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                return new NotFoundObjectResult("Test not found");
            }

            try
            {
                test = _mapper.Map<TestModel>(testDto);
                _db.Tests.Update(test);
                _db.SaveChanges();
                return new OkObjectResult(test);
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult($"Error updating test: {ex.Message}");
            }
        }
    }
}
