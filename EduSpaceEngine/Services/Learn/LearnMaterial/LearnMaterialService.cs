using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.LearnMaterial
{
    public class LearnMaterialService : ILearnMaterialService
    {
        private readonly DataDbContext _db;

        public LearnMaterialService(DataDbContext db)
        {
            _db = db;
        }

        public Task<IActionResult> CreateLearnMaterialAsync(LearnMaterialDto learnMaterialDto)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> DeleteLearnMaterialAsync(int lessonId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetAllLearnMaterialAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetLearnMaterialByIdAsync(int learnMaterialId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> UpdateLearnMaterialAsync(int learnMaterialId, LearnMaterialDto learnMaterialDto)
        {
            throw new NotImplementedException();
        }
    }
}
