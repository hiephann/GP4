# GP6 — Traceability: Code Modules (nhóm màn hình **Lesson**)

Bảng dưới dùng để điền vào cột **Code Modules** (cột đứng ngay sau *InCharge*) trong sheet **WBS**.
Mỗi dòng WBS tương ứng một màn hình; copy đúng ô "Code Modules" sang file `.xlsx`.

---

## Bảng điền vào WBS

| # WBS | Màn hình | Feature | FT | **Code Modules** |
|---|---|---|---|---|
| 7 | Lesson Editor | Lesson | FT-02 | `LessonEditor.razor`, `LessonController`, `LessonService`, `LessonRepository`, `LessonDtos` (`CreateLessonRequest`, `UpsertLessonContentRequest`, `LessonContentDto`, `LessonListItemDto`, `ModuleOptionDto`), `LessonEntities` (`Lesson`, `LessonContent`), `LessonValidationException`, `MarkdownView.razor`, `EduNexusDbContext` |
| 8 | AI Lesson Staging | Lesson | FT-02 (BR-07) | `AiLessonStaging.razor`, `LessonController`, `LessonService`, `LessonRepository`, `IAiContentService`, `GeminiAiContentService`, `FakeAiContentService`, `AiOptions`, `AiServiceCollectionExtensions`, `LessonDtos` (`GenerateLessonRequest`, `AiLessonDraftDto`, `UpdateDraftRequest`), `LessonEntities.AiLessonDraft`, `SystemEntities.AiUsageLog`, `MarkdownView.razor` |
| 9 | Lesson Text Extract | Lesson | FT-02 (AC-02f) | `LessonTextExtract.razor`, `LessonController`, `LessonService`, `LessonRepository`, `IYoutubeTranscriptService`, `YoutubeTranscriptService`, `TranscriptResult`, `IAiContentService`, `GeminiAiContentService`, `LessonDtos` (`ExtractTranscriptRequest`, `LessonSummaryDto`), `TranscriptUnavailableException`, `SystemEntities.AiUsageLog` |
| 10 | Lesson View | Lesson | FT-06 | `LessonView.razor`, `LessonController`, `LessonService`, `LessonRepository`, `LessonDtos` (`LessonViewDto`, `LessonContentDto`, `UserOptionDto`), `LessonEntities` (`Lesson`, `LessonContent`), `SystemEntities.LessonProgress`, `MarkdownView.razor` |

---

## Đường dẫn file (để đối chiếu khi review)

### Tầng API — `src/EduNexus.Api/`

| Class / File | Đường dẫn | Vai trò |
|---|---|---|
| `LessonController` | `Lesson/Controllers/LessonController.cs` | 13 endpoint REST cho 4 màn hình |
| `ILessonService`, `LessonService` | `Lesson/Services/LessonService.cs` | Toàn bộ nghiệp vụ FT-02 / FT-06 |
| `LessonValidationException`, `TranscriptUnavailableException` | `Lesson/Services/LessonExceptions.cs` | Lỗi nghiệp vụ (BR-04, chỉ nhận YouTube…) |
| `ILessonRepository`, `LessonRepository` | `Lesson/Repositories/LessonRepository.cs` | Truy vấn EF Core, kế thừa `EfRepository<T>` |
| DTOs | `Lesson/DTOs/LessonDtos.cs` | Request / Response của 4 màn |
| Entities | `Lesson/Entities/LessonEntities.cs` | `Lesson`, `LessonContent`, `AiLessonDraft` |
| `IAiContentService`, `AiResult` | `Infrastructure/Ai/IAiContentService.cs` | Cổng GenAI dùng chung |
| `GeminiAiContentService` | `Infrastructure/Ai/GeminiAiContentService.cs` | Gọi Google Gemini thật |
| `FakeAiContentService` | `Infrastructure/Ai/FakeAiContentService.cs` | Fallback khi chưa cấu hình API key |
| `AiOptions` | `Infrastructure/Ai/AiOptions.cs` | Cấu hình section `Ai` |
| `AiServiceCollectionExtensions` | `Infrastructure/Ai/AiServiceCollectionExtensions.cs` | `AddEduNexusAi()` — DI dùng chung Api + Web |
| `IYoutubeTranscriptService`, `TranscriptResult` | `Infrastructure/Youtube/IYoutubeTranscriptService.cs` | Hợp đồng lấy transcript |
| `YoutubeTranscriptService` | `Infrastructure/Youtube/YoutubeTranscriptService.cs` | Phụ đề timedtext → YouTube Data API v3 |
| `DomainExceptionFilter` | `Infrastructure/DomainExceptionFilter.cs` | Lỗi nghiệp vụ → HTTP 400 + ProblemDetails |
| `EduNexusDbContext` | `Infrastructure/EduNexusDbContext.cs` | Ánh xạ EF Core (dùng chung toàn hệ thống) |

### Tầng giao diện — `src/EduNexus.Web/`

| File | Đường dẫn | Màn hình |
|---|---|---|
| `LessonEditor.razor` | `Components/Pages/Lesson/LessonEditor.razor` | Lesson Editor |
| `AiLessonStaging.razor` | `Components/Pages/Lesson/AiLessonStaging.razor` | AI Lesson Staging |
| `LessonTextExtract.razor` | `Components/Pages/Lesson/LessonTextExtract.razor` | Lesson Text Extract |
| `LessonView.razor` | `Components/Pages/Lesson/LessonView.razor` | Lesson View |
| `MarkdownView.razor` | `Components/Shared/MarkdownView.razor` | Render markdown (dùng chung 4 màn) |

### Database

| File | Nội dung |
|---|---|
| `database/EduNexus_CreateDatabase.sql` | Tạo schema — bảng liên quan: `Modules`, `Lessons`, `LessonContents`, `AiLessonDrafts`, `LessonProgress`, `AiUsageLogs` |
| `database/EduNexus_DemoData.sql` | Demo data nhóm Lesson (chạy sau script trên, idempotent) |

---

## Kiến trúc — luồng gọi

```
Blazor (.razor)  ─┐
                  ├─→  ILessonService  →  ILessonRepository  →  EF Core  →  SQL Server
API Controller   ─┘          │
                             ├─→  IAiContentService        (Gemini | Fake)
                             └─→  IYoutubeTranscriptService (timedtext | Data API v3)
```

Giao diện Blazor và API Controller **dùng chung một tầng Service** — không có logic nghiệp vụ nào nằm trong `.razor`.

---

## Ánh xạ nghiệp vụ (SRS → code)

| Quy tắc | Cài đặt ở đâu |
|---|---|
| **BR-04** — giới hạn định dạng tệp đính kèm | `LessonService.UpsertContentAsync` → `AllowedFileExtensions` |
| **BR-07** — nội dung AI phải được SME duyệt trước khi dùng | `LessonService.GenerateDraftAsync` luôn đặt `Status = "Pending"`; chỉ `ApproveDraftAsync` mới tạo `LessonContent` |
| **AC-02f** — transcript video → GenAI tóm tắt | `LessonService.ExtractAndSummarizeAsync` + `YoutubeTranscriptService` |
| **FT-12** — ghi nhận tiến độ học | `LessonService.MarkCompletedAsync` → upsert `LessonProgress` |
| **VS 4.3** — giám sát token AI | `LessonService.LogAiUsageAsync` → ghi `AiUsageLogs`, cộng dồn `Users.AiTokenUsed` |
| Chỉ nhúng video YouTube | `LessonService.UpsertContentAsync` gọi `IYoutubeTranscriptService.TryParseVideoId` |
