namespace EduNexus.Api.Common.DTOs;

// ----- Auth / User (FT-14) -----
public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record GoogleLoginRequest(string IdToken);
public record AuthResponse(Guid UserId, string Email, string DisplayName, string AccessToken);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
public record UserProfileDto(Guid Id, string Email, string DisplayName, string? AvatarUrl);

// ----- Student Dashboard -----
public record DashboardDto(
    IEnumerable<EnrolledCourseDto> Courses,
    IEnumerable<EnrolledClassDto> Classes,
    IEnumerable<ActivePackageDto> Packages);
public record EnrolledCourseDto(Guid CourseId, string Title, double ProgressPercent);
public record EnrolledClassDto(Guid ClassId, string Name, DateTime? EndDate);
public record ActivePackageDto(Guid SubscriptionId, string PackageName, DateTime ExpiresAt);

// ----- Personal Progress (FT-12) -----
public record PersonalProgressDto(
    Guid CourseId,
    double LessonCompletionPercent,
    IEnumerable<ModuleProgressDto> Modules,
    IEnumerable<QuizScorePointDto> QuizTrend);
public record ModuleProgressDto(Guid ModuleId, string Title, double CompletionPercent);
public record QuizScorePointDto(DateTime Date, decimal Score);

// ----- Course List / Course Structure (FT-01) -----
public record CourseListItemDto(Guid Id, string Title, string Status, bool IsVisible);
public record CourseStructureDto(Guid CourseId, string Title, IEnumerable<ModuleNodeDto> Modules);
public record ModuleNodeDto(Guid ModuleId, string Title, int OrderIndex,
    int LessonCount, int QuestionCount, int FlashcardCount, int AssignmentCount);
