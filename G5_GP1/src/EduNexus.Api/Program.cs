using EduNexus.Api.Assignment.Repositories;
using EduNexus.Api.Assignment.Services;
using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Common.Services;
using EduNexus.Api.Flashcard.Repositories;
using EduNexus.Api.Flashcard.Services;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Lesson.Repositories;
using EduNexus.Api.Lesson.Services;
using EduNexus.Api.Question.Repositories;
using EduNexus.Api.Question.Services;
using EduNexus.Api.Quiz.Repositories;
using EduNexus.Api.Quiz.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----- DbContext (SQL Server) -----
builder.Services.AddDbContext<EduNexusDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----- Repositories -----
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IFlashcardRepository, FlashcardRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();

// ----- Services (theo feature / nhóm màn hình) -----
// Common (Member 1)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<ICourseService, CourseService>();
// Lesson (Member 2)
builder.Services.AddScoped<ILessonService, LessonService>();
// Assignment (Member 3)
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
// Flashcard (Member 4)
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
// Question (Member 5)
builder.Services.AddScoped<IQuestionService, QuestionService>();
// Quiz (Member 6)
builder.Services.AddScoped<IQuizService, QuizService>();

// ----- MVC + Swagger -----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EduNexus API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
