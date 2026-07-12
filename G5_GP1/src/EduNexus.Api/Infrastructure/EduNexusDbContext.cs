using EduNexus.Api.Assignment.Entities;
using EduNexus.Api.Common.Entities;
using EduNexus.Api.Flashcard.Entities;
using EduNexus.Api.Lesson.Entities;
using EduNexus.Api.Question.Entities;
using EduNexus.Api.Quiz.Entities;
using Microsoft.EntityFrameworkCore;
using LessonEntity = EduNexus.Api.Lesson.Entities.Lesson;

namespace EduNexus.Api.Infrastructure;

/// <summary>
/// DbContext ánh xạ TOÀN BỘ hệ thống EduNexus.
/// Tên bảng khớp với database/EduNexus_CreateDatabase.sql.
/// </summary>
public class EduNexusDbContext : DbContext
{
    public EduNexusDbContext(DbContextOptions<EduNexusDbContext> options) : base(options) { }

    // --- Auth / User ---
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<LoginSession> LoginSessions => Set<LoginSession>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    // --- Content: Course / Module ---
    public DbSet<CourseGroup> CourseGroups => Set<CourseGroup>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseGroupCourse> CourseGroupCourses => Set<CourseGroupCourse>();
    public DbSet<Module> Modules => Set<Module>();

    // --- Lesson ---
    public DbSet<LessonEntity> Lessons => Set<LessonEntity>();
    public DbSet<LessonContent> LessonContents => Set<LessonContent>();
    public DbSet<AiLessonDraft> AiLessonDrafts => Set<AiLessonDraft>();

    // --- Question ---
    public DbSet<Question.Entities.Question> Questions => Set<Question.Entities.Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<AiQuestionDraft> AiQuestionDrafts => Set<AiQuestionDraft>();

    // --- Flashcard ---
    public DbSet<FlashcardGroup> FlashcardGroups => Set<FlashcardGroup>();
    public DbSet<FlashcardItem> Flashcards => Set<FlashcardItem>();
    public DbSet<AiFlashcardDraft> AiFlashcardDrafts => Set<AiFlashcardDraft>();
    public DbSet<FlashcardProgress> FlashcardProgress => Set<FlashcardProgress>();

    // --- Assignment ---
    public DbSet<AssignmentItem> Assignments => Set<AssignmentItem>();
    public DbSet<RubricCriterion> RubricCriteria => Set<RubricCriterion>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<SubmissionCriterionScore> SubmissionCriterionScores => Set<SubmissionCriterionScore>();

    // --- Quiz ---
    public DbSet<QuizItem> Quizzes => Set<QuizItem>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();

    // --- Class & ghi danh ---
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassMaterial> ClassMaterials => Set<ClassMaterial>();
    public DbSet<ClassStudent> ClassStudents => Set<ClassStudent>();

    // --- Catalog / Pricing / Payment ---
    public DbSet<CoursePrice> CoursePrices => Set<CoursePrice>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<PackageSubscription> PackageSubscriptions => Set<PackageSubscription>();
    public DbSet<AccessGrant> AccessGrants => Set<AccessGrant>();
    public DbSet<Payment> Payments => Set<Payment>();

    // --- Progress / Analytics / Log / Notification ---
    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();
    public DbSet<AiUsageLog> AiUsageLogs => Set<AiUsageLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public IEnumerable<object> FlashcardProgresses { get; internal set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Tên bảng khớp SQL script (class name khác table name ở 3 entity dưới)
        b.Entity<FlashcardItem>().ToTable("Flashcards");
        b.Entity<AssignmentItem>().ToTable("Assignments");
        b.Entity<QuizItem>().ToTable("Quizzes");

        // Khóa chính kép
        b.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        b.Entity<CourseGroupCourse>().HasKey(x => new { x.CourseGroupId, x.CourseId });

        // Ràng buộc duy nhất phản ánh nghiệp vụ
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        b.Entity<Submission>().HasIndex(x => new { x.AssignmentId, x.StudentId }).IsUnique();   // mỗi HV nộp 1 lần
        b.Entity<ClassStudent>().HasIndex(x => new { x.ClassId, x.StudentId }).IsUnique();
        b.Entity<FlashcardProgress>().HasIndex(x => new { x.FlashcardId, x.StudentId }).IsUnique();
        b.Entity<LessonProgress>().HasIndex(x => new { x.LessonId, x.StudentId }).IsUnique();

        base.OnModelCreating(b);
    }
}
