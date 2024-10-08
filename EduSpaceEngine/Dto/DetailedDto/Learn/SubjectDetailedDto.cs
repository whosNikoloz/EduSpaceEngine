﻿using EduSpaceEngine.Model.Learn;

namespace EduSpaceEngine.Dto.Learn.DetailedDto.Learn
{
    public class SubjectDetailedDto
    {

        public string? SubjectName_ka { get; set; }
        public string? SubjectName_en { get; set; }

        public string? Description_ka { get; set; }
        public string? Description_en { get; set; }

        public string? LogoURL { get; set; }

        public virtual ICollection<LessonModel>? Lessons { get; set; } = new List<LessonModel>();

    }
}
