﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Learn.Test
{
    public class LearnModel
    {
        [Key]
        public int LearnId { get; set; }

        [Required]
        public string? LearnName_en { get; set; }

        [Required]
        public string? LearnName_ka { get; set; }

        [Required] 
        public string? Content_en { get; set; }

        [Required]
        public string? Content_ka { get; set; }

        public string? Code { get; set; }

        public int VideoId { get; set; }
        public virtual VideoModel Video { get; set; }

        public int? TestId { get; set; }

        public virtual TestModel? Test { get; set; }

        public int LessonId { get; set; }

        [JsonIgnore]
        public virtual LessonModel? Lesson { get; set; }


    }
}
