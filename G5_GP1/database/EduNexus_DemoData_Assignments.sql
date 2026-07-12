/* =====================================================================
   EduNexus — Demo Data Full (Tất cả Modules: Lớn, Đầy đủ, Hoàn thiện)
   Nhóm: G5  |  Deliverable: GP6
   ---------------------------------------------------------------------
   Chạy script này SAU KHI chạy EduNexus_CreateDatabase.sql
   ===================================================================== */

USE EduNexus;
GO

PRINT N'>>> Đang xóa dữ liệu cũ (nếu có)...';
DELETE FROM SubmissionCriterionScores;
DELETE FROM Submissions;
DELETE FROM RubricCriteria;
DELETE FROM Assignments;
DELETE FROM QuizQuestions;
DELETE FROM Quizzes;
DELETE FROM QuestionOptions;
DELETE FROM Questions;
DELETE FROM Flashcards;
DELETE FROM FlashcardGroups;
DELETE FROM LessonProgress;
DELETE FROM LessonContents;
DELETE FROM Lessons;
DELETE FROM Modules;
DELETE FROM Courses;
DELETE FROM UserRoles;
DELETE FROM Users;
DELETE FROM Roles;
GO

PRINT N'>>> 1. Đang tạo Roles...';
SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (Id, Name, Description) VALUES
(1, N'Admin', N'Quản trị viên hệ thống'),
(2, N'SME', N'Chuyên gia nội dung (Tạo khóa học, duyệt AI)'),
(3, N'Teacher', N'Giảng viên (Chấm bài, theo dõi tiến độ)'),
(4, N'CourseManager', N'Quản lý Khóa học'),
(5, N'Student', N'Học viên');
SET IDENTITY_INSERT Roles OFF;
GO

PRINT N'>>> 2. Đang tạo Users...';
INSERT INTO Users (Id, Email, PasswordHash, DisplayName, AuthProvider, IsEmailVerified, IsActive, CreatedAt) VALUES 
('A0000000-0000-0000-0000-000000000000', N'admin@edunexus.vn', N'$2a$12$mockhash_for_demo_only', N'Admin System', N'Local', 1, 1, SYSUTCDATETIME()),
('A1111111-1111-1111-1111-111111111111', N'sme.nguyen@edunexus.vn', N'$2a$12$mockhash_for_demo_only', N'Nguyễn Văn An (SME)', N'Local', 1, 1, SYSUTCDATETIME()),
('B2222222-2222-2222-2222-222222222222', N'teacher.tran@edunexus.vn', N'$2a$12$mockhash_for_demo_only', N'Trần Minh Tuấn (GV)', N'Local', 1, 1, SYSUTCDATETIME()),
('C3333333-3333-3333-3333-333333333333', N'student.le@edunexus.vn', N'$2a$12$mockhash_for_demo_only', N'Lê Thị Bình (HV)', N'Local', 1, 1, SYSUTCDATETIME()),
('D4444444-4444-4444-4444-444444444444', N'student.pham@edunexus.vn', N'$2a$12$mockhash_for_demo_only', N'Phạm Hoàng Dũng (HV)', N'Local', 1, 1, SYSUTCDATETIME()),
('E5555555-5555-5555-5555-555555555555', N'student.hoang@edunexus.vn', N'$2a$12$mockhash_for_demo_only', N'Hoàng Quốc Bảo (HV)', N'Local', 1, 1, SYSUTCDATETIME()),
('F6666666-6666-6666-6666-666666666666', N'teacher.pending@edunexus.vn', N'$2a$12$mockhash_for_demo_only', N'Ngô Chờ Duyệt (GV Pending)', N'Local', 1, 0, SYSUTCDATETIME());

INSERT INTO UserRoles (UserId, RoleId) VALUES 
('A0000000-0000-0000-0000-000000000000', 1), -- Admin
('A1111111-1111-1111-1111-111111111111', 2), -- SME
('B2222222-2222-2222-2222-222222222222', 3), -- Teacher
('C3333333-3333-3333-3333-333333333333', 5), -- Student
('D4444444-4444-4444-4444-444444444444', 5), -- Student
('E5555555-5555-5555-5555-555555555555', 5), -- Student
('F6666666-6666-6666-6666-666666666666', 3); -- Teacher (Inactive = Pending Approval)
GO

PRINT N'>>> 3. Đang tạo Courses & Modules...';
INSERT INTO Courses (Id, Title, Description, OwnerSmeId, Status, CreatedAt) VALUES 
('C0000000-0000-0000-0000-000000000001', N'Nhập môn Kỹ thuật Phần mềm (SE)', N'Khóa học nền tảng cho lập trình viên (SDLC, Agile, Requirement).', 'A1111111-1111-1111-1111-111111111111', N'Published', SYSUTCDATETIME()),
('C0000000-0000-0000-0000-000000000002', N'Lập trình C# Cơ bản', N'Ngôn ngữ C# và .NET Framework.', 'A1111111-1111-1111-1111-111111111111', N'Published', SYSUTCDATETIME());

INSERT INTO Modules (Id, CourseId, Title, OrderIndex, CreatedAt) VALUES
('11000000-0000-0000-0000-000000000001', 'C0000000-0000-0000-0000-000000000001', N'Module 1: SDLC và Quy trình', 1, SYSUTCDATETIME()),
('11000000-0000-0000-0000-000000000002', 'C0000000-0000-0000-0000-000000000001', N'Module 2: Phân tích Yêu cầu', 2, SYSUTCDATETIME()),
('11000000-0000-0000-0000-000000000003', 'C0000000-0000-0000-0000-000000000002', N'Module 1: Biến & Kiểu dữ liệu', 1, SYSUTCDATETIME());
GO

PRINT N'>>> 4. Đang tạo Lessons...';
INSERT INTO Lessons (Id, ModuleId, Title, OrderIndex) VALUES
('22000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', N'Bài 1: SDLC là gì?', 1),
('22000000-0000-0000-0000-000000000002', '11000000-0000-0000-0000-000000000001', N'Bài 2: Các mô hình Agile vs Waterfall', 2),
('22000000-0000-0000-0000-000000000003', '11000000-0000-0000-0000-000000000002', N'Bài 3: Kỹ thuật Elicitation', 1);

INSERT INTO LessonContents (Id, LessonId, ContentType, MarkdownText, YoutubeUrl, OrderIndex) VALUES
('33000000-0000-0000-0000-000000000001', '22000000-0000-0000-0000-000000000001', N'YoutubeVideo', NULL, N'https://www.youtube.com/watch?v=R9R9t8w6hD4', 1),
('33000000-0000-0000-0000-000000000002', '22000000-0000-0000-0000-000000000001', N'Markdown', N'## Khái niệm cơ bản
Vòng đời phát triển phần mềm (SDLC) là quá trình thiết kế, phát triển và thử nghiệm các phần mềm chất lượng cao.
Mục tiêu là tạo ra phần mềm đáp ứng hoặc vượt quá sự mong đợi của khách hàng.', NULL, 2),
('33000000-0000-0000-0000-000000000003', '22000000-0000-0000-0000-000000000002', N'YoutubeVideo', NULL, N'https://www.youtube.com/watch?v=Z9wN-0x4U6M', 1);
GO

PRINT N'>>> 5. Đang tạo Flashcards...';
INSERT INTO FlashcardGroups (Id, ModuleId, Name, OrderIndex) VALUES
('44000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', N'Thuật ngữ KTPM (Cơ bản)', 1),
('44000000-0000-0000-0000-000000000002', '11000000-0000-0000-0000-000000000002', N'Thuật ngữ Yêu cầu', 2);

INSERT INTO Flashcards (Id, GroupId, ModuleId, FrontText, BackText, OrderIndex) VALUES
('55000000-0000-0000-0000-000000000001', '44000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', N'SDLC', N'Software Development Life Cycle - Vòng đời phát triển phần mềm', 1),
('55000000-0000-0000-0000-000000000002', '44000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', N'Agile', N'Phương pháp phát triển linh hoạt, phân nhỏ dự án thành các sprint ngắn.', 2),
('55000000-0000-0000-0000-000000000003', '44000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', N'Waterfall', N'Mô hình thác nước, các giai đoạn diễn ra tuần tự từ trên xuống dưới.', 3),
('55000000-0000-0000-0000-000000000004', '44000000-0000-0000-0000-000000000002', '11000000-0000-0000-0000-000000000002', N'Stakeholder', N'Người hoặc tổ chức có lợi ích hoặc bị ảnh hưởng bởi dự án.', 1);
GO

PRINT N'>>> 6. Đang tạo Ngân hàng Câu hỏi & Quizzes...';
INSERT INTO Questions (Id, ModuleId, Content, Explanation, Difficulty) VALUES
('66000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', N'Giai đoạn đầu tiên của SDLC là gì?', N'Thu thập và phân tích yêu cầu (Requirement Analysis) luôn là bước số 1.', N'Easy'),
('66000000-0000-0000-0000-000000000002', '11000000-0000-0000-0000-000000000001', N'Đặc điểm nổi bật của Agile là gì?', N'Tính linh hoạt, thay đổi theo yêu cầu khách hàng.', N'Medium'),
('66000000-0000-0000-0000-000000000003', '11000000-0000-0000-0000-000000000001', N'Trong Waterfall, khi nào Testing được thực hiện?', N'Chỉ sau khi hoàn thành giai đoạn Coding.', N'Medium');

INSERT INTO QuestionOptions (QuestionId, Content, IsCorrect, OrderIndex) VALUES
('66000000-0000-0000-0000-000000000001', N'Thiết kế kiến trúc', 0, 1),
('66000000-0000-0000-0000-000000000001', N'Kiểm thử', 0, 2),
('66000000-0000-0000-0000-000000000001', N'Thu thập & Phân tích yêu cầu', 1, 3),
('66000000-0000-0000-0000-000000000001', N'Bảo trì', 0, 4),
('66000000-0000-0000-0000-000000000002', N'Tài liệu đồ sộ từ đầu', 0, 1),
('66000000-0000-0000-0000-000000000002', N'Linh hoạt với sự thay đổi', 1, 2),
('66000000-0000-0000-0000-000000000002', N'Làm tuần tự 1 chiều', 0, 3),
('66000000-0000-0000-0000-000000000003', N'Song song với phân tích', 0, 1),
('66000000-0000-0000-0000-000000000003', N'Sau giai đoạn Code', 1, 2);

INSERT INTO Quizzes (Id, StudentId, CourseId, Title, QuestionCount, Difficulty) VALUES
('77000000-0000-0000-0000-000000000001', 'C3333333-3333-3333-3333-333333333333', 'C0000000-0000-0000-0000-000000000001', N'Bài Kiểm tra Cuối Tuần 1', 3, N'Mixed');

INSERT INTO QuizQuestions (QuizId, QuestionId, OrderIndex) VALUES
('77000000-0000-0000-0000-000000000001', '66000000-0000-0000-0000-000000000001', 1),
('77000000-0000-0000-0000-000000000001', '66000000-0000-0000-0000-000000000002', 2),
('77000000-0000-0000-0000-000000000001', '66000000-0000-0000-0000-000000000003', 3);
GO

PRINT N'>>> 7. Đang tạo Assignments & Submissions...';
INSERT INTO Assignments (Id, ModuleId, Title, PromptMarkdown, DueDate, Status) VALUES 
('88000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000002', N'Đặc tả Use Case "Mượn Sách"', N'Viết Use Case Mượn Sách tại Thư viện. Yêu cầu:
1. Xác định Actor.
2. Main flow.
3. Alternative flow.', DATEADD(DAY, 7, SYSUTCDATETIME()), N'Published'),
('88000000-0000-0000-0000-000000000002', '11000000-0000-0000-0000-000000000001', N'So sánh Agile và Waterfall', N'Viết một đoạn văn (khoảng 300 chữ) so sánh 2 phương pháp trên.', DATEADD(DAY, 14, SYSUTCDATETIME()), N'Published');

INSERT INTO RubricCriteria (Id, AssignmentId, Name, Weight, MaxScore, OrderIndex) VALUES
('99000000-0000-0000-0000-000000000001', '88000000-0000-0000-0000-000000000001', N'Đầy đủ luồng Main Flow', 40, 10, 1),
('99000000-0000-0000-0000-000000000002', '88000000-0000-0000-0000-000000000001', N'Luồng Alternative hợp lý', 40, 10, 2),
('99000000-0000-0000-0000-000000000003', '88000000-0000-0000-0000-000000000001', N'Trình bày chuẩn format', 20, 10, 3);

INSERT INTO Submissions (Id, AssignmentId, StudentId, ContentText, Status, AiTotalScore) VALUES 
('AA000000-0000-0000-0000-000000000001', '88000000-0000-0000-0000-000000000001', 'C3333333-3333-3333-3333-333333333333', N'Actor: Sinh viên, Thủ thư. 
Main Flow: SV xuất trình thẻ -> Chọn sách -> Thủ thư quét mã -> Ghi mượn thành công.
Alternative: Thẻ hết hạn -> Từ chối mượn.', N'AiGraded', 9.0);

INSERT INTO SubmissionCriterionScores (SubmissionId, CriterionId, AiScore, Comment) VALUES
('AA000000-0000-0000-0000-000000000001', '99000000-0000-0000-0000-000000000001', 9.5, N'Luồng chính rất rõ ràng, đúng logic.'),
('AA000000-0000-0000-0000-000000000001', '99000000-0000-0000-0000-000000000002', 8.5, N'Đã bao gồm ngoại lệ Thẻ hết hạn.'),
('AA000000-0000-0000-0000-000000000001', '99000000-0000-0000-0000-000000000003', 9.0, N'Đúng chuẩn định dạng Use Case.');
GO

PRINT N'>>> HOÀN TẤT. EduNexus Demo Data (Bản Siêu Đầy Đủ) đã được nạp thành công!';
GO
