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

-- BR-13/BR-15: a Course Manager works only within course groups assigned by Admin.
CREATE TABLE CourseGroupManagers (
    CourseGroupId UNIQUEIDENTIFIER NOT NULL,
    UserId        UNIQUEIDENTIFIER NOT NULL,
    AssignedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    AssignedById  UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_CourseGroupManagers PRIMARY KEY (CourseGroupId, UserId),
    CONSTRAINT FK_CGM_Group FOREIGN KEY (CourseGroupId) REFERENCES CourseGroups(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CGM_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_CGM_AssignedBy FOREIGN KEY (AssignedById) REFERENCES Users(Id)
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
    ClassId     UNIQUEIDENTIFIER NULL,             -- NULL = source assignment; set = class-specific assignment
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

-- Payment gateways can retry a webhook; the provider transaction reference must be idempotent.
CREATE UNIQUE INDEX UX_Payments_ProviderTxnId ON Payments(ProviderTxnId) WHERE ProviderTxnId IS NOT NULL;
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

-- Classes are defined after assignments in this script, so add the optional FK here.
ALTER TABLE Assignments ADD CONSTRAINT FK_Assignments_Class
    FOREIGN KEY (ClassId) REFERENCES Classes(Id);
GO

-- SCR-08 / CFG-Bxx. Values are managed by Admin and read by services/jobs.
CREATE TABLE SystemConfigs (
    ConfigKey    NVARCHAR(100) PRIMARY KEY,
    ConfigValue  NVARCHAR(MAX) NOT NULL,
    Description  NVARCHAR(500) NULL,
    UpdatedById  UNIQUEIDENTIFIER NULL,
    UpdatedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_SystemConfigs_UpdatedBy FOREIGN KEY (UpdatedById) REFERENCES Users(Id)
);
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

/* =====================================================================
   SEED DATA - EDUNEXUS
   Tự động sinh ID sạch, dữ liệu dồi dào cho các Role.
   ===================================================================== */

-- KHAI BÁO CÁC BIẾN ID
DECLARE @AdminId UNIQUEIDENTIFIER = NEWID();
DECLARE @SmeId UNIQUEIDENTIFIER = NEWID();
DECLARE @TeacherId UNIQUEIDENTIFIER = NEWID();
DECLARE @Student1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Student2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Student3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @CourseManagerId UNIQUEIDENTIFIER = NEWID();

-- 1. USERS
INSERT INTO Users (Id, Email, DisplayName, AuthProvider, IsActive, AiTokenQuota) VALUES 
(@AdminId, 'admin@edunexus.vn', N'Quản trị viên', 'Local', 1, 999999),
(@SmeId, 'sme@edunexus.vn', N'SME Nguyễn Văn A', 'Local', 1, 500000),
(@TeacherId, 'teacher@edunexus.vn', N'Giảng viên Trần B', 'Local', 1, 100000),
(@Student1Id, 'student1@edunexus.vn', N'Học viên Lê Thị C', 'Local', 1, 0),
(@Student2Id, 'student2@edunexus.vn', N'Học viên Phạm D', 'Local', 1, 0);

-- GÁN ROLE
-- The web demo uses these accounts.  The ids remain GUIDs for relational safety;
-- users select names/emails in the UI and never have to type an id.
UPDATE Users SET Email = 'sme.nguyen@edunexus.vn', DisplayName = N'SME Nguyen Minh An' WHERE Id = @SmeId;
UPDATE Users SET Email = 'teacher.tran@edunexus.vn', DisplayName = N'Giang vien Tran Hai Nam' WHERE Id = @TeacherId;
UPDATE Users SET Email = 'student.le@edunexus.vn', DisplayName = N'Hoc vien Le Mai Anh' WHERE Id = @Student1Id;
UPDATE Users SET Email = 'student.pham@edunexus.vn', DisplayName = N'Hoc vien Pham Quoc Bao' WHERE Id = @Student2Id;
INSERT INTO Users (Id, Email, DisplayName, AuthProvider, IsActive, AiTokenQuota)
VALUES (@CourseManagerId, 'manager.do@edunexus.vn', N'Course Manager Do Thu Ha', 'Local', 1, 100000),
       (@Student3Id, 'student.hoang@edunexus.vn', N'Hoc vien Hoang Minh Chau', 'Local', 1, 0);

INSERT INTO UserRoles (UserId, RoleId) VALUES 
(@AdminId, 1),    -- Admin
(@SmeId, 2),      -- SME
(@TeacherId, 3),  -- Teacher
(@CourseManagerId, 4), -- Course manager
(@Student1Id, 5), -- Student
(@Student2Id, 5), -- Student
(@Student3Id, 5); -- Student

-- 2. COURSE GROUPS & COURSES
DECLARE @CourseGroup1 UNIQUEIDENTIFIER = NEWID();
INSERT INTO CourseGroups (Id, Name, Description) VALUES 
(@CourseGroup1, N'Lập trình Cơ bản', N'Các khóa học nền tảng cho người mới bắt đầu');

DECLARE @Course1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Course2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Courses (Id, Title, Description, OwnerSmeId, Status) VALUES 
(@Course1, N'Nhập môn Kỹ thuật Phần mềm (SE)', N'Khóa học SDLC, Agile, Requirement.', @SmeId, 'Published'),
(@Course2, N'Lập trình C# Cơ bản', N'Ngôn ngữ C# và .NET Framework.', @SmeId, 'Published');

INSERT INTO CourseGroupCourses (CourseGroupId, CourseId) VALUES 
(@CourseGroup1, @Course1), (@CourseGroup1, @Course2);
INSERT INTO CourseGroupManagers (CourseGroupId, UserId, AssignedById) VALUES
(@CourseGroup1, @CourseManagerId, @AdminId);

-- 3. MODULES
DECLARE @Module1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Module2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Modules (Id, CourseId, Title, OrderIndex) VALUES 
(@Module1, @Course1, N'Chương 1: Tổng quan SDLC', 1),
(@Module2, @Course1, N'Chương 2: Agile & Scrum', 2);

-- 4. LESSONS
DECLARE @Lesson1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Lesson2 UNIQUEIDENTIFIER = NEWID();
INSERT INTO Lessons (Id, ModuleId, Title, OrderIndex) VALUES 
(@Lesson1, @Module1, N'Bài 1: Giới thiệu SDLC', 1),
(@Lesson2, @Module1, N'Bài 2: Các mô hình phát triển', 2);

INSERT INTO LessonContents (LessonId, ContentType, MarkdownText, YoutubeUrl) VALUES 
(@Lesson1, 'Markdown', N'### SDLC là gì?
SDLC (Software Development Life Cycle) là vòng đời phát triển phần mềm...', NULL),
(@Lesson2, 'YoutubeVideo', NULL, 'https://www.youtube.com/watch?v=dQw4w9WgXcQ');

-- 5. FLASHCARDS
DECLARE @FcGroup1 UNIQUEIDENTIFIER = NEWID();
INSERT INTO FlashcardGroups (Id, ModuleId, Name) VALUES 
(@FcGroup1, @Module1, N'Thuật ngữ SDLC');

INSERT INTO Flashcards (GroupId, ModuleId, FrontText, BackText, OrderIndex) VALUES 
(@FcGroup1, @Module1, N'SDLC', N'Software Development Life Cycle - Vòng đời PT phần mềm', 1),
(@FcGroup1, @Module1, N'Waterfall', N'Mô hình thác nước, tuần tự từng bước', 2),
(@FcGroup1, @Module1, N'Agile', N'Mô hình linh hoạt, lặp đi lặp lại', 3);

-- 6. QUESTIONS & QUIZZES
DECLARE @Q1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Q2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Questions (Id, ModuleId, Content, Difficulty, CreatedById) VALUES 
(@Q1, @Module1, N'SDLC viết tắt của từ gì?', 'Easy', @SmeId),
(@Q2, @Module1, N'Mô hình nào linh hoạt nhất?', 'Medium', @SmeId);

INSERT INTO QuestionOptions (QuestionId, Content, IsCorrect, OrderIndex) VALUES 
(@Q1, N'Software Design Life Cycle', 0, 1),
(@Q1, N'Software Development Life Cycle', 1, 2),
(@Q2, N'Waterfall', 0, 1),
(@Q2, N'Agile', 1, 2);

-- 7. ASSIGNMENTS & RUBRICS
DECLARE @Assign1 UNIQUEIDENTIFIER = NEWID();
INSERT INTO Assignments (Id, ModuleId, Title, PromptMarkdown, Status) VALUES 
(@Assign1, @Module1, N'Viết đặc tả SDLC', N'Hãy viết 1 trang giải thích SDLC cho người mới.', 'Published');

DECLARE @Rubric1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Rubric2 UNIQUEIDENTIFIER = NEWID();
INSERT INTO RubricCriteria (Id, AssignmentId, Name, Weight, MaxScore) VALUES 
(@Rubric1, @Assign1, N'Nội dung chính xác', 60.0, 6.0),
(@Rubric2, @Assign1, N'Trình bày rõ ràng', 40.0, 4.0);

-- SUBMISSION CỦA STUDENT 1
DECLARE @Sub1 UNIQUEIDENTIFIER = NEWID();
INSERT INTO Submissions (Id, AssignmentId, StudentId, ContentText, Status, FinalScore) VALUES 
(@Sub1, @Assign1, @Student1Id, N'SDLC là vòng đời phát triển phần mềm gồm các bước: Lấy yêu cầu, Thiết kế, Code, Test, Deploy.', 'Confirmed', 8.5);

INSERT INTO SubmissionCriterionScores (SubmissionId, CriterionId, FinalScore) VALUES 
(@Sub1, @Rubric1, 5.0), (@Sub1, @Rubric2, 3.5);

/* =====================================================================
   EXPANDED DEMO DATA
   Covers each role and every functional area.  It intentionally contains
   active, draft, pending, completed and expired examples for demonstrations.
   ===================================================================== */
DECLARE @CourseGroup2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Course3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Module3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Module4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Module5 UNIQUEIDENTIFIER = NEWID();
DECLARE @Module6 UNIQUEIDENTIFIER = NEWID();
DECLARE @Lesson3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Lesson4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Lesson5 UNIQUEIDENTIFIER = NEWID();
DECLARE @Assignment2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Class1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Class2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Package1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Quiz1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Attempt1 UNIQUEIDENTIFIER = NEWID();

INSERT INTO CourseGroups (Id, Name, Description) VALUES
(@CourseGroup2, N'Digital Skills', N'Practical data and software development courses.');
INSERT INTO Courses (Id, Title, Description, OwnerSmeId, Status, IsVisible, PublishedAt) VALUES
(@Course3, N'Data Analysis with SQL', N'Query, aggregate and present data for business decisions.', @SmeId, 'Draft', 1, NULL);
INSERT INTO CourseGroupCourses (CourseGroupId, CourseId) VALUES
(@CourseGroup2, @Course3), (@CourseGroup2, @Course2);
INSERT INTO CourseGroupManagers (CourseGroupId, UserId, AssignedById) VALUES
(@CourseGroup2, @CourseManagerId, @AdminId);

INSERT INTO Modules (Id, CourseId, Title, OrderIndex) VALUES
(@Module3, @Course1, N'Chapter 3: Requirements and Quality', 3),
(@Module4, @Course2, N'Chapter 1: C# Fundamentals', 1),
(@Module5, @Course2, N'Chapter 2: Object Oriented Programming', 2),
(@Module6, @Course3, N'Chapter 1: SQL Queries', 1);
INSERT INTO Lessons (Id, ModuleId, Title, OrderIndex) VALUES
(@Lesson3, @Module2, N'Lesson 3: Scrum roles and events', 1),
(@Lesson4, @Module4, N'Lesson 1: Variables and control flow', 1),
(@Lesson5, @Module5, N'Lesson 2: Classes and objects', 1);
INSERT INTO LessonContents (LessonId, ContentType, MarkdownText, OrderIndex) VALUES
(@Lesson3, 'Markdown', N'# Scrum overview\n\nA sprint creates an increment of value. Product Owner, Scrum Master and Developers collaborate daily.', 1),
(@Lesson4, 'Markdown', N'# C# basics\n\nVariables store typed data. Conditional statements direct program flow.', 1),
(@Lesson5, 'Markdown', N'# Object oriented programming\n\nA class defines data and behaviour; an object is an instance of that class.', 1);
INSERT INTO LessonContents (LessonId, ContentType, YoutubeUrl, VideoSummary, OrderIndex) VALUES
(@Lesson3, 'YoutubeVideo', 'https://www.youtube.com/watch?v=dQw4w9WgXcQ', N'Demo summary: Scrum uses short iterations and continuous feedback.', 2);

/* Question bank: enough records to demonstrate search, filtering, quiz and AI staging. */
DECLARE @Q3 UNIQUEIDENTIFIER = NEWID(); DECLARE @Q4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Q5 UNIQUEIDENTIFIER = NEWID(); DECLARE @Q6 UNIQUEIDENTIFIER = NEWID();
DECLARE @Q7 UNIQUEIDENTIFIER = NEWID(); DECLARE @Q8 UNIQUEIDENTIFIER = NEWID();
INSERT INTO Questions (Id, ModuleId, Content, Explanation, Difficulty, Status, CreatedById) VALUES
(@Q3, @Module2, N'Who owns the product backlog?', N'The Product Owner is accountable for backlog ordering.', 'Easy', 'Active', @SmeId),
(@Q4, @Module2, N'What is the purpose of a sprint review?', N'Inspect the increment and adapt the product backlog.', 'Medium', 'Active', @SmeId),
(@Q5, @Module4, N'Which type stores a whole number in C#?', N'int stores a 32-bit signed integer.', 'Easy', 'Active', @SmeId),
(@Q6, @Module4, N'Which statement repeats while a condition is true?', N'while repeatedly executes while its condition remains true.', 'Easy', 'Active', @SmeId),
(@Q7, @Module5, N'What is encapsulation?', N'It bundles state and behavior and controls access to state.', 'Medium', 'Active', @SmeId),
(@Q8, @Module6, N'Which SQL clause filters rows before grouping?', N'WHERE filters individual rows before GROUP BY.', 'Medium', 'Archived', @SmeId);
INSERT INTO QuestionOptions (QuestionId, Content, IsCorrect, OrderIndex) VALUES
(@Q3,N'Scrum Master',0,1),(@Q3,N'Product Owner',1,2),(@Q3,N'Developers',0,3),(@Q3,N'Course Manager',0,4),
(@Q4,N'Plan payroll',0,1),(@Q4,N'Inspect the increment',1,2),(@Q4,N'Write source code alone',0,3),(@Q4,N'Close the course',0,4),
(@Q5,N'bool',0,1),(@Q5,N'int',1,2),(@Q5,N'string',0,3),(@Q5,N'decimal only',0,4),
(@Q6,N'if',0,1),(@Q6,N'switch',0,2),(@Q6,N'while',1,3),(@Q6,N'return',0,4),
(@Q7,N'Duplicating code',0,1),(@Q7,N'Hiding internal state behind an interface',1,2),(@Q7,N'A database index',0,3),(@Q7,N'A test report',0,4),
(@Q8,N'HAVING',0,1),(@Q8,N'ORDER BY',0,2),(@Q8,N'WHERE',1,3),(@Q8,N'SELECT',0,4);
INSERT INTO AiQuestionDrafts (Id, ModuleId, CreatedById, SourceText, GeneratedJson, Status) VALUES
(NEWID(), @Module2, @SmeId, N'Scrum roles and events', N'[{"content":"What is a sprint?","explanation":"A time-boxed iteration","difficulty":"Easy","options":[{"content":"A time-boxed iteration","isCorrect":true},{"content":"A database","isCorrect":false}]}]', 'Pending'),
(NEWID(), @Module4, @SmeId, N'C# variable types', N'[{"content":"What does var infer?","explanation":"The compile-time type","difficulty":"Medium","options":[{"content":"The compile-time type","isCorrect":true},{"content":"A random type","isCorrect":false}]}]', 'Approved');

DECLARE @FcGroup2 UNIQUEIDENTIFIER = NEWID(); DECLARE @FcGroup3 UNIQUEIDENTIFIER = NEWID();
INSERT INTO FlashcardGroups (Id, ModuleId, Name, OrderIndex) VALUES
(@FcGroup2, @Module2, N'Scrum essentials', 1), (@FcGroup3, @Module4, N'C# vocabulary', 1);
INSERT INTO Flashcards (Id, GroupId, ModuleId, FrontText, BackText, OrderIndex) VALUES
(NEWID(),@FcGroup2,@Module2,N'Sprint',N'A fixed-length event to create a usable increment.',1),
(NEWID(),@FcGroup2,@Module2,N'Daily Scrum',N'A short daily planning event for Developers.',2),
(NEWID(),@FcGroup3,@Module4,N'Variable',N'A named storage location with a type and value.',1),
(NEWID(),@FcGroup3,@Module4,N'Boolean',N'A value that is either true or false.',2),
(NEWID(),NULL,@Module4,N'Loop',N'A construct that repeats an action.',3);
INSERT INTO AiFlashcardDrafts (Id, ModuleId, CreatedById, SourceText, GeneratedJson, Status) VALUES
(NEWID(),@Module2,@SmeId,N'Scrum content',N'[{"frontText":"Product Backlog","backText":"Ordered list of work for the product."}]','Pending'),
(NEWID(),@Module4,@SmeId,N'C# basics',N'[{"frontText":"int","backText":"32-bit signed integer."}]','Approved');

INSERT INTO Assignments (Id, ModuleId, Title, PromptMarkdown, DueDate, MaxChars, Status, CreatedById) VALUES
(@Assignment2,@Module2,N'Scrum retrospective note',N'Write a short retrospective with one improvement for the next sprint.',DATEADD(day,7,SYSUTCDATETIME()),5000,'Published',@SmeId);
DECLARE @Rubric3 UNIQUEIDENTIFIER = NEWID(); DECLARE @Rubric4 UNIQUEIDENTIFIER = NEWID();
INSERT INTO RubricCriteria (Id, AssignmentId, Name, Weight, MaxScore, OrderIndex) VALUES
(@Rubric3,@Assignment2,N'Analysis quality',70,7,1),(@Rubric4,@Assignment2,N'Actionable improvement',30,3,2);
DECLARE @Sub2 UNIQUEIDENTIFIER = NEWID();
INSERT INTO Submissions (Id, AssignmentId, StudentId, ContentText, Status, AiTotalScore, FinalScore, TeacherComment, ConfirmedById, ConfirmedAt) VALUES
(@Sub2,@Assignment2,@Student2Id,N'The team should improve sprint planning by refining stories before the sprint starts.','AiGraded',7.5,NULL,NULL,NULL,NULL);
INSERT INTO SubmissionCriterionScores (SubmissionId, CriterionId, AiScore, Comment) VALUES
(@Sub2,@Rubric3,5.0,N'Clear analysis.'),(@Sub2,@Rubric4,2.5,N'Concrete next action.');

INSERT INTO Classes (Id, CourseId, Name, TeacherId, StartDate, EndDate, MaxStudents, Fee, Status) VALUES
(@Class1,@Course1,N'SE Fundamentals - Summer 2026',@TeacherId,CAST(DATEADD(day,-21,GETUTCDATE()) AS date),CAST(DATEADD(day,70,GETUTCDATE()) AS date),40,1500000,'Open'),
(@Class2,@Course2,N'C# Starter - Summer 2026',@TeacherId,CAST(DATEADD(day,-7,GETUTCDATE()) AS date),CAST(DATEADD(day,84,GETUTCDATE()) AS date),35,1200000,'Open');
INSERT INTO ClassStudents (ClassId, StudentId, Status) VALUES
(@Class1,@Student1Id,'Active'),(@Class1,@Student2Id,'Active'),(@Class2,@Student3Id,'Active');
INSERT INTO ClassMaterials (ClassId, Title, ContentType, MarkdownText) VALUES
(@Class1,N'Week 1 discussion guide','Markdown',N'Bring one real example of a software project to discuss.'),
(@Class2,N'C# practice repository','File',N'Use this material for the weekly exercises.');

INSERT INTO CoursePrices (CourseId, Price) VALUES (@Course1,990000),(@Course2,790000),(@Course3,1290000);
INSERT INTO Packages (Id, CourseGroupId, Name, DurationMonths, Price) VALUES
(@Package1,@CourseGroup1,N'Programming foundation - 6 months',6,1490000),
(NEWID(),@CourseGroup2,N'Digital skills - 3 months',3,990000);
INSERT INTO PackageSubscriptions (PackageId, StudentId, StartAt, ExpiresAt, Status) VALUES
(@Package1,@Student1Id,DATEADD(day,-30,SYSUTCDATETIME()),DATEADD(month,5,SYSUTCDATETIME()),'Active'),
(@Package1,@Student2Id,DATEADD(month,-8,SYSUTCDATETIME()),DATEADD(month,-2,SYSUTCDATETIME()),'Expired');
INSERT INTO AccessGrants (StudentId, CourseId, GrantType, SourceRefId, ExpiresAt, IsRevoked) VALUES
(@Student1Id,@Course1,'H3',@Package1,DATEADD(month,5,SYSUTCDATETIME()),0),
(@Student1Id,@Course2,'H3',@Package1,DATEADD(month,5,SYSUTCDATETIME()),0),
(@Student2Id,@Course1,'H2',@Class1,DATEADD(day,70,SYSUTCDATETIME()),0),
(@Student3Id,@Course2,'H2',@Class2,DATEADD(day,84,SYSUTCDATETIME()),0),
(@Student2Id,@Course2,'H1',NEWID(),NULL,1);
INSERT INTO Payments (StudentId, Amount, Provider, PurchaseType, RefId, ProviderTxnId, Status, PaidAt) VALUES
(@Student1Id,1490000,'VNPay','H3',@Package1,'DEMO-VNP-0001','Success',DATEADD(day,-30,SYSUTCDATETIME())),
(@Student2Id,1500000,'SePay','H2',@Class1,'DEMO-SP-0002','Success',DATEADD(day,-20,SYSUTCDATETIME())),
(@Student3Id,1200000,'VNPay','H2',@Class2,'DEMO-VNP-0003','Pending',NULL);

INSERT INTO LessonProgress (LessonId, StudentId, IsCompleted, CompletedAt) VALUES
(@Lesson1,@Student1Id,1,DATEADD(day,-18,SYSUTCDATETIME())),(@Lesson2,@Student1Id,1,DATEADD(day,-17,SYSUTCDATETIME())),
(@Lesson3,@Student1Id,0,NULL),(@Lesson1,@Student2Id,1,DATEADD(day,-12,SYSUTCDATETIME())),(@Lesson4,@Student3Id,0,NULL);
INSERT INTO FlashcardProgress (FlashcardId, StudentId, IsLearned, LastReviewedAt)
SELECT TOP 5 Id,@Student1Id,CASE WHEN OrderIndex % 2 = 0 THEN 1 ELSE 0 END,DATEADD(day,-OrderIndex,SYSUTCDATETIME()) FROM Flashcards ORDER BY CreatedAt;

INSERT INTO Quizzes (Id, StudentId, CourseId, Title, QuestionCount, Difficulty) VALUES
(@Quiz1,@Student1Id,@Course1,N'SDLC practice quiz',4,'Mixed');
INSERT INTO QuizQuestions (QuizId, QuestionId, OrderIndex) VALUES
(@Quiz1,@Q1,1),(@Quiz1,@Q2,2),(@Quiz1,@Q3,3),(@Quiz1,@Q4,4);
INSERT INTO QuizAttempts (Id, QuizId, StudentId, StartedAt, SubmittedAt, Score) VALUES
(@Attempt1,@Quiz1,@Student1Id,DATEADD(day,-5,SYSUTCDATETIME()),DATEADD(day,-5,SYSUTCDATETIME()),75);
INSERT INTO QuizAttemptAnswers (AttemptId, QuestionId, SelectedOptionId, IsCorrect)
SELECT @Attempt1,q.Id,(SELECT TOP 1 o.Id FROM QuestionOptions o WHERE o.QuestionId=q.Id AND o.IsCorrect=1),1
FROM Questions q WHERE q.Id IN (@Q1,@Q2,@Q3);

INSERT INTO AiLessonDrafts (Id, LessonId, CreatedById, SourceText, GeneratedText, Status) VALUES
(NEWID(),@Lesson3,@SmeId,N'Outline: Scrum inspection and adaptation.',N'# Scrum inspection\n\nDraft content awaiting SME approval.','Pending'),
(NEWID(),@Lesson4,@SmeId,N'Outline: C# conditions.',N'# C# conditions\n\nApproved demo lesson content.','Approved');
INSERT INTO AiUsageLogs (UserId, TaskType, TokensUsed, DurationMs) VALUES
(@SmeId,'GenLesson',1200,860),(@SmeId,'GenQuestion',850,720),(@SmeId,'GenFlashcard',630,510),(@TeacherId,'GradeEssay',980,940);
INSERT INTO Notifications (UserId, Type, Title, Message, Channel, IsRead) VALUES
(@Student1Id,'Assignment','Assignment feedback available',N'Your SDLC assignment has been confirmed by the teacher.','InApp',0),
(@Student2Id,'Assignment','AI pre-grade ready',N'The teacher can now review the preliminary grading result.','InApp',1),
(@SmeId,'AI','AI draft waiting for review',N'A Scrum lesson draft is pending approval.','InApp',0),
(@TeacherId,'Class','New student enrollment',N'A student enrolled in SE Fundamentals - Summer 2026.','InApp',0);
INSERT INTO ActivityLogs (UserId, Action, EntityType, EntityId, Result, Detail) VALUES
(@AdminId,'SeedDemo','Database',NULL,'Success',N'Full role-based demo dataset created.'),
(@SmeId,'CreateLesson','Lesson',CONVERT(NVARCHAR(64),@Lesson3),'Success',N'Created Scrum lesson.'),
(@TeacherId,'ConfirmGrade','Submission',CONVERT(NVARCHAR(64),@Sub1),'Success',N'Confirmed final score 8.5.');
INSERT INTO SystemConfigs (ConfigKey, ConfigValue, Description, UpdatedById) VALUES
(N'CFG-B01',N'10485760',N'Maximum attached-file size in bytes (10 MB).',@AdminId),
(N'CFG-B02',N'10',N'Maximum AI items generated in one request.',@AdminId),
(N'CFG-B09',N'365',N'Activity-log retention period in days.',@AdminId);

PRINT N'>>> FULL DEMO DATA INSERTS COMPLETED SUCCESSFULLY.';
PRINT N'>>> Demo accounts: admin@edunexus.vn/admin123 | sme.nguyen@edunexus.vn/sme123 | teacher.tran@edunexus.vn/teacher123 | student.le@edunexus.vn/student123';
