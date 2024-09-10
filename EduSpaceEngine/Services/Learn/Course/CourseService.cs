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

        public async Task<IActionResult> CreateCourseAsync(CourseDto courseDto)
        {
            if (_db.Courses.Any(u => u.CourseName_ka == courseDto.CourseName_ka))
            {
                return new BadRequestObjectResult("Course Already Exists");
            }

            CourseModel courseModel = _mapper.Map<CourseModel>(courseDto);

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
            return new OkObjectResult(course);

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

        public async Task<IActionResult> GetCourseFormattedNameAsync(string FormattedCourseName)
        {
            var course = await _db.Courses
                .Include(u => u.Level)
                .Include(u => u.Subjects).ThenInclude(s => s.Lessons)
                .Include(u => u.Enrollments)
                .FirstOrDefaultAsync(u => u.FormattedCourseName == FormattedCourseName);

            if (course == null)
            {
                return new NotFoundObjectResult("Course Not Found");
            }

            return new OkObjectResult(course);
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
                course = _mapper.Map<CourseModel>(courseDto);
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
