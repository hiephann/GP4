# AI Usage Report — GP6

> **Đổi tên file này thành `<RollNumber>_AI_Usage_Report.md`** trước khi nộp (ví dụ: `SE170123_AI_Usage_Report.md`).
> Các ô `<...>` là phần bạn tự điền.

| | |
|---|---|
| **Họ tên** | `<Họ và tên>` |
| **RollNumber** | `<RollNumber>` |
| **Nhóm** | G5 |
| **Phần được giao** | Nhóm màn hình **Lesson** — Lesson Editor, AI Lesson Staging, Lesson Text Extract, Lesson View (FT-02, FT-06) |
| **Deliverable** | GP6 — Final Project Codes |

---

## Phần 1 — AI được ứng dụng **trong sản phẩm**

Nhóm màn hình Lesson là nơi hệ thống EduNexus sử dụng GenAI trực tiếp. Có 2 tính năng AI:

### 1.1. AI Lesson Staging — sinh nội dung bài giảng (FT-02, BR-07)

| Hạng mục | Chi tiết |
|---|---|
| Nhà cung cấp | **Google Gemini** — `generativelanguage.googleapis.com`, model `gemini-2.0-flash` |
| Đầu vào | Đề cương / văn bản nguồn do SME nhập |
| Đầu ra | Bài giảng hoàn chỉnh dạng Markdown (mục tiêu học tập → các phần nội dung → ví dụ → tóm tắt) |
| Code | `GeminiAiContentService.GenerateLessonAsync()` |
| Prompt | System prompt tiếng Việt, ràng buộc "chỉ trả về Markdown, không thêm lời dẫn" |

**Điểm thiết kế quan trọng — BR-07:** AI **không bao giờ** ghi thẳng vào bài giảng. Kết quả luôn được lưu vào bảng `AiLessonDrafts` với `Status = "Pending"`. SME phải xem lại, có thể **sửa nội dung**, rồi mới bấm **Phê duyệt** — lúc đó hệ thống mới tạo `LessonContent` thật cho bài học. SME cũng có thể **Từ chối** (`Status = "Rejected"`). Con người luôn là người ra quyết định cuối cùng.

### 1.2. Lesson Text Extract — tóm tắt video thành lesson summary (AC-02f)

Hệ thống thử lần lượt 3 tầng để lấy văn bản mô tả video:

1. **Phụ đề video** — đọc `captionTracks` từ trang video rồi tải endpoint `timedtext`.
   *Thực tế:* YouTube hiện chặn phần lớn request server-side (yêu cầu proof-of-origin token của trình duyệt), nên tầng này thường thất bại. Code vẫn giữ vì khi lấy được thì đây là nguồn đầy đủ nhất.
2. **YouTube Data API v3** (`videos.list`) — lấy tiêu đề, kênh và mô tả video bằng API key. Đây là tầng chạy ổn định.
3. **Dán transcript thủ công** — SME tự dán transcript vào ô text.

Văn bản thu được đưa sang Gemini (`SummarizeTranscriptAsync`) để sinh lesson summary Markdown, sau đó SME có thể lưu vào `LessonContents.VideoSummary` của đúng video.

| Code | `YoutubeTranscriptService` + `GeminiAiContentService.SummarizeTranscriptAsync()` |
|---|---|

### 1.3. Kiến trúc AI — thay được nhà cung cấp, demo được khi không có key

```
IAiContentService  ──┬── GeminiAiContentService   (gọi API thật)
                     └── FakeAiContentService     (fallback)
```

`AddEduNexusAi()` đọc `Ai:Gemini:ApiKey` từ cấu hình: **có key** → dùng Gemini; **không có key** → tự động dùng `FakeAiContentService` sinh nội dung có cấu trúc (gắn nhãn `[MOCK]`). Nhờ vậy đồ án luôn chạy được khi chấm, kể cả trên máy không có API key hay không có mạng. Muốn đổi sang OpenAI/Claude chỉ cần thêm một class implement `IAiContentService`.

### 1.4. Giám sát chi phí AI (VS 4.3)

Mỗi lần gọi AI, `LessonService.LogAiUsageAsync()` ghi một dòng vào bảng `AiUsageLogs` (`TaskType`, `TokensUsed`, `DurationMs`) và cộng dồn vào `Users.AiTokenUsed`. `TokensUsed` lấy từ `usageMetadata.totalTokenCount` mà Gemini trả về.

| TaskType | Sinh ra ở đâu |
|---|---|
| `GenLesson` | AI Lesson Staging |
| `Summary` | Lesson Text Extract |

### 1.5. Cấu hình để chạy AI thật

Điền vào `src/EduNexus.Web/appsettings.Development.json` (file này không commit key lên Git):

```jsonc
{
  "Ai": {
    "Gemini":  { "ApiKey": "<key từ https://aistudio.google.com/apikey>" },
    "Youtube": { "ApiKey": "<key YouTube Data API v3 từ Google Cloud Console>" }
  }
}
```

---

## Phần 2 — AI hỗ trợ **quá trình phát triển**

> Phần này ghi lại việc cá nhân dùng AI như một công cụ khi làm đồ án. Hãy chỉnh sửa cho khớp thực tế của bạn.

| Công cụ | Mục đích sử dụng |
|---|---|
| `<Claude Code / GitHub Copilot / ChatGPT / ...>` | `<điền>` |

### 2.1. Đã dùng AI vào những việc gì

- `<ví dụ: sinh khung code tầng Service/Repository từ interface có sẵn>`
- `<ví dụ: viết script SQL demo data>`
- `<ví dụ: giải thích lỗi khi gọi API YouTube>`
- `<ví dụ: rà soát lại code trước khi commit>`

### 2.2. Việc AI làm **không** đúng, phải tự sửa

- **YouTube transcript:** giải pháp ban đầu (endpoint `timedtext`) chạy thất bại trên thực tế vì YouTube đã chặn request từ server. Phải tự kiểm chứng bằng cách gọi thử nhiều video, xác định nguyên nhân rồi bổ sung tầng YouTube Data API v3 và nhánh dán transcript thủ công.
- **Script demo data:** bản đầu chỉ xóa dữ liệu theo GUID cố định nên chạy lại lần hai bị lỗi khóa ngoại (draft AI phát sinh trong lúc demo tham chiếu tới user). Phải sửa lại điều kiện xóa theo `CreatedById` / `LessonId`.
- `<bổ sung các trường hợp khác của bạn>`

### 2.3. Nhận xét

`<Viết 3-5 câu: AI giúp được gì, hạn chế ở đâu, bạn kiểm chứng kết quả của AI bằng cách nào.>`

---

## Phần 3 — Bằng chứng đã chạy thật

Đã kiểm thử end-to-end trên SQL Server local (xem `docs/GP6_Traceability_Lesson.md` để tra file code):

| Kịch bản | Kết quả |
|---|---|
| Tạo bài học, thêm nội dung Markdown / YouTube / File | OK |
| Dán link **không phải YouTube** | Bị chặn — HTTP 400 |
| Đính kèm file `.exe` | Bị chặn theo BR-04 — HTTP 400 |
| AI sinh bản nháp → SME sửa → phê duyệt | Nội dung được đổ vào bài học; duyệt lần 2 bị chặn |
| Tóm tắt transcript | Sinh lesson summary Markdown |
| Đánh dấu hoàn thành bài học | Ghi `LessonProgress` |
| Ghi log token AI | `AiUsageLogs` có bản ghi, `Users.AiTokenUsed` cộng dồn |
