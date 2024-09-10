using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Level
{
    public interface ILevelService
    {
        Task<IActionResult> CreateLevelAsync(LevelDto levelDto);

        Task<IActionResult> GetLevelByIdAsync(int levelId);

        Task<IActionResult> GetAllLevelsAsync();

        Task<IActionResult> UpdateLevelAsync(int levelId, LevelDto levelDto);

        Task<IActionResult> DeleteLevelAsync(int levelId);
    }
}
