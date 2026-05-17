using Microsoft.EntityFrameworkCore;
using ReviewerPortal.API.Application.Interfaces;
using ReviewerPortal.API.Application.Services;
using ReviewerPortal.API.Infrastructure.ExternalApi;
using ReviewerPortal.API.Infrastructure.Persistence;
using ReviewerPortal.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ReviewerPortalDb"));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddScoped<IUniversityApiClient, UniversityApiClient>();
builder.Services.AddScoped<IUniversityService, UniversityService>();
builder.Services.AddScoped<IUserService, UserService>();

var universityApiBaseUrl = builder.Configuration["UniversityApiBaseUrl"]
    ?? throw new InvalidOperationException("Configuration value 'UniversityApiBaseUrl' is required.");

builder.Services.AddHttpClient("UniversityApi", client =>
{
    client.BaseAddress = new Uri(universityApiBaseUrl);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
