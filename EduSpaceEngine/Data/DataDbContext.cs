using EduSpaceEngine.Model;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Model.Learn.Test;
using EduSpaceEngine.Model.Social;
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

            // User and Course Enrollment relationship
            modelBuilder.Entity<CourseEnrollmentModel>()
                .HasKey(ce => new { ce.UserId, ce.CourseId });

            modelBuilder.Entity<CourseEnrollmentModel>()
                .HasOne(ce => ce.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(ce => ce.UserId)
                .OnDelete(DeleteBehavior.Cascade); // When User is deleted, their enrollments are deleted

            modelBuilder.Entity<CourseEnrollmentModel>()
                .HasOne(ce => ce.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(ce => ce.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // When Course is deleted, its enrollments are deleted

            // Level → Course
            modelBuilder.Entity<LevelModel>()
                .HasMany(l => l.Courses)
                .WithOne(c => c.Level)
                .HasForeignKey(c => c.LevelId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Level to Courses

            // Course → Subject
            modelBuilder.Entity<CourseModel>()
                .HasMany(c => c.Subjects)
                .WithOne(s => s.Course)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Course to Subjects

            // Subject → Lesson
            modelBuilder.Entity<SubjectModel>()
                .HasMany(s => s.Lessons)
                .WithOne(l => l.Subject)
                .HasForeignKey(l => l.SubjectId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Subject to Lessons

            // Lesson → LearnMaterial
            modelBuilder.Entity<LessonModel>()
                .HasMany(l => l.LearnMaterial)
                .WithOne(learn => learn.Lesson)
                .HasForeignKey(learn => learn.LessonId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Lesson to LearnMaterials

            // LearnModel → Test
            modelBuilder.Entity<LearnModel>()
                .HasOne(l => l.Test)
                .WithOne(test => test.Learn)
                .HasForeignKey<TestModel>(test => test.LearnId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from LearnModel to Test

            // Test → TestOptions
            modelBuilder.Entity<TestModel>()
                .HasMany(test => test.Answers)
                .WithOne(option => option.Test)
                .HasForeignKey(option => option.TestId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Test to TestOption

            // User → Post relationship
            modelBuilder.Entity<PostModel>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)  // Assuming UserModel has a collection of Posts
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from User to Post

            // Post → Comment relationship
            modelBuilder.Entity<PostModel>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Post to Comments

            // User → Comment relationship
            modelBuilder.Entity<CommentModel>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)  // Assuming UserModel has a collection of Comments
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from User to Comments

            // User → Notification relationship
            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)  // Assuming UserModel has a collection of Notifications
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from User to Notifications

            // Post → Notification relationship
            modelBuilder.Entity<NotificationModel>()
                .HasOne<PostModel>()
                .WithMany() // No inverse navigation property required
                .HasForeignKey(n => n.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete from Post to Notifications

        }



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
