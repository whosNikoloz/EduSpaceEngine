using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Course
{
    public interface ICourseService
    {

        Task<IActionResult> CreateCourseAsync(CourseDto courseDto);

        Task<IActionResult> GetCourseByIdAsync(int courseId);

        Task<IActionResult> GetAllCoursesAsync();

        Task<IActionResult> GetCourseFormattedNameAsync(string FormattedCourseName);

        Task<IActionResult> UpdateCourseAsync(int courseId, CourseDto courseDto);

        Task<IActionResult> DeleteCourseAsync(int courseId);
    }
}
