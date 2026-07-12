/* =====================================================================
   EduNexus — Demo Data Full (Tất cả Modules)
   Nhóm: G5  |  Deliverable: GP6
   ---------------------------------------------------------------------
   File này INSERT dữ liệu mẫu CHUẨN, KHÔNG CONFLICT cho toàn bộ dự án.
   Bao gồm: Users, Course, Lesson, Question, Flashcard, Quiz, Assignment
   Chạy sau file EduNexus_CreateDatabase.sql
   ===================================================================== */

USE EduNexus;
GO

/* ---------- 1. Users — Admin, SME, Teacher, 3 Students ---------- */

INSERT INTO Users (Id, Email, PasswordHash, DisplayName, AuthProvider, IsEmailVerified, IsActive, CreatedAt)
VALUES 
('A0000000-0000-0000-0000-000000000000', N'admin@edunexus.vn', N'$2a$12$hashedPassword', N'Admin System', N'Local', 1, 1, SYSUTCDATETIME()),
('A1111111-1111-1111-1111-111111111111', N'sme.nguyen@edunexus.vn', N'$2a$12$hashedPassword', N'Nguyễn Văn An (SME)', N'Local', 1, 1, SYSUTCDATETIME()),
('B2222222-2222-2222-2222-222222222222', N'teacher.tran@edunexus.vn', N'$2a$12$hashedPassword', N'Trần Minh Tuấn (GV)', N'Local', 1, 1, SYSUTCDATETIME()),
('C3333333-3333-3333-3333-333333333333', N'student.le@edunexus.vn', N'$2a$12$hashedPassword', N'Lê Thị Bình (HV)', N'Local', 1, 1, SYSUTCDATETIME()),
('D4444444-4444-4444-4444-444444444444', N'student.pham@edunexus.vn', N'$2a$12$hashedPassword', N'Phạm Hoàng Dũng (HV)', N'Local', 1, 1, SYSUTCDATETIME()),
('E5555555-5555-5555-5555-555555555555', N'student.hoang@edunexus.vn', N'$2a$12$hashedPassword', N'Hoàng Quốc Bảo (HV)', N'Local', 1, 1, SYSUTCDATETIME());

INSERT INTO UserRoles (UserId, RoleId) VALUES 
('A0000000-0000-0000-0000-000000000000', 1), -- Admin
('A1111111-1111-1111-1111-111111111111', 2), -- SME
('B2222222-2222-2222-2222-222222222222', 3), -- Teacher
('C3333333-3333-3333-3333-333333333333', 5), -- Student
('D4444444-4444-4444-4444-444444444444', 5), -- Student
('E5555555-5555-5555-5555-555555555555', 5); -- Student
GO

/* ---------- 2. Course & Modules ---------- */

INSERT INTO Courses (Id, Title, Description, OwnerSmeId, Status, CreatedAt)
VALUES ('C0000000-0000-0000-0000-000000000000', N'Nhập môn Kỹ thuật Phần mềm (SE)', N'Khóa học nền tảng cho lập trình viên.', 'A1111111-1111-1111-1111-111111111111', N'Published', SYSUTCDATETIME());

INSERT INTO Modules (Id, CourseId, Title, OrderIndex, CreatedAt) VALUES
('M1000000-0000-0000-0000-000000000001', 'C0000000-0000-0000-0000-000000000000', N'Module 1: Tổng quan quy trình', 1, SYSUTCDATETIME()),
('M2000000-0000-0000-0000-000000000002', 'C0000000-0000-0000-0000-000000000000', N'Module 2: Phân tích yêu cầu', 2, SYSUTCDATETIME()),
('M3000000-0000-0000-0000-000000000003', 'C0000000-0000-0000-0000-000000000000', N'Module 3: Thiết kế hệ thống', 3, SYSUTCDATETIME());
GO

/* ---------- 3. Lessons (FT-01, FT-02) ---------- */

INSERT INTO Lessons (Id, ModuleId, Title, OrderIndex) VALUES
('L1000000-0000-0000-0000-000000000001', 'M1000000-0000-0000-0000-000000000001', N'Bài 1: SDLC là gì?', 1),
('L2000000-0000-0000-0000-000000000002', 'M1000000-0000-0000-0000-000000000001', N'Bài 2: Agile vs Waterfall', 2),
('L3000000-0000-0000-0000-000000000003', 'M2000000-0000-0000-0000-000000000002', N'Bài 3: Thu thập yêu cầu (Elicitation)', 1);

INSERT INTO LessonContents (Id, LessonId, ContentType, MarkdownText, YoutubeUrl, OrderIndex) VALUES
('LC100000-0000-0000-0000-000000000001', 'L1000000-0000-0000-0000-000000000001', N'YoutubeVideo', NULL, N'https://youtube.com/watch?v=example1', 1),
('LC200000-0000-0000-0000-000000000002', 'L1000000-0000-0000-0000-000000000001', N'Markdown', N'## SDLC
Software Development Life Cycle bao gồm 6 bước cơ bản...', NULL, 2),
('LC300000-0000-0000-0000-000000000003', 'L2000000-0000-0000-0000-000000000002', N'YoutubeVideo', NULL, N'https://youtube.com/watch?v=example2', 1);
GO

/* ---------- 4. Flashcards (FT-04, FT-06) ---------- */

INSERT INTO FlashcardGroups (Id, ModuleId, Name, OrderIndex) VALUES
('F1000000-0000-0000-0000-000000000001', 'M1000000-0000-0000-0000-000000000001', N'Thuật ngữ cơ bản KTPM', 1);

INSERT INTO Flashcards (Id, GroupId, ModuleId, FrontText, BackText, OrderIndex) VALUES
('FC100000-0000-0000-0000-000000000001', 'F1000000-0000-0000-0000-000000000001', 'M1000000-0000-0000-0000-000000000001', N'SDLC', N'Software Development Life Cycle - Vòng đời phát triển phần mềm', 1),
('FC200000-0000-0000-0000-000000000002', 'F1000000-0000-0000-0000-000000000001', 'M1000000-0000-0000-0000-000000000001', N'Agile', N'Phương pháp luận phát triển phần mềm linh hoạt, lặp đi lặp lại', 2),
('FC300000-0000-0000-0000-000000000003', 'F1000000-0000-0000-0000-000000000001', 'M1000000-0000-0000-0000-000000000001', N'Stakeholder', N'Các bên liên quan, những người có lợi ích trong dự án', 3);
GO

/* ---------- 5. Questions Bank (FT-03) ---------- */

INSERT INTO Questions (Id, ModuleId, Content, Explanation, Difficulty) VALUES
('Q1000000-0000-0000-0000-000000000001', 'M1000000-0000-0000-0000-000000000001', N'Đâu là giai đoạn đầu tiên của SDLC?', N'Giai đoạn đầu tiên luôn là xác định và phân tích yêu cầu.', N'Easy'),
('Q2000000-0000-0000-0000-000000000002', 'M1000000-0000-0000-0000-000000000001', N'Mô hình nào KHÔNG linh hoạt với sự thay đổi yêu cầu?', N'Waterfall đi theo trình tự tuyến tính, rất khó quay lại thay đổi.', N'Medium');

INSERT INTO QuestionOptions (QuestionId, Content, IsCorrect, OrderIndex) VALUES
('Q1000000-0000-0000-0000-000000000001', N'Thiết kế (Design)', 0, 1),
('Q1000000-0000-0000-0000-000000000001', N'Thu thập yêu cầu (Requirement Analysis)', 1, 2),
('Q1000000-0000-0000-0000-000000000001', N'Lập trình (Coding)', 0, 3),
('Q1000000-0000-0000-0000-000000000001', N'Kiểm thử (Testing)', 0, 4),
('Q2000000-0000-0000-0000-000000000002', N'Agile', 0, 1),
('Q2000000-0000-0000-0000-000000000002', N'Scrum', 0, 2),
('Q2000000-0000-0000-0000-000000000002', N'Waterfall', 1, 3);
GO

/* ---------- 6. Quiz (FT-07) ---------- */

INSERT INTO Quizzes (Id, StudentId, CourseId, Title, QuestionCount, Difficulty) VALUES
('QZ100000-0000-0000-0000-000000000001', 'C3333333-3333-3333-3333-333333333333', 'C0000000-0000-0000-0000-000000000000', N'Quiz: Ôn tập Module 1', 2, N'Mixed');

INSERT INTO QuizQuestions (QuizId, QuestionId, OrderIndex) VALUES
('QZ100000-0000-0000-0000-000000000001', 'Q1000000-0000-0000-0000-000000000001', 1),
('QZ100000-0000-0000-0000-000000000001', 'Q2000000-0000-0000-0000-000000000002', 2);
GO

/* ---------- 7. Assignments (FT-05, 08) ---------- */

INSERT INTO Assignments (Id, ModuleId, Title, PromptMarkdown, DueDate, Status) VALUES 
('AA000001-0001-0001-0001-000000000002', 'M2000000-0000-0000-0000-000000000002', N'Bài tập: Đặc tả Use Case', N'Viết đặc tả Use Case Mượn Sách...', DATEADD(DAY, 7, SYSUTCDATETIME()), N'Published');

INSERT INTO RubricCriteria (Id, AssignmentId, Name, Weight, MaxScore, OrderIndex) VALUES
('CC000002-0001-0001-0001-000000000001', 'AA000001-0001-0001-0001-000000000002', N'Đầy đủ luồng Main Flow', 50, 10, 1),
('CC000002-0001-0001-0001-000000000002', 'AA000001-0001-0001-0001-000000000002', N'Luồng Alternative hợp lý', 50, 10, 2);

INSERT INTO Submissions (Id, AssignmentId, StudentId, ContentText, Status, AiTotalScore) VALUES 
('DD000001-0001-0001-0001-000000000001', 'AA000001-0001-0001-0001-000000000002', 'C3333333-3333-3333-3333-333333333333', N'Đây là bài làm Use Case của tôi...', N'AiGraded', 8.5);

INSERT INTO SubmissionCriterionScores (SubmissionId, CriterionId, AiScore, Comment) VALUES
('DD000001-0001-0001-0001-000000000001', 'CC000002-0001-0001-0001-000000000001', 9.0, N'Tốt'),
('DD000001-0001-0001-0001-000000000001', 'CC000002-0001-0001-0001-000000000002', 8.0, N'Khá');
GO

PRINT N'>>> EduNexus FULL demo data inserted successfully.';
GO
