/* =====================================================================
   EduNexus — Nền tảng Học tập & Đào tạo Tích hợp AI
   Script tạo Database cho TOÀN BỘ hệ thống (SQL Server / T-SQL)
   Nhóm: G5  |  Deliverable: GP1
   Tham chiếu: 01_EduNexus_VS_v2.pdf, 02_EduNexus_SRS_v2.pdf
   ---------------------------------------------------------------------
   Cách chạy:
     sqlcmd -S <server> -i EduNexus_CreateDatabase.sql
   hoặc mở bằng SSMS và Execute.
   ===================================================================== */

/* ---------- 0. Tạo Database ---------- */
IF DB_ID(N'EduNexus') IS NOT NULL
BEGIN
    ALTER DATABASE EduNexus SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EduNexus;
END
GO
CREATE DATABASE EduNexus;
GO
USE EduNexus;
GO

/* =====================================================================
   NHÓM 1 — AUTH / USER (FT-14, FT-15)
   ===================================================================== */

CREATE TABLE Roles (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(50)  NOT NULL UNIQUE,   -- Admin, SME, Teacher, CourseManager, Student
    Description NVARCHAR(255) NULL
);
GO

CREATE TABLE Users (
    Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email           NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(512) NULL,           -- NULL nếu là tài khoản Google (BR-21)
    DisplayName     NVARCHAR(150) NOT NULL,
    AvatarUrl       NVARCHAR(512) NULL,
    AuthProvider    NVARCHAR(20)  NOT NULL DEFAULT N'Local', -- Local | Google
    GoogleSubject   NVARCHAR(128) NULL,           -- sub từ Google OAuth
    IsEmailVerified BIT           NOT NULL DEFAULT 0,
    IsActive        BIT           NOT NULL DEFAULT 1,
    AiTokenQuota    BIGINT        NULL,            -- hạn mức token/tháng (SME, Teacher)
    AiTokenUsed     BIGINT        NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2     NULL,
    LastLoginAt     DATETIME2     NULL
);
GO

CREATE TABLE UserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId INT              NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE LoginSessions (
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId       UNIQUEIDENTIFIER NOT NULL,
    DeviceInfo   NVARCHAR(256) NULL,
    IpAddress    NVARCHAR(64)  NULL,
    CreatedAt    DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    ExpiresAt    DATETIME2     NULL,
    RevokedAt    DATETIME2     NULL,
    CONSTRAINT FK_LoginSessions_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

CREATE TABLE PasswordResetTokens (
    Id        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId    UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(512) NOT NULL,
    ExpiresAt DATETIME2     NOT NULL,             -- hiệu lực 1 giờ (BR-22)
    UsedAt    DATETIME2     NULL,
    CreatedAt DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PwdReset_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

CREATE TABLE EmailVerificationTokens (
    Id        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId    UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(512) NOT NULL,
    ExpiresAt DATETIME2     NOT NULL,
    UsedAt    DATETIME2     NULL,
    CreatedAt DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_EmailVerify_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

/* =====================================================================
   NHÓM 2 — CONTENT: COURSE / MODULE / LESSON (FT-01, FT-02)
   ===================================================================== */

CREATE TABLE CourseGroups (                       -- nhóm khóa học (FT-10)
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name        NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE Courses (
    Id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title         NVARCHAR(250) NOT NULL,
    Description   NVARCHAR(2000) NULL,
    OwnerSmeId    UNIQUEIDENTIFIER NULL,           -- SME được phân công (BR-01)
    Status        NVARCHAR(20) NOT NULL DEFAULT N'Draft', -- Draft | Published | Locked
    IsVisible     BIT NOT NULL DEFAULT 1,          -- bật/tắt hiển thị trên danh mục
    Version       INT NOT NULL DEFAULT 1,          -- lịch sử xuất bản (AC-01d)
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt     DATETIME2 NULL,
    PublishedAt   DATETIME2 NULL,
    CONSTRAINT FK_Courses_Sme FOREIGN KEY (OwnerSmeId) REFERENCES Users(Id)
);
GO

CREATE TABLE CourseGroupCourses (                 -- N-N CourseGroup <-> Course
    CourseGroupId UNIQUEIDENTIFIER NOT NULL,
    CourseId      UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_CourseGroupCourses PRIMARY KEY (CourseGroupId, CourseId),
    CONSTRAINT FK_CGC_Group  FOREIGN KEY (CourseGroupId) REFERENCES CourseGroups(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CGC_Course FOREIGN KEY (CourseId)      REFERENCES Courses(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Modules (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseId    UNIQUEIDENTIFIER NOT NULL,
    Title       NVARCHAR(250) NOT NULL,
    OrderIndex  INT NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Modules_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Lessons (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModuleId    UNIQUEIDENTIFIER NOT NULL,
    Title       NVARCHAR(250) NOT NULL,
    OrderIndex  INT NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2 NULL,
    CONSTRAINT FK_Lessons_Module FOREIGN KEY (ModuleId) REFERENCES Modules(Id) ON DELETE CASCADE
);
GO

CREATE TABLE LessonContents (                     -- video / markdown / file (FT-02)
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    LessonId     UNIQUEIDENTIFIER NOT NULL,
    ContentType  NVARCHAR(20) NOT NULL,            -- Markdown | YoutubeVideo | File
    MarkdownText NVARCHAR(MAX) NULL,
    YoutubeUrl   NVARCHAR(512) NULL,
    VideoSummary NVARCHAR(MAX) NULL,               -- tóm tắt AI từ transcript
    FileUrl      NVARCHAR(512) NULL,
    FileName     NVARCHAR(256) NULL,
    OrderIndex   INT NOT NULL DEFAULT 0,
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LessonContents_Lesson FOREIGN KEY (LessonId) REFERENCES Lessons(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AiLessonDrafts (                     -- AI Lesson Staging
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    LessonId    UNIQUEIDENTIFIER NULL,
    CreatedById UNIQUEIDENTIFIER NOT NULL,
    SourceText  NVARCHAR(MAX) NULL,                -- đề cương/transcript nguồn
    GeneratedText NVARCHAR(MAX) NULL,              -- kết quả AI sinh
    Status      NVARCHAR(20) NOT NULL DEFAULT N'Pending', -- Pending | Approved | Rejected
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AiLessonDrafts_Lesson FOREIGN KEY (LessonId) REFERENCES Lessons(Id),
    CONSTRAINT FK_AiLessonDrafts_User   FOREIGN KEY (CreatedById) REFERENCES Users(Id)
);
GO

/* =====================================================================
   NHÓM 3 — QUESTION BANK (FT-03)
   ===================================================================== */

CREATE TABLE Questions (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModuleId    UNIQUEIDENTIFIER NOT NULL,
    Content     NVARCHAR(MAX) NOT NULL,
    Explanation NVARCHAR(MAX) NULL,
    Difficulty  NVARCHAR(20) NOT NULL DEFAULT N'Medium', -- Easy | Medium | Hard
    Status      NVARCHAR(20) NOT NULL DEFAULT N'Active',  -- Active | Archived
    CreatedById UNIQUEIDENTIFIER NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Questions_Module FOREIGN KEY (ModuleId) REFERENCES Modules(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Questions_User   FOREIGN KEY (CreatedById) REFERENCES Users(Id)
);
GO

CREATE TABLE QuestionOptions (                    -- 2..6 đáp án, đúng 1 (AC-03a)
    Id         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    QuestionId UNIQUEIDENTIFIER NOT NULL,
    Content    NVARCHAR(1000) NOT NULL,
    IsCorrect  BIT NOT NULL DEFAULT 0,
    OrderIndex INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_QuestionOptions_Question FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AiQuestionDrafts (                   -- AI Question Staging (BR-07)
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModuleId     UNIQUEIDENTIFIER NULL,
    CreatedById  UNIQUEIDENTIFIER NOT NULL,
    SourceText   NVARCHAR(MAX) NULL,
    GeneratedJson NVARCHAR(MAX) NULL,              -- câu hỏi + đáp án dạng JSON chờ duyệt
    Status       NVARCHAR(20) NOT NULL DEFAULT N'Pending',
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AiQDrafts_Module FOREIGN KEY (ModuleId) REFERENCES Modules(Id),
    CONSTRAINT FK_AiQDrafts_User   FOREIGN KEY (CreatedById) REFERENCES Users(Id)
);
GO

/* =====================================================================
   NHÓM 4 — FLASHCARD (FT-04, FT-06)
   ===================================================================== */

CREATE TABLE FlashcardGroups (
    Id         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModuleId   UNIQUEIDENTIFIER NOT NULL,          -- BR-19: nhóm thẻ thuộc 1 module
    Name       NVARCHAR(200) NOT NULL,
    OrderIndex INT NOT NULL DEFAULT 0,
    CreatedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_FlashcardGroups_Module FOREIGN KEY (ModuleId) REFERENCES Modules(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Flashcards (
    Id         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GroupId    UNIQUEIDENTIFIER NULL,              -- NULL = "Chưa phân nhóm"
    ModuleId   UNIQUEIDENTIFIER NOT NULL,
    FrontText  NVARCHAR(500) NOT NULL,             -- thuật ngữ (<=500 ký tự AC-04b)
    BackText   NVARCHAR(500) NOT NULL,             -- định nghĩa
    OrderIndex INT NOT NULL DEFAULT 0,
    CreatedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Flashcards_Group  FOREIGN KEY (GroupId)  REFERENCES FlashcardGroups(Id),
    CONSTRAINT FK_Flashcards_Module FOREIGN KEY (ModuleId) REFERENCES Modules(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AiFlashcardDrafts (
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModuleId     UNIQUEIDENTIFIER NULL,
    CreatedById  UNIQUEIDENTIFIER NOT NULL,
    SourceText   NVARCHAR(MAX) NULL,
    GeneratedJson NVARCHAR(MAX) NULL,
    Status       NVARCHAR(20) NOT NULL DEFAULT N'Pending',
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AiFDrafts_Module FOREIGN KEY (ModuleId) REFERENCES Modules(Id),
    CONSTRAINT FK_AiFDrafts_User   FOREIGN KEY (CreatedById) REFERENCES Users(Id)
);
GO

CREATE TABLE FlashcardProgress (                  -- tiến độ ôn thẻ của học viên
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FlashcardId UNIQUEIDENTIFIER NOT NULL,
    StudentId   UNIQUEIDENTIFIER NOT NULL,
    IsLearned   BIT NOT NULL DEFAULT 0,            -- "Đã thuộc" / "Chưa thuộc"
    LastReviewedAt DATETIME2 NULL,
    CONSTRAINT FK_FProgress_Card    FOREIGN KEY (FlashcardId) REFERENCES Flashcards(Id) ON DELETE CASCADE,
    CONSTRAINT FK_FProgress_Student FOREIGN KEY (StudentId)   REFERENCES Users(Id),
    CONSTRAINT UQ_FProgress UNIQUE (FlashcardId, StudentId)
);
GO

/* =====================================================================
   NHÓM 5 — ASSIGNMENT (FT-05, FT-08)
   ===================================================================== */

CREATE TABLE Assignments (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModuleId    UNIQUEIDENTIFIER NOT NULL,
    Title       NVARCHAR(250) NOT NULL,
    PromptMarkdown NVARCHAR(MAX) NULL,             -- đề bài (Markdown)
    DueDate     DATETIME2 NULL,
    MaxChars    INT NOT NULL DEFAULT 20000,        -- giới hạn bài nộp
    Status      NVARCHAR(20) NOT NULL DEFAULT N'Draft', -- Draft | Published
    CreatedById UNIQUEIDENTIFIER NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Assignments_Module FOREIGN KEY (ModuleId) REFERENCES Modules(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Assignments_User   FOREIGN KEY (CreatedById) REFERENCES Users(Id)
);
GO

CREATE TABLE RubricCriteria (                     -- tiêu chí chấm, tổng tỷ trọng = 100%
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    Name         NVARCHAR(250) NOT NULL,
    Weight       DECIMAL(5,2) NOT NULL,            -- % > 0 (NAC-05-b)
    MaxScore     DECIMAL(5,2) NOT NULL,
    OrderIndex   INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_RubricCriteria_Assignment FOREIGN KEY (AssignmentId) REFERENCES Assignments(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Submissions (
    Id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AssignmentId  UNIQUEIDENTIFIER NOT NULL,
    StudentId     UNIQUEIDENTIFIER NOT NULL,
    ContentText   NVARCHAR(MAX) NULL,
    SubmittedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Status        NVARCHAR(20) NOT NULL DEFAULT N'Submitted', -- Submitted | AiGraded | Confirmed
    AiTotalScore  DECIMAL(6,2) NULL,               -- điểm AI sơ bộ (chỉ GV xem)
    FinalScore    DECIMAL(6,2) NULL,               -- điểm GV xác nhận (BR-10)
    TeacherComment NVARCHAR(MAX) NULL,
    ConfirmedById UNIQUEIDENTIFIER NULL,
    ConfirmedAt   DATETIME2 NULL,
    CONSTRAINT FK_Submissions_Assignment FOREIGN KEY (AssignmentId) REFERENCES Assignments(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Submissions_Student    FOREIGN KEY (StudentId)    REFERENCES Users(Id),
    CONSTRAINT FK_Submissions_Confirmer  FOREIGN KEY (ConfirmedById) REFERENCES Users(Id),
    CONSTRAINT UQ_Submission_OnePerStudent UNIQUE (AssignmentId, StudentId) -- mỗi HV nộp 1 lần
);
GO

CREATE TABLE SubmissionCriterionScores (          -- điểm từng tiêu chí
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SubmissionId UNIQUEIDENTIFIER NOT NULL,
    CriterionId  UNIQUEIDENTIFIER NOT NULL,
    AiScore      DECIMAL(5,2) NULL,
    FinalScore   DECIMAL(5,2) NULL,
    Comment      NVARCHAR(MAX) NULL,
    CONSTRAINT FK_SCS_Submission FOREIGN KEY (SubmissionId) REFERENCES Submissions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SCS_Criterion  FOREIGN KEY (CriterionId)  REFERENCES RubricCriteria(Id)
);
GO

/* =====================================================================
   NHÓM 6 — QUIZ (FT-07) — bài kiểm tra luyện tập, không tính điểm chính thức
   ===================================================================== */

CREATE TABLE Quizzes (
    Id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StudentId     UNIQUEIDENTIFIER NOT NULL,
    CourseId      UNIQUEIDENTIFIER NULL,
    Title         NVARCHAR(250) NULL,
    QuestionCount INT NOT NULL DEFAULT 0,
    Difficulty    NVARCHAR(20) NULL,
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Quizzes_Student FOREIGN KEY (StudentId) REFERENCES Users(Id),
    CONSTRAINT FK_Quizzes_Course  FOREIGN KEY (CourseId)  REFERENCES Courses(Id)
);
GO

CREATE TABLE QuizQuestions (                      -- snapshot câu hỏi rút vào quiz
    Id         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    QuizId     UNIQUEIDENTIFIER NOT NULL,
    QuestionId UNIQUEIDENTIFIER NOT NULL,
    OrderIndex INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_QuizQuestions_Quiz     FOREIGN KEY (QuizId)     REFERENCES Quizzes(Id) ON DELETE CASCADE,
    CONSTRAINT FK_QuizQuestions_Question FOREIGN KEY (QuestionId) REFERENCES Questions(Id)
);
GO

CREATE TABLE QuizAttempts (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    QuizId      UNIQUEIDENTIFIER NOT NULL,
    StudentId   UNIQUEIDENTIFIER NOT NULL,
    StartedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    SubmittedAt DATETIME2 NULL,
    Score       DECIMAL(6,2) NULL,                 -- chỉ tự đánh giá (BR-06)
    CONSTRAINT FK_QuizAttempts_Quiz    FOREIGN KEY (QuizId)    REFERENCES Quizzes(Id) ON DELETE CASCADE,
    CONSTRAINT FK_QuizAttempts_Student FOREIGN KEY (StudentId) REFERENCES Users(Id)
);
GO

CREATE TABLE QuizAttemptAnswers (
    Id               UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AttemptId        UNIQUEIDENTIFIER NOT NULL,
    QuestionId       UNIQUEIDENTIFIER NOT NULL,
    SelectedOptionId UNIQUEIDENTIFIER NULL,
    IsCorrect        BIT NULL,
    CONSTRAINT FK_QAA_Attempt  FOREIGN KEY (AttemptId)  REFERENCES QuizAttempts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_QAA_Question FOREIGN KEY (QuestionId) REFERENCES Questions(Id)
);
GO

/* =====================================================================
   NHÓM 7 — CLASS & GHI DANH (FT-09)
   ===================================================================== */

CREATE TABLE Classes (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseId    UNIQUEIDENTIFIER NOT NULL,
    Name        NVARCHAR(250) NOT NULL,
    TeacherId   UNIQUEIDENTIFIER NULL,
    StartDate   DATE NULL,
    EndDate     DATE NULL,
    MaxStudents INT NOT NULL DEFAULT 500,          -- LI-07
    Fee         DECIMAL(12,2) NOT NULL DEFAULT 0,  -- 0 = miễn phí
    Status      NVARCHAR(20) NOT NULL DEFAULT N'Open',
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Classes_Course  FOREIGN KEY (CourseId)  REFERENCES Courses(Id),
    CONSTRAINT FK_Classes_Teacher FOREIGN KEY (TeacherId) REFERENCES Users(Id)
);
GO

CREATE TABLE ClassMaterials (                     -- tài liệu riêng GV bổ sung (BR-02)
    Id        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClassId   UNIQUEIDENTIFIER NOT NULL,
    Title     NVARCHAR(250) NOT NULL,
    ContentType NVARCHAR(20) NOT NULL,
    Url       NVARCHAR(512) NULL,
    MarkdownText NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ClassMaterials_Class FOREIGN KEY (ClassId) REFERENCES Classes(Id) ON DELETE CASCADE
);
GO

CREATE TABLE ClassStudents (
    Id         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClassId    UNIQUEIDENTIFIER NOT NULL,
    StudentId  UNIQUEIDENTIFIER NOT NULL,
    JoinedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Status     NVARCHAR(20) NOT NULL DEFAULT N'Active', -- Active | Removed
    CONSTRAINT FK_ClassStudents_Class   FOREIGN KEY (ClassId)   REFERENCES Classes(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ClassStudents_Student FOREIGN KEY (StudentId) REFERENCES Users(Id),
    CONSTRAINT UQ_ClassStudents UNIQUE (ClassId, StudentId)
);
GO

/* =====================================================================
   NHÓM 8 — CATALOG / PRICING / ĐĂNG KÝ / THANH TOÁN (FT-10, FT-11)
   ===================================================================== */

CREATE TABLE CoursePrices (                        -- H1: mua lẻ vĩnh viễn
    Id        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseId  UNIQUEIDENTIFIER NOT NULL,
    Price     DECIMAL(12,2) NOT NULL,
    IsActive  BIT NOT NULL DEFAULT 1,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CoursePrices_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Packages (                            -- H3: gói nhóm khóa học theo thời hạn
    Id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseGroupId UNIQUEIDENTIFIER NOT NULL,
    Name          NVARCHAR(200) NOT NULL,
    DurationMonths INT NOT NULL,                   -- 1 | 3 | 6 | 12
    Price         DECIMAL(12,2) NOT NULL,
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Packages_Group FOREIGN KEY (CourseGroupId) REFERENCES CourseGroups(Id) ON DELETE CASCADE
);
GO

CREATE TABLE PackageSubscriptions (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PackageId   UNIQUEIDENTIFIER NOT NULL,
    StudentId   UNIQUEIDENTIFIER NOT NULL,
    StartAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ExpiresAt   DATETIME2 NOT NULL,                -- gia hạn cộng dồn (AC-10b)
    Status      NVARCHAR(20) NOT NULL DEFAULT N'Active', -- Active | Expired
    CONSTRAINT FK_PkgSub_Package FOREIGN KEY (PackageId) REFERENCES Packages(Id),
    CONSTRAINT FK_PkgSub_Student FOREIGN KEY (StudentId) REFERENCES Users(Id)
);
GO

CREATE TABLE AccessGrants (                        -- quyền truy cập nội dung (H1/H2/H3)
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StudentId    UNIQUEIDENTIFIER NOT NULL,
    CourseId     UNIQUEIDENTIFIER NULL,
    GrantType    NVARCHAR(10) NOT NULL,            -- H1 | H2 | H3
    SourceRefId  UNIQUEIDENTIFIER NULL,            -- Class/Package/Payment liên quan
    GrantedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ExpiresAt    DATETIME2 NULL,                   -- NULL = vĩnh viễn (H1)
    IsRevoked    BIT NOT NULL DEFAULT 0,           -- thu hồi nhưng giữ lịch sử (BR-18)
    CONSTRAINT FK_AccessGrants_Student FOREIGN KEY (StudentId) REFERENCES Users(Id),
    CONSTRAINT FK_AccessGrants_Course  FOREIGN KEY (CourseId)  REFERENCES Courses(Id)
);
GO

CREATE TABLE Payments (
    Id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StudentId     UNIQUEIDENTIFIER NOT NULL,
    Amount        DECIMAL(12,2) NOT NULL,
    Provider      NVARCHAR(20) NOT NULL,           -- VNPay | SePay
    PurchaseType  NVARCHAR(10) NOT NULL,           -- H1 | H2 | H3
    RefId         UNIQUEIDENTIFIER NULL,           -- Course/Class/Package
    ProviderTxnId NVARCHAR(128) NULL,
    Status        NVARCHAR(20) NOT NULL DEFAULT N'Pending', -- Pending | Success | Failed | Refunded
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    PaidAt        DATETIME2 NULL,
    CONSTRAINT FK_Payments_Student FOREIGN KEY (StudentId) REFERENCES Users(Id)
);
GO

/* =====================================================================
   NHÓM 9 — PROGRESS / ANALYTICS / LOG / NOTIFICATION (FT-12, FT-13, FT-15)
   ===================================================================== */

CREATE TABLE LessonProgress (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    LessonId    UNIQUEIDENTIFIER NOT NULL,
    StudentId   UNIQUEIDENTIFIER NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    CompletedAt DATETIME2 NULL,
    CONSTRAINT FK_LessonProgress_Lesson  FOREIGN KEY (LessonId)  REFERENCES Lessons(Id) ON DELETE CASCADE,
    CONSTRAINT FK_LessonProgress_Student FOREIGN KEY (StudentId) REFERENCES Users(Id),
    CONSTRAINT UQ_LessonProgress UNIQUE (LessonId, StudentId)
);
GO

CREATE TABLE AiUsageLogs (                         -- giám sát token (VS 4.3)
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId      UNIQUEIDENTIFIER NOT NULL,
    TaskType    NVARCHAR(50) NOT NULL,             -- GenQuestion | GenFlashcard | Summary | GradeEssay ...
    TokensUsed  BIGINT NOT NULL DEFAULT 0,
    DurationMs  INT NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AiUsageLogs_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE TABLE ActivityLogs (                        -- nhật ký hệ thống (BR-25, không xóa qua UI)
    Id         BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId     UNIQUEIDENTIFIER NULL,
    Action     NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100) NULL,
    EntityId   NVARCHAR(64) NULL,
    Result     NVARCHAR(50) NULL,
    Detail     NVARCHAR(MAX) NULL,
    CreatedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ActivityLogs_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE TABLE Notifications (                        -- danh mục thông báo NTF-01..15
    Id         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId     UNIQUEIDENTIFIER NOT NULL,
    Type       NVARCHAR(50) NOT NULL,
    Title      NVARCHAR(250) NOT NULL,
    Message    NVARCHAR(MAX) NULL,
    Channel    NVARCHAR(20) NOT NULL DEFAULT N'InApp', -- InApp | Email | Push
    IsRead     BIT NOT NULL DEFAULT 0,
    CreatedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

/* =====================================================================
   INDEX cơ bản
   ===================================================================== */
CREATE INDEX IX_Modules_Course        ON Modules(CourseId);
CREATE INDEX IX_Lessons_Module        ON Lessons(ModuleId);
CREATE INDEX IX_Questions_Module      ON Questions(ModuleId);
CREATE INDEX IX_Flashcards_Module     ON Flashcards(ModuleId);
CREATE INDEX IX_Submissions_Assignment ON Submissions(AssignmentId);
CREATE INDEX IX_AccessGrants_Student  ON AccessGrants(StudentId);
CREATE INDEX IX_Payments_Student      ON Payments(StudentId);
CREATE INDEX IX_Notifications_User    ON Notifications(UserId);
GO

/* =====================================================================
   SEED dữ liệu nền — Roles
   ===================================================================== */
INSERT INTO Roles (Name, Description) VALUES
  (N'Admin',         N'Quản trị viên toàn hệ thống'),
  (N'SME',           N'Chuyên gia nội dung'),
  (N'Teacher',       N'Giảng viên'),
  (N'CourseManager', N'Người quản lý khóa học'),
  (N'Student',       N'Học viên');
GO

PRINT N'>>> EduNexus database created successfully.';
GO

