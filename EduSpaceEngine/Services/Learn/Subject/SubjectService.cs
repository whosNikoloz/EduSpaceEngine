using AutoMapper;
using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduSpaceEngine.Services.Learn.Subject
{
    public class SubjectService : ISubjectService
    {

        private readonly DataDbContext _db;
        private readonly IMapper _mapper;

        public SubjectService(DataDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IActionResult> GetSubjectsByCourseIdAsync(int courseId)
        {
            var subjects = await _db.Subjects
                .Include(u => u.Course)
                .Where(u => u.CourseId == courseId)
                .ToListAsync();
            if (subjects == null)
            {
                return new NotFoundObjectResult("Subjects Not Found");
            }
            return new OkObjectResult(subjects);
        }
        public async Task<IActionResult> CreateSubjectAsync(SubjectDto subjectDto , int CourseId)
        {

            var course = await _db.Courses.FirstOrDefaultAsync(u => u.CourseId == CourseId);

            if (course == null)
            {
                return new NotFoundObjectResult("Course Not Found");
            }

            if (_db.Subjects.Any(u => u.SubjectName_ka == subjectDto.SubjectName_ka && u.CourseId == course.CourseId))
            {
                return new BadRequestObjectResult("Subject Already Exists");
            }

            SubjectModel subjectModel = _mapper.Map<SubjectModel>(subjectDto);
            subjectModel.CourseId = course.CourseId;


            try
            {
                _db.Subjects.Add(subjectModel);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
            return new OkObjectResult(subjectModel);
        }

        public async Task<IActionResult> DeleteSubjectAsync(int subjectId)
        {
            try
            {
                var subject = await _db.Subjects.FindAsync(subjectId);

                if (subject == null)
                {
                    return new NotFoundObjectResult("Subject Not Found");
                }

                _db.Subjects.Remove(subject);
                await _db.SaveChangesAsync();

                return new OkObjectResult("Subject and associated entities removed");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }

        public async Task<IActionResult> GetAllSubjectsAsync()
        {
            var subject  = await _db.Subjects.ToListAsync();
            if(subject == null)
            {
                return new NotFoundObjectResult("No Subjects Found");
            }
            return new OkObjectResult(subject);
        }

        public async Task<IActionResult> GetSubjectByIdAsync(int subjectId)
        {
           var subject = await _db.Subjects.FirstOrDefaultAsync(u => u.SubjectId == subjectId);

            if (subject == null)
            {
                return new NotFoundObjectResult("Subject Not Found");
            }
            return new OkObjectResult(subject);
        }

        public async Task<IActionResult> UpdateSubjectAsync(int subjectId, SubjectDto subjectDto)
        {
            var subject = await _db.Subjects.FirstOrDefaultAsync(u => u.SubjectId == subjectId);

            if (subject == null)
            {
                return new NotFoundObjectResult("Subject Not Found");
            }
            subject = _mapper.Map<SubjectModel>(subjectDto);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
            return new OkObjectResult(subject);
        }
    }
}
