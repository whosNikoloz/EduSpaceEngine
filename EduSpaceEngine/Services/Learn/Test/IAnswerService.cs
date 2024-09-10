using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Test
{
    public interface IAnswerService
    {
        Task<IActionResult> CreateAnswerAsync(TestAnswerDto answerDto, int TestId);

        Task<IActionResult> GetAnswerByIdAsync(int answerId);

        Task<IActionResult> GetAllAnswerAsync();

        Task<IActionResult> UpdateAnswerAsync(int answerId, TestAnswerDto answerDto);

        Task<IActionResult> DeleteAnswerAsync(int answerId);
    }
}
