using EduNexus.Api.Lesson.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EduNexus.Api.Infrastructure;

/// <summary>
/// Đổi lỗi nghiệp vụ thành 400 + ProblemDetails thay vì 500 — để Swagger/client nhận được thông báo rõ ràng.
/// </summary>
public class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var message = context.Exception switch
        {
            LessonValidationException ex => ex.Message,
            TranscriptUnavailableException ex => ex.Message,
            _ => null
        };

        if (message is null) return;

        context.Result = new BadRequestObjectResult(new ProblemDetails
        {
            Title = "Yêu cầu không hợp lệ",
            Detail = message,
            Status = StatusCodes.Status400BadRequest
        });
        context.ExceptionHandled = true;
    }
}
