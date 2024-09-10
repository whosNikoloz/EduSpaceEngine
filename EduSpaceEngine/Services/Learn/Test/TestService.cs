using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Test
{
    public class TestService : ITestService
    {
        private readonly DataDbContext _db;

        public TestService(DataDbContext db)
        {
            _db = db;
        }

        public Task<IActionResult> CreateTestlAsync(TestDto testDto)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> DeleteTestAsync(int testId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetAllTestAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetTestByIdAsync(int testId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> UpdateTestAsync(int testId, TestDto testDto)
        {
            throw new NotImplementedException();
        }
    }
}
