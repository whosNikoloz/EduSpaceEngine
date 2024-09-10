﻿using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Test
{
    public interface ITestService
    {
        Task<IActionResult> CreateTestlAsync(TestDto testDto);

        Task<IActionResult> GetTestByIdAsync(int testId);

        Task<IActionResult> GetAllTestAsync();

        Task<IActionResult> UpdateTestAsync(int testId, TestDto testDto);

        Task<IActionResult> DeleteTestAsync(int testId);
    }
}