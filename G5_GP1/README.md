# EduNexus — GP1 (Nhóm G5)

Nền tảng Học tập & Đào tạo Tích hợp AI. Đây là deliverable **GP1**: script tạo database
toàn hệ thống + project skeleton (code module chưa chi tiết) cho các chức năng theo
nhóm màn hình trong file phạm vi (`*.xlsx`).

## Công nghệ
- **.NET 8 (LTS)** — pin SDK qua `global.json`
- **EduNexus.Api**: ASP.NET Core Web API (skeleton) — kiến trúc **package-by-feature**,
  đủ layer **Controller → Service → Repository → Entity / DTO**
- **EduNexus.Web**: **Blazor Server** — giao diện 26 màn hình, truy cập **EF Core DbContext
  trực tiếp** vào SQL Server (chạy được ngay với DB thật)
- **SQL Server** + **EF Core 8**

## Giao diện (Blazor) — 26 màn hình
Project `src/EduNexus.Web` chứa giao diện cho toàn bộ 26 màn hình, gom theo nhóm trong
`Components/Pages/{Common,Lesson,Assignment,Flashcard,Question,Quiz}`. Menu trái liệt kê
đầy đủ; trang chủ là bảng điều hướng tới từng màn hình.

Chạy giao diện:
```bash
dotnet run --project src/EduNexus.Web
```
Mở trình duyệt tại URL hiển thị (vd `http://localhost:5112`). Connection string mặc định
(`src/EduNexus.Web/appsettings.json`): `Server=localhost; User Id=sa; Password=123456`.

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

> Mỗi feature có đủ 5 lớp; CRUD cơ bản nằm ở `EfRepository<T>`.
>
> **Trạng thái (GP6):** feature **Lesson** đã hoàn thiện — Blazor → `ILessonService` → `ILessonRepository` → EF Core,
> tích hợp GenAI (Gemini) và YouTube. Xem `docs/GP6_Traceability_Lesson.md`.
> Các feature còn lại vẫn để `NotImplementedException()` kèm `// TODO`.

## 1) Tạo database

Mở SQL Server (localhost) rồi chạy script:

```bash
sqlcmd -S localhost -i database/EduNexus_CreateDatabase.sql
sqlcmd -S localhost -i database/EduNexus_DemoData.sql      # demo data (nhóm Lesson)
```

Hoặc mở 2 file `.sql` bằng SSMS và **Execute** theo thứ tự trên.
Script đầu tạo database `EduNexus` và toàn bộ bảng (Auth, Course/Module/Lesson,
Question, Flashcard, Assignment, Quiz, Class, Catalog/Payment, Progress/Log/Notification)
cùng seed bảng `Roles`. Script thứ hai nạp dữ liệu demo (chạy lại nhiều lần vẫn an toàn).

## 1b) Cấu hình GenAI (tùy chọn)

Các màn hình Lesson dùng **Google Gemini** để sinh nội dung bài giảng và tóm tắt video.
Điền key vào `src/EduNexus.Web/appsettings.Development.json` (**không commit key**):

```jsonc
{
  "Ai": {
    "Gemini":  { "ApiKey": "<https://aistudio.google.com/apikey>" },
    "Youtube": { "ApiKey": "<YouTube Data API v3 — Google Cloud Console>" }
  }
}
```

**Bỏ trống cũng chạy được**: hệ thống tự dùng `FakeAiContentService` sinh nội dung mẫu,
còn Lesson Text Extract sẽ chuyển sang nhánh dán transcript thủ công.

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
