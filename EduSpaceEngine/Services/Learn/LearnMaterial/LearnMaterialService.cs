using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;

namespace EduSpaceEngine.Services.Learn.LearnMaterial
{
    public class LearnMaterialService : ILearnMaterialService
    {
        private readonly DataDbContext _db;

        public LearnMaterialService(DataDbContext db)
        {
            _db = db;
        }

        public Task<LearnMaterialDto> CreateLearnMaterialAsync(LearnMaterialDto learnMaterialDto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteLearnMaterialAsync(int lessonId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LearnMaterialDto>> GetAllLearnMaterialAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LearnMaterialDto> GetLearnMaterialByIdAsync(int learnMaterialId)
        {
            throw new NotImplementedException();
        }

        public Task<LearnMaterialDto> UpdateLearnMaterialAsync(int learnMaterialId, LearnMaterialDto learnMaterialDto)
        {
            throw new NotImplementedException();
        }
    }
}
