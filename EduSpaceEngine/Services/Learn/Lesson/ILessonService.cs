﻿using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Lesson
{
    public interface ILessonService
    {
        Task<IActionResult> CreateLessonAsync(LessonDto lessonDto, int subjectid);

        Task<IActionResult> GetLessonByIdAsync(int lessonId);

        Task<IActionResult> GetAllLessonsAsync();

        Task<IActionResult> UpdateLessonAsync(int lessonId, LessonDto lessonDto);

        Task<IActionResult> DeleteLessonAsync(int lessonId);

        Task<IActionResult> GetLessonsBySubjectId(int subjectId);
    }
}
