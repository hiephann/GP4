namespace EduNexus.Api.Lesson.Services;

/// <summary>Vi phạm quy tắc nghiệp vụ của feature Lesson (BR-04, chỉ nhận YouTube, dữ liệu rỗng...).</summary>
public class LessonValidationException : Exception
{
    public LessonValidationException(string message) : base(message) { }
}

/// <summary>Video không có phụ đề và SME cũng chưa dán transcript thủ công.</summary>
public class TranscriptUnavailableException : Exception
{
    public TranscriptUnavailableException(string message) : base(message) { }
}
