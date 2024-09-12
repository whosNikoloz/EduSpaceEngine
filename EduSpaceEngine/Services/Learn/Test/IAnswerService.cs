using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Test
{
    public interface IAnswerService
    {
        Task<IActionResult> CreateAnswerAsync(TestAnswerDto answerDto, int testid);

        Task<IActionResult> GetAnswerByIdAsync(int answerid);

        Task<IActionResult> GetAllAnswerAsync();

        Task<IActionResult> UpdateAnswerAsync(int answerid, TestAnswerDto answerDto);

        Task<IActionResult> DeleteAnswerAsync(int answerid);

        Task<IActionResult> GetAnswersByTestIdAsync(int testId);
    }
}
