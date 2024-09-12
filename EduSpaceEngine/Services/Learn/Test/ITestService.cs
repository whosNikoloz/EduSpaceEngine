using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Test
{
    public interface ITestService
    {
        Task<IActionResult> CreateTestAsync(TestDto testDto, int learnid);

        Task<IActionResult> GetTestByIdAsync(int testId);

        Task<IActionResult> GetAllTestAsync();

        Task<IActionResult> UpdateTestAsync(int testId, TestDto testDto);

        Task<IActionResult> DeleteTestAsync(int testId);

        Task<IActionResult> GetTestsByLearnIdAsync(int learnid);
    }
}
