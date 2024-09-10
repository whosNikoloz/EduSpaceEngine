using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Subject
{
    public interface ISubjectService
    {
        Task<IActionResult> CreateSubjectAsync(SubjectDto subjectDto , int courseid);

        Task<IActionResult> GetSubjectByIdAsync(int subjectId);

        Task<IActionResult> GetAllSubjectsAsync();

        Task<IActionResult> UpdateSubjectAsync(int subjectId, SubjectDto subjectDto);

        Task<IActionResult> DeleteSubjectAsync(int subjectId);

        Task<IActionResult> GetSubjectsByCourseIdAsync(int courseId);
    }
}
