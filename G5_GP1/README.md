# EduNexus — GP1 (Nhóm G5)

Nền tảng Học tập & Đào tạo Tích hợp AI. Đây là deliverable **GP1**: script tạo database
toàn hệ thống + project skeleton (code module chưa chi tiết) cho các chức năng theo
nhóm màn hình trong file phạm vi (`*.xlsx`).

## Công nghệ
- **.NET 8 (LTS)** — ASP.NET Core Web API (pin SDK qua `global.json`)
- **SQL Server** + **EF Core 8**
- Kiến trúc: **single project, package-by-feature**, đủ layer
  **Controller → Service → Repository → Entity / DTO**

## Phân công nhóm màn hình (mỗi thành viên 1 nhóm, theo thứ tự trong xlsx)

| # | Feature | Màn hình | FT |
|---|---------|----------|----|
| 1 | **Common** | User Login, Student Dashboard, Personal Progress, Course List, Course Structure | FT-01, FT-12, FT-14 |
| 2 | **Lesson** | Lesson Editor, AI Lesson Staging, Lesson Text Extract, Lesson View | FT-02, FT-06 |
| 3 | **Assignment** | Assignment List, Detail, Submit, Result | FT-05, FT-08 |
| 4 | **Flashcard** | Flashcard Editor, AI Staging, Library, Practice | FT-04, FT-06 |
| 5 | **Question** | Question Bank, Detail, AI Staging, Import | FT-03 |
| 6 | **Quiz** | Quiz History, New Quiz, Taking, Results, Review | FT-07 |

## Cấu trúc thư mục

```
G5_GP1/
├─ global.json                       # pin SDK .NET 8
├─ EduNexus.sln
├─ database/
│  └─ EduNexus_CreateDatabase.sql    # script tạo DB TOÀN BỘ hệ thống (~30 bảng)
└─ src/EduNexus.Api/
   ├─ Program.cs                     # DI services/repos, DbContext, Swagger
   ├─ appsettings.json               # ConnectionString SQL Server
   ├─ Infrastructure/EduNexusDbContext.cs
   ├─ Common/      Controllers/ Services/ Repositories/ Entities/ DTOs/
   ├─ Lesson/      ...
   ├─ Assignment/  ...
   ├─ Flashcard/   ...
   ├─ Question/    ...
   └─ Quiz/        ...
```

> Mỗi feature có đủ 5 lớp. Phần thân phương thức nghiệp vụ để `NotImplementedException()`
> kèm `// TODO` (đúng yêu cầu "chưa cần chi tiết"); CRUD cơ bản nằm ở `EfRepository<T>`.

## 1) Tạo database

Mở SQL Server (localhost) rồi chạy script:

```bash
sqlcmd -S localhost -i database/EduNexus_CreateDatabase.sql
```

Hoặc mở `database/EduNexus_CreateDatabase.sql` bằng SSMS và **Execute**.
Script sẽ tạo database `EduNexus` và toàn bộ bảng (Auth, Course/Module/Lesson,
Question, Flashcard, Assignment, Quiz, Class, Catalog/Payment, Progress/Log/Notification)
cùng seed bảng `Roles`.

## 2) Chạy API

Sửa chuỗi kết nối trong `src/EduNexus.Api/appsettings.json` nếu cần, rồi:

```bash
dotnet build EduNexus.sln
dotnet run --project src/EduNexus.Api
```

Mở Swagger UI tại `https://localhost:<port>/swagger` để xem toàn bộ endpoint của 6 feature.

## Tham chiếu tài liệu
- `01_EduNexus_VS_v2.pdf` — Vision & Scope v2
- `02_EduNexus_SRS_v2.pdf` — SRS v2 (FT-01..FT-15, BR-01..BR-27, NFR, NTF)
