// =============================================================================
// UPDATED PROGRAM.CS - WITH ONBOARDING SERVICE REGISTRATION
// File: TPAHRSystem.API/Program.cs (Replace existing)
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TPAHRSystem.Infrastructure.Data;
using TPAHRSystem.Application.Services;
using TPAHRSystem.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<TPADbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Application Services
builder.Services.AddScoped<TPAHRSystem.Application.Services.IAuthService, TPAHRSystem.Application.Services.AuthService>();
builder.Services.AddScoped<TPAHRSystem.API.Services.IDashboardService, TPAHRSystem.API.Services.DashboardService>();
builder.Services.AddScoped<ITimeAttendanceService, MockTimeAttendanceService>();

// Add Onboarding Service
builder.Services.AddScoped<TPAHRSystem.API.Services.IOnboardingService, TPAHRSystem.API.Services.OnboardingService>();

// Add Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Information);
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:7062",
                "https://localhost:7062"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TPA HR Management System API",
        Version = "v1",
        Description = "API for Tennessee Personal Assistance HR Management System with Session-Based Authentication and Onboarding Workflow"
    });

    // Add Session Token Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Session Token Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {session-token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Authorization",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Enable XML comments if available
    c.EnableAnnotations();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TPA HR System API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

// Enable CORS
app.UseCors("AllowReactApp");

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Add custom middleware for logging requests
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");

    await next();

    logger.LogInformation($"Response: {context.Response.StatusCode}");
});

// Map controllers
app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "TPA HR System API",
    version = "1.0.0"
});

// Add API info endpoint
app.MapGet("/api/info", () => new
{
    name = "TPA HR Management System API",
    version = "1.0.0",
    description = "Complete HR Management System with Employee Onboarding Workflow",
    features = new[]
    {
        "Session-based Authentication",
        "Employee Onboarding Workflow",
        "Role-based Menu Access Control",
        "HR Task Management",
        "Dashboard Analytics",
        "Time & Attendance (Coming Soon)"
    },
    endpoints = new
    {
        authentication = "/api/auth",
        onboarding = "/api/onboarding",
        menus = "/api/menu",
        dashboard = "/api/dashboard"
    },
    documentation = "/swagger"
});

Console.WriteLine("🚀 TPA HR Management System API is starting...");
Console.WriteLine("📋 Features enabled:");
Console.WriteLine("   ✅ Employee Onboarding Workflow");
Console.WriteLine("   ✅ Session-based Authentication");
Console.WriteLine("   ✅ Role-based Access Control");
Console.WriteLine("   ✅ HR Task Management");
Console.WriteLine("   ✅ Dashboard Analytics");
Console.WriteLine("📖 API Documentation available at: /swagger");

app.Run();