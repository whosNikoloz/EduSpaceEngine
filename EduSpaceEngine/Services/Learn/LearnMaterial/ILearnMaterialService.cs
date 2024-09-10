using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.LearnMaterial
{
    public interface ILearnMaterialService
    {
        Task<IActionResult> CreateLearnMaterialAsync(LearnMaterialDto learnMaterialDto);

        Task<IActionResult> GetLearnMaterialByIdAsync(int learnMaterialId);

        Task<IActionResult> GetAllLearnMaterialAsync();

        Task<IActionResult> UpdateLearnMaterialAsync(int learnMaterialId, LearnMaterialDto learnMaterialDto);

        Task<IActionResult> DeleteLearnMaterialAsync(int lessonId);

        Task<IActionResult> GetLearnMateriasByLessonId(int lessonId);
    }
}
