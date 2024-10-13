using AutoMapper;
using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduSpaceEngine.Services.Learn.Level
{
    public class LevelService : ILevelService
    {
        private readonly DataDbContext _db;
        private readonly IMapper _mapper;

        public LevelService(DataDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }
        public async Task<IActionResult> CreateLevelAsync(LevelDto levelDto)
        {
            if (_db.Levels.Any(u => u.LevelName_ka == levelDto.LevelName_ka))
            {
                return new BadRequestObjectResult("Level With LevelName_ka Already Exsits");
            }

            if (_db.Levels.Any(u => u.LevelName_en == levelDto.LevelName_en))
            {
                return new BadRequestObjectResult("Level With LevelName_en Already Exsits");
            }


            LevelModel level = _mapper.Map<LevelModel>(levelDto);

            _db.Levels.Add(level);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            return new OkObjectResult(level);
        }

        public async Task<IActionResult> DeleteLevelAsync(int levelId)
        {
            try
            {
                var level = await _db.Levels
                    .SingleOrDefaultAsync(l => l.LevelId == levelId);

                if (level == null)
                {
                    return new NotFoundObjectResult("Level Not Found");
                }

                _db.Levels.Remove(level);

                await _db.SaveChangesAsync();

                return new OkObjectResult("Level and all associated entities removed");
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<IActionResult> GetAllLevelsAsync()
        {
            var levels = await _db.Levels.Include(c => c.Courses).ToListAsync();
            if(levels == null)
            {
                return new NotFoundObjectResult("No Levels Found");
            }
            return new OkObjectResult(levels);
        }

        public async Task<IActionResult> GetLevelByIdAsync(int levelId)
        {
            var level = await _db.Levels.SingleOrDefaultAsync(l => l.LevelId == levelId);
            if (level == null)
            {
                return new NotFoundObjectResult("Level Not Found");
            }
            return new OkObjectResult(level);
        }


        public async Task<IActionResult> UpdateLevelAsync(int levelId, LevelDto levelDto)
        {
            var levelEntity = await _db.Levels.FirstOrDefaultAsync(u => u.LevelId == levelId);

            if (levelEntity == null)
            {
                return new NotFoundObjectResult("Level Not Found");
            }

            _mapper.Map(levelDto, levelEntity);

            try
            {
                _db.Levels.Update(levelEntity);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkObjectResult(levelEntity);
        }
    }
}
