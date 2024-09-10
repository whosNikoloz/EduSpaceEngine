using AutoMapper;
using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduSpaceEngine.Services.Learn.LearnMaterial
{
    public class LearnMaterialService : ILearnMaterialService
    {
        private readonly DataDbContext _db;
        private readonly IMapper _mapper;

        public LearnMaterialService(DataDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IActionResult> CreateLearnMaterialAsync(LearnMaterialDto learnMaterialDto)
        {
            var learnMaterial = _mapper.Map<LearnModel>(learnMaterialDto);
            try
            {
                await _db.Learn.AddAsync(learnMaterial);
                await _db.SaveChangesAsync();
                return new OkObjectResult(learnMaterial);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error creating LearnMaterial: {ex.Message}");
            }
            throw new NotImplementedException();
        }

        public async Task<IActionResult> DeleteLearnMaterialAsync(int lessonId)
        {
            var learnMaterial = await _db.Learn.FirstOrDefaultAsync(lm => lm.LessonId == lessonId);
            if (learnMaterial == null)
            {
                return new NotFoundObjectResult("LearnMaterial not found");
            }
            try
            {
                _db.Learn.Remove(learnMaterial);
                await _db.SaveChangesAsync();
                return new OkObjectResult("LearnMaterial deleted successfully");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error deleting LearnMaterial: {ex.Message}");
            }
        }

        public async Task<IActionResult> GetAllLearnMaterialAsync()
        {
            var learnMaterials = await _db.Learn.ToListAsync();
            if(learnMaterials == null)
            {
                return new NotFoundObjectResult("LearnMaterials not found");
            }
            return new OkObjectResult(learnMaterials);
        }

        public async Task<IActionResult> GetLearnMaterialByIdAsync(int learnMaterialId)
        {
            var learnMaterial = await _db.Learn.FirstOrDefaultAsync(lm => lm.LearnId == learnMaterialId);
            if (learnMaterial == null)
            {
               return new NotFoundObjectResult("LearnMaterial not found");
            }
            return new OkObjectResult(learnMaterial);
        }

        public async Task<IActionResult> UpdateLearnMaterialAsync(int learnMaterialId, LearnMaterialDto learnMaterialDto)
        {
            var learnMaterial = await _db.Learn.FirstOrDefaultAsync(lm => lm.LearnId == learnMaterialId);
            if (learnMaterial == null)
            {
                return new NotFoundObjectResult("LearnMaterial not found");
            }
            try
            {
                _mapper.Map(learnMaterialDto, learnMaterial);
                await _db.SaveChangesAsync();
                return new OkObjectResult(learnMaterial);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error updating LearnMaterial: {ex.Message}");
            }
        }
    }
}
