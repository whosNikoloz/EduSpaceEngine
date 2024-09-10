using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;

namespace EduSpaceEngine.Services.Learn.Test
{
    public class TestService : ITestService
    {
        private readonly DataDbContext _db;

        public TestService(DataDbContext db)
        {
            _db = db;
        }
        public Task<TestDto> CreateTestlAsync(TestDto testDto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteTestAsync(int testId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TestDto>> GetAllTestAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TestDto> GetTestByIdAsync(int testId)
        {
            throw new NotImplementedException();
        }

        public Task<TestDto> UpdateTestAsync(int testId, TestDto testDto)
        {
            throw new NotImplementedException();
        }
    }
}
