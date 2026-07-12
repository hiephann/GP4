/* =====================================================================
   EduNexus — DEMO DATA cho nhóm màn hình LESSON (FT-02, FT-06)
   Màn hình: Lesson Editor, AI Lesson Staging, Lesson Text Extract, Lesson View

   Chạy SAU EduNexus_CreateDatabase.sql:
       sqlcmd -S localhost -i database/EduNexus_DemoData.sql

   Script idempotent: chạy lại nhiều lần vẫn ra đúng một bộ dữ liệu
   (xóa theo GUID cố định rồi chèn lại).
   ===================================================================== */

USE EduNexus;
GO

SET NOCOUNT ON;
GO

/* ---------------------------------------------------------------------
   GUID cố định — để script có thể chạy lại và để dễ tra cứu khi demo
   --------------------------------------------------------------------- */
DECLARE @SmeId        UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @Student1Id   UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @Student2Id   UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';

DECLARE @CourseId     UNIQUEIDENTIFIER = 'AA000000-0000-0000-0000-000000000001';
DECLARE @Module1Id    UNIQUEIDENTIFIER = 'BB000000-0000-0000-0000-000000000001';
DECLARE @Module2Id    UNIQUEIDENTIFIER = 'BB000000-0000-0000-0000-000000000002';

DECLARE @Lesson1Id    UNIQUEIDENTIFIER = 'CC000000-0000-0000-0000-000000000001';
DECLARE @Lesson2Id    UNIQUEIDENTIFIER = 'CC000000-0000-0000-0000-000000000002';
DECLARE @Lesson3Id    UNIQUEIDENTIFIER = 'CC000000-0000-0000-0000-000000000003';
DECLARE @Lesson4Id    UNIQUEIDENTIFIER = 'CC000000-0000-0000-0000-000000000004';

DECLARE @Content1Id   UNIQUEIDENTIFIER = 'DD000000-0000-0000-0000-000000000001';
DECLARE @Content2Id   UNIQUEIDENTIFIER = 'DD000000-0000-0000-0000-000000000002';
DECLARE @Content3Id   UNIQUEIDENTIFIER = 'DD000000-0000-0000-0000-000000000003';
DECLARE @Content4Id   UNIQUEIDENTIFIER = 'DD000000-0000-0000-0000-000000000004';
DECLARE @Content5Id   UNIQUEIDENTIFIER = 'DD000000-0000-0000-0000-000000000005';

DECLARE @Draft1Id     UNIQUEIDENTIFIER = 'EE000000-0000-0000-0000-000000000001';
DECLARE @Draft2Id     UNIQUEIDENTIFIER = 'EE000000-0000-0000-0000-000000000002';

/* ---------------------------------------------------------------------
   Dọn dữ liệu demo cũ — xóa từ bảng con lên bảng cha
   --------------------------------------------------------------------- */
-- Xóa theo NGƯỜI DÙNG / BÀI HỌC demo chứ không theo GUID cố định: các bản ghi phát sinh
-- trong lúc demo (draft AI mới, tiến độ mới...) cũng phải dọn, nếu không khóa ngoại sẽ chặn DELETE Users.
DELETE FROM AiUsageLogs    WHERE UserId   IN (@SmeId, @Student1Id, @Student2Id);
DELETE FROM LessonProgress WHERE LessonId IN (@Lesson1Id, @Lesson2Id, @Lesson3Id, @Lesson4Id)
                              OR StudentId IN (@SmeId, @Student1Id, @Student2Id);
DELETE FROM AiLessonDrafts WHERE Id       IN (@Draft1Id, @Draft2Id)
                              OR CreatedById IN (@SmeId, @Student1Id, @Student2Id)
                              OR LessonId IN (@Lesson1Id, @Lesson2Id, @Lesson3Id, @Lesson4Id);
DELETE FROM LessonContents WHERE LessonId IN (@Lesson1Id, @Lesson2Id, @Lesson3Id, @Lesson4Id);
DELETE FROM Lessons        WHERE Id       IN (@Lesson1Id, @Lesson2Id, @Lesson3Id, @Lesson4Id);
DELETE FROM Modules        WHERE Id       IN (@Module1Id, @Module2Id);
DELETE FROM Courses        WHERE Id       =  @CourseId;
DELETE FROM UserRoles      WHERE UserId   IN (@SmeId, @Student1Id, @Student2Id);
DELETE FROM Users          WHERE Id       IN (@SmeId, @Student1Id, @Student2Id);

/* ---------------------------------------------------------------------
   1. Users — 1 SME + 2 Student
      PasswordHash chỉ là chuỗi giả lập; luồng đăng nhập thật thuộc FT-14 (nhóm Common).
   --------------------------------------------------------------------- */
INSERT INTO Users (Id, Email, PasswordHash, DisplayName, AuthProvider, IsEmailVerified, IsActive, AiTokenQuota, AiTokenUsed)
VALUES
  (@SmeId,      N'sme@edunexus.vn',      N'DEMO_HASH', N'Trần Minh Anh (SME)',   N'Local', 1, 1, 1000000, 0),
  (@Student1Id, N'student1@edunexus.vn', N'DEMO_HASH', N'Nguyễn Văn Hùng',       N'Local', 1, 1, NULL,    0),
  (@Student2Id, N'student2@edunexus.vn', N'DEMO_HASH', N'Lê Thị Mai',            N'Local', 1, 1, NULL,    0);

INSERT INTO UserRoles (UserId, RoleId)
SELECT @SmeId, Id FROM Roles WHERE Name = N'SME'
UNION ALL SELECT @Student1Id, Id FROM Roles WHERE Name = N'Student'
UNION ALL SELECT @Student2Id, Id FROM Roles WHERE Name = N'Student';

/* ---------------------------------------------------------------------
   2. Course + Modules
   --------------------------------------------------------------------- */
INSERT INTO Courses (Id, Title, Description, OwnerSmeId, Status, IsVisible, Version, PublishedAt)
VALUES (@CourseId, N'Lập trình C# cơ bản',
        N'Khóa học nhập môn C# và .NET dành cho người mới bắt đầu.',
        @SmeId, N'Published', 1, 1, SYSUTCDATETIME());

INSERT INTO Modules (Id, CourseId, Title, OrderIndex) VALUES
  (@Module1Id, @CourseId, N'Module 1 — Nhập môn C#',           1),
  (@Module2Id, @CourseId, N'Module 2 — Lập trình hướng đối tượng', 2);

/* ---------------------------------------------------------------------
   3. Lessons
   --------------------------------------------------------------------- */
INSERT INTO Lessons (Id, ModuleId, Title, OrderIndex) VALUES
  (@Lesson1Id, @Module1Id, N'Bài 1 — Biến và kiểu dữ liệu', 1),
  (@Lesson2Id, @Module1Id, N'Bài 2 — Câu lệnh điều kiện và vòng lặp', 2),
  (@Lesson3Id, @Module2Id, N'Bài 3 — Lớp và đối tượng', 1),
  (@Lesson4Id, @Module2Id, N'Bài 4 — Kế thừa và đa hình', 2);

/* ---------------------------------------------------------------------
   4. LessonContents — đủ 3 loại: Markdown / YoutubeVideo / File
   --------------------------------------------------------------------- */
INSERT INTO LessonContents (Id, LessonId, ContentType, MarkdownText, YoutubeUrl, VideoSummary, FileUrl, FileName, OrderIndex)
VALUES
  (@Content1Id, @Lesson1Id, N'Markdown',
   N'# Biến và kiểu dữ liệu trong C#

## Mục tiêu học tập
- Khai báo được biến với đúng kiểu dữ liệu
- Phân biệt kiểu giá trị (value type) và kiểu tham chiếu (reference type)

## 1. Khai báo biến
Trong C#, mỗi biến phải có kiểu xác định tại thời điểm biên dịch:

```csharp
int soLuong = 10;
string tenSanPham = "Sách C#";
bool conHang = true;
```

## 2. Kiểu giá trị và kiểu tham chiếu
| Nhóm | Ví dụ | Lưu ở đâu |
|------|-------|-----------|
| Value type | `int`, `double`, `bool`, `struct` | Stack |
| Reference type | `string`, `class`, `array` | Heap |

## Tóm tắt
Chọn đúng kiểu dữ liệu giúp chương trình an toàn và tiết kiệm bộ nhớ.',
   NULL, NULL, NULL, NULL, 1),

  (@Content2Id, @Lesson1Id, N'YoutubeVideo', NULL,
   N'https://www.youtube.com/watch?v=gfkTfcpWqAY', NULL, NULL, NULL, 2),

  (@Content3Id, @Lesson1Id, N'File', NULL, NULL, NULL,
   N'/files/csharp-cheatsheet.pdf', N'csharp-cheatsheet.pdf', 3),

  (@Content4Id, @Lesson2Id, N'Markdown',
   N'# Câu lệnh điều kiện và vòng lặp

## if / else
```csharp
if (diem >= 5) Console.WriteLine("Đậu");
else Console.WriteLine("Rớt");
```

## Vòng lặp for
```csharp
for (int i = 0; i < 5; i++) Console.WriteLine(i);
```',
   NULL, NULL, NULL, NULL, 1),

  -- Content video đã có sẵn VideoSummary: minh họa kết quả của màn Lesson Text Extract
  (@Content5Id, @Lesson3Id, N'YoutubeVideo', NULL,
   N'https://www.youtube.com/watch?v=RCUHmDBS1IY',
   N'# Lesson Summary

## Tổng quan
Video giới thiệu khái niệm lớp (class) và đối tượng (object) trong lập trình hướng đối tượng với C#.

## Các ý chính
- Lớp là bản thiết kế, đối tượng là thực thể được tạo ra từ bản thiết kế đó.
- Thuộc tính (property) mô tả trạng thái, phương thức (method) mô tả hành vi.
- Từ khóa `new` cấp phát đối tượng trên heap và gọi constructor.
- Tính đóng gói (encapsulation) che giấu dữ liệu nội bộ qua access modifier.

*Tóm tắt được sinh bởi GenAI từ transcript YouTube (AC-02f).*',
   NULL, NULL, 1);

/* ---------------------------------------------------------------------
   5. AiLessonDrafts — AI Lesson Staging (BR-07: chờ SME duyệt)
   --------------------------------------------------------------------- */
INSERT INTO AiLessonDrafts (Id, LessonId, CreatedById, SourceText, GeneratedText, Status, CreatedAt)
VALUES
  (@Draft1Id, @Lesson4Id, @SmeId,
   N'Kế thừa trong C#; lớp cha lớp con; từ khóa virtual và override; đa hình',
   N'# Kế thừa và đa hình trong C#

## Mục tiêu học tập
- Hiểu quan hệ kế thừa giữa lớp cha và lớp con
- Sử dụng được `virtual` / `override` để ghi đè hành vi

## 1. Kế thừa
```csharp
public class Animal { public virtual void Speak() => Console.WriteLine("..."); }
public class Dog : Animal { public override void Speak() => Console.WriteLine("Gâu gâu"); }
```

## 2. Đa hình
Cùng một lời gọi `Speak()` cho ra hành vi khác nhau tùy đối tượng thực tế.

## Tóm tắt
Kế thừa giúp tái sử dụng mã; đa hình giúp mở rộng hành vi mà không sửa mã cũ.',
   N'Pending', DATEADD(HOUR, -2, SYSUTCDATETIME())),

  (@Draft2Id, @Lesson2Id, @SmeId,
   N'Vòng lặp while và do-while trong C#',
   N'# Vòng lặp while và do-while

`while` kiểm tra điều kiện trước khi chạy thân vòng lặp; `do-while` chạy thân ít nhất một lần rồi mới kiểm tra.',
   N'Approved', DATEADD(DAY, -1, SYSUTCDATETIME()));

/* ---------------------------------------------------------------------
   6. LessonProgress — tiến độ học viên (FT-12)
   --------------------------------------------------------------------- */
INSERT INTO LessonProgress (Id, LessonId, StudentId, IsCompleted, CompletedAt) VALUES
  (NEWID(), @Lesson1Id, @Student1Id, 1, DATEADD(DAY, -3, SYSUTCDATETIME())),
  (NEWID(), @Lesson2Id, @Student1Id, 1, DATEADD(DAY, -1, SYSUTCDATETIME())),
  (NEWID(), @Lesson1Id, @Student2Id, 1, DATEADD(DAY, -2, SYSUTCDATETIME()));

/* ---------------------------------------------------------------------
   7. AiUsageLogs — giám sát token AI (VS 4.3)
   --------------------------------------------------------------------- */
INSERT INTO AiUsageLogs (Id, UserId, TaskType, TokensUsed, DurationMs, CreatedAt) VALUES
  (NEWID(), @SmeId, N'GenLesson', 1420, 2350, DATEADD(HOUR, -2, SYSUTCDATETIME())),
  (NEWID(), @SmeId, N'Summary',    860, 1780, DATEADD(HOUR, -1, SYSUTCDATETIME()));

UPDATE Users SET AiTokenUsed = 2280 WHERE Id = @SmeId;
GO

PRINT N'>>> Demo data cho nhóm màn hình Lesson đã được nạp.';
GO
