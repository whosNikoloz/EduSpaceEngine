using AutoMapper;
using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Model.Learn.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

namespace EduSpaceEngine.Services.Learn.Course
{
    public class CourseService : ICourseService
    {

        private readonly DataDbContext _db;
        private readonly IMapper _mapper;

        public CourseService(DataDbContext db,IMapper mapper)
        {
            _mapper = mapper;
            _db = db;

        }

        public async Task<IActionResult> GetCoursesByLevelAsync(int LevelId)
        {
            var levelExists = _db.Levels.Any(u => u.LevelId == LevelId);
            if (!levelExists) 
            {
                return new NotFoundObjectResult("Level Not Found");    
            }
            var courses = await _db.Courses
                .Include(u => u.Level)
                .Include(u => u.Subjects)
                .Where(u => u.LevelId == LevelId)
                .ToListAsync();
            if(courses == null)
            {
                return new NotFoundObjectResult("Courses Not Found");
            }
            return new OkObjectResult(courses);

        }

        public async Task<IActionResult> CreateCourseAsync(CourseDto courseDto , int levelId)
        {
            if (_db.Courses.Any(u => u.CourseName_ka == courseDto.CourseName_ka))
            {
                return new BadRequestObjectResult("Course With CourseName_ka Already Exists");
            }

            if (_db.Courses.Any(u => u.CourseName_en == courseDto.CourseName_en))
            {
                return new BadRequestObjectResult("Course With CourseName_en Already Exists");
            }

            if (_db.Courses.Any(u => u.FormattedCourseName == courseDto.FormattedCourseName))
            {
                return new BadRequestObjectResult("Course With FormattedCourseName Already Exists");
            }
            var levelExists = _db.Levels.Any(u => u.LevelId == levelId);
            if(!levelExists)
            {
                return new NotFoundObjectResult("Level Not Found");
            }

            CourseModel courseModel = _mapper.Map<CourseModel>(courseDto);
            courseModel.LevelId = levelId;
            _db.Courses.Add(courseModel);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return new BadRequestObjectResult("Error Creating Course");
            }

            return new OkObjectResult(courseModel);

        }

        public async Task<IActionResult> DeleteCourseAsync(int courseId)
        {
            var course   = _db.Courses.FirstOrDefault(u => u.CourseId == courseId);
            if(course == null)
            {
                return new NotFoundObjectResult("Course Not Found");
            } 
            try
            {
                _db.Courses.Remove(course);
                await _db.SaveChangesAsync();
            }
            catch(Exception)
            {
                return new BadRequestObjectResult("Error Deleting Course");
            }
            return new OkObjectResult("Deleted Course");

        }

        public async Task<IActionResult> GetAllCoursesAsync()
        {
            var course = await _db.Courses
                .Include(u => u.Level)
                .Include(u => u.Subjects).ThenInclude(s => s.Lessons)
                .Include(u => u.Enrollments)
                .ToListAsync();

            if(course == null)
            {
                return new NotFoundObjectResult("Courses Not Found");
            }
            return new OkObjectResult(course);
        }

        public async Task<IActionResult> GetCourseByIdAsync(int courseId)
        {
            var course = await _db.Courses
                .Include(u => u.Level)
                .Include(u => u.Subjects).ThenInclude(s => s.Lessons)
                .Include(u => u.Enrollments)
                .FirstOrDefaultAsync(u => u.CourseId == courseId);
            if(course == null)
            {
                return new NotFoundObjectResult("Course Not Found");
            }
            return new OkObjectResult(course);
        }

        public async Task<IActionResult> GetCourseByName(string courseName, string lang = "ka")
        {
            var course = await _db.Courses
                .Include(u => u.Level)
                .Include(u => u.Subjects).ThenInclude(s => s.Lessons)
                .Include(u => u.Enrollments)
                .FirstOrDefaultAsync(u => u.FormattedCourseName == courseName);

            if(course == null)
            {
                return new NotFoundObjectResult("Course Not Found");
            }

            CourseDto courseDto = _mapper.Map<CourseDto>(course);

            return new OkObjectResult(courseDto);
        }

        public async Task<IActionResult> GetCourseFormattedNameAsync(string notFormattedCourseName, string lang = "ka")
        {
            string columnName = $"CourseName_{lang}";

            var course = await _db.Courses
                .Where(u => u.FormattedCourseName == notFormattedCourseName)
                .Select(u => new { CourseName = EF.Property<string>(u, columnName) })
                .FirstOrDefaultAsync();

            if (course == null || string.IsNullOrWhiteSpace(course.CourseName))
            {
                return new NotFoundObjectResult("Course Not Found");
            }

            return new OkObjectResult(course.CourseName);
        }

        public async Task<IActionResult> UpdateCourseAsync(int courseId, CourseDto courseDto)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(u => u.CourseId == courseId);

            if (course == null)
            {
                return new NotFoundObjectResult("Course Not Found");
            }
            try
            {
                _mapper.Map(courseDto, course);
                await _db.SaveChangesAsync();
            }
            catch(Exception)
            {
                return new BadRequestObjectResult("Error Updating Course");
            }
            return new OkObjectResult(course);
        }
    }
}
