using Microsoft.EntityFrameworkCore;
using AsFi.Models;

namespace AsFi.Data
{
    public class AsFiContext : DbContext
    {
        public AsFiContext(DbContextOptions<AsFiContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<GroupJoinCode> GroupJoinCodes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<LectureType> LectureTypes { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<AvailableLecture> AvailableLectures { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<AvailableTest> AvailableTests { get; set; }
        public DbSet<TestQuestion> TestQuestions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<TestAttemptResult> TestAttemptResults { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Login).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<Group>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<UserGroup>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.User).WithMany(x => x.UserGroups).HasForeignKey(x => x.UserId);
                e.HasOne(x => x.Group).WithMany(x => x.UserGroups).HasForeignKey(x => x.GroupId);
            });

            modelBuilder.Entity<GroupJoinCode>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.User).WithMany(x => x.GroupJoinCodes).HasForeignKey(x => x.UserId);
                e.HasOne(x => x.Group).WithMany(x => x.GroupJoinCodes).HasForeignKey(x => x.GroupId);
                e.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<AvailableLecture>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Lecture).WithMany(x => x.AvailableLectures).HasForeignKey(x => x.LectureId);
                e.HasOne(x => x.Group).WithMany(x => x.AvailableLectures).HasForeignKey(x => x.GroupId);
            });

            modelBuilder.Entity<AvailableTest>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Test).WithMany(x => x.AvailableTests).HasForeignKey(x => x.TestId);
                e.HasOne(x => x.Group).WithMany(x => x.AvailableTests).HasForeignKey(x => x.GroupId);
            });

            modelBuilder.Entity<TestQuestion>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Test).WithMany(x => x.TestQuestions).HasForeignKey(x => x.TestId);
            });

            modelBuilder.Entity<AnswerOption>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Question).WithMany(x => x.AnswerOptions).HasForeignKey(x => x.QuestionId);
            });

            modelBuilder.Entity<TestAttemptResult>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.User).WithMany(x => x.TestAttemptResults).HasForeignKey(x => x.UserId);
                e.HasOne(x => x.Test).WithMany(x => x.TestAttemptResults).HasForeignKey(x => x.TestId);
            });

            modelBuilder.Entity<StudentAnswer>(e =>
            {
                e.HasKey(x => new { x.TestAttemptResultId, x.QuestionId });
                e.HasOne(x => x.TestAttemptResult).WithMany(x => x.StudentAnswers).HasForeignKey(x => x.TestAttemptResultId);
                e.HasOne(x => x.Question).WithMany(x => x.StudentAnswers).HasForeignKey(x => x.QuestionId);
                e.HasOne(x => x.AnswerOption).WithMany(x => x.StudentAnswers).HasForeignKey(x => x.AnswerOptionId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}