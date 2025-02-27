﻿using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Course
{
    public interface ICourseService
    {

        Task<IActionResult> CreateCourseAsync(CourseDto courseDto, int levelId);

        Task<IActionResult> GetCourseByIdAsync(int courseId);

        Task<IActionResult> GetAllCoursesAsync();

        Task<IActionResult> GetCourseFormattedNameAsync(string notFormattedCourseName, string lang = "ka");

        Task<IActionResult> GetCourseByName(string courseName, string lang = "ka");

        Task<IActionResult> UpdateCourseAsync(int courseId, CourseDto courseDto);

        Task<IActionResult> UploadCourseLogoAsync(UploadLogoRequest imageRequest);

        Task<IActionResult> GetCoursesByLevelAsync(int levelId);
        Task<IActionResult> DeleteCourseAsync(int courseId);
    }
}
