using AutoMapper;
using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Dto.Social;
using EduSpaceEngine.Model.Learn.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace EduSpaceEngine.Services.Learn.Test
{
    public class AnswerService : IAnswerService
    {
        private readonly DataDbContext _db;
        private readonly IMapper _mapper;

        public AnswerService(DataDbContext db , IMapper mapper)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task<IActionResult> GetAnswersByTestIdAsync(int testId)
        {
            if(!_db.Tests.Any(t => t.TestId == testId))
            {
                return new NotFoundObjectResult("Test not found");
            }
            var answers = await _db.TestAnswers.Where(t => t.TestId == testId).ToListAsync();
            if (answers == null)
            {
                return new NotFoundObjectResult("Answers not found");
            }
            return new OkObjectResult(answers);
        }

        public async Task<IActionResult> CreateAnswerAsync(TestAnswerDto answerDto, int testid)
        {
            if(!_db.Tests.Any(t => t.TestId == testid))
            {
                return new NotFoundObjectResult("Test not found");
            }
            var test = await _db.Tests.FirstOrDefaultAsync(u => u.TestId == testid);

            if (test == null)
            {
                return new NotFoundObjectResult("test not found");
            }

            try
            {
                TestAnswerModel newAnswer = _mapper.Map<TestAnswerModel>(answerDto);
                newAnswer.TestId = testid;

                _db.TestAnswers.Add(newAnswer);
                await _db.SaveChangesAsync();

                return new OkObjectResult(newAnswer);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error creating test: {ex.Message}");
            }
        }


        public async Task<IActionResult> DeleteAnswerAsync(int answerId)
        {
            try
            {
                var answer = await _db.TestAnswers.FirstOrDefaultAsync(t => t.AnswerId == answerId);
                if (answer == null)
                {
                    return new NotFoundObjectResult("answer not found");
                }

                _db.TestAnswers.Remove(answer);
                await _db.SaveChangesAsync();

                return new OkObjectResult("Deleted");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error deleting answer: {ex.Message}");
            }
        }


        public async Task<IActionResult> GetAllAnswerAsync()
        {
            var answers = await _db.TestAnswers.ToListAsync();  
            if (answers == null)
            {
                return new NotFoundObjectResult("answers not found");
            }
            return new OkObjectResult(answers);
        }

        public async Task<IActionResult> GetAnswerByIdAsync(int answerId)
        {
            var answer = await _db.TestAnswers.FirstOrDefaultAsync(t => t.AnswerId == answerId);

            if (answer == null)
            {
                return new NotFoundObjectResult("answer not found");
            }
            return new OkObjectResult(answer);
        }

        public async Task<IActionResult> UpdateAnswerAsync(int answerId, TestAnswerDto answerDto)
        {
            var answer = await _db.TestAnswers.FirstOrDefaultAsync(t => t.AnswerId == answerId);
            if (answer == null)
            {
                return new NotFoundObjectResult("Test not found");
            }
            try
            {
                _mapper.Map(answerDto, answer);
                _db.TestAnswers.Update(answer);
                _db.SaveChanges();
                return new OkObjectResult(answer);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error updating answer: {ex.Message}");
            }
        }
        
    }
}
