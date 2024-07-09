using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Model.Learn.Test;
using EduSpaceEngine.Model.Social;
using EduSpaceEngine.Model.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace EduSpaceEngine.Data
{
    public class DataDbContext : DbContext
    {
        public DataDbContext(DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User and Course Enrollment
            modelBuilder.Entity<CourseEnrollmentModel>()
                .HasKey(ce => new { ce.UserId, ce.CourseId });

            modelBuilder.Entity<CourseEnrollmentModel>()
                .HasOne(ce => ce.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(ce => ce.UserId);

            modelBuilder.Entity<CourseEnrollmentModel>()
                .HasOne(ce => ce.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(ce => ce.CourseId);

            modelBuilder.Entity<ProgressModel>()
                .HasOne(p => p.Lesson)
                .WithMany()
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProgressModel>()
                .HasOne(p => p.Subject)
                .WithMany()
                .HasForeignKey(p => p.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LearnModel>()
                .HasOne(learn => learn.Test)        
                .WithOne(test => test.Learn)       
                .HasForeignKey<LearnModel>(learn => learn.TestId);

            modelBuilder.Entity<LevelModel>()
                .HasMany(l => l.Courses)
                .WithOne(c => c.Level)
                .HasForeignKey(c => c.LevelId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Level to Course

            modelBuilder.Entity<CourseModel>()
                .HasMany(c => c.Subjects)
                .WithOne(s => s.Course)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Course to Subject

            modelBuilder.Entity<SubjectModel>()
                .HasMany(s => s.Lessons)
                .WithOne(l => l.Subject)
                .HasForeignKey(l => l.SubjectId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Subject to Lesson

            modelBuilder.Entity<LessonModel>()
                .HasMany(l => l.LearnMaterial)
                .WithOne(learn => learn.Lesson)
                .HasForeignKey(learn => learn.LessonId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Lesson to Learn

            modelBuilder.Entity<LearnModel>()
                .HasOne(l => l.Test)
                .WithOne(test => test.Learn)
                .HasForeignKey<TestModel>(test => test.LearnId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Lesson to Test

            modelBuilder.Entity<TestModel>()
                .HasMany(test => test.Answers)
                .WithOne(answer => answer.Test)
                .HasForeignKey(answer => answer.TestId)
                .OnDelete(DeleteBehavior.Cascade);
        }


        //user

        public DbSet<UserModel> Users { get; set; }
        public DbSet<ProgressModel> Progress { get; set; }




        //Social
        public DbSet<PostModel> Posts { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }


        //Learn 
        public DbSet<LevelModel> Levels { get; set; }
        public DbSet<CourseModel> Courses { get; set; }
        public DbSet<SubjectModel> Subjects { get; set; }
        public DbSet<CourseEnrollmentModel> CourseEnroll { get; set; }
        public DbSet<LessonModel> Lessons { get; set; }
        public DbSet<LearnModel> Learn { get; set; }
        public DbSet<TestModel> Tests { get; set; }
        public DbSet<TestAnswerModel> TestAnswers { get; set; }
        public DbSet<VideoModel> Videos { get; set; }

    }
}
