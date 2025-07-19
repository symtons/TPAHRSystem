// =============================================================================
// SESSION-BASED PROGRAM.CS - NO JWT AUTHENTICATION
// File: TPAHRSystem.API/Program.cs (REPLACE ENTIRE FILE)
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
        Description = "API for Tennessee Personal Assistance HR Management System with Session-Based Authentication"
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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TPA HR System API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// CORS must come before other middleware
app.UseCors("AllowReactApp");

// Simple request logging
app.Use(async (context, next) =>
{
    Console.WriteLine($"?? Request: {context.Request.Method} {context.Request.Path}");

    // Log authorization header for debugging
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;
        Console.WriteLine($"?? Auth Token: {token.Substring(0, Math.Min(20, token.Length))}...");
    }
    else
    {
        Console.WriteLine("?? No auth header");
    }

    await next();

    Console.WriteLine($"?? Response: {context.Response.StatusCode}");
});

// Health Check Endpoint
app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow.ToString("o"),
    api = "TPA HR Management System",
    version = "1.0.0",
    authType = "Session-based"
});

// Map Controllers
app.MapControllers();

// Simple database check (Development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<TPADbContext>();

        try
        {
            if (await context.Database.CanConnectAsync())
            {
                Console.WriteLine("? Database connection successful");

                var userCount = await context.Users.CountAsync();
                var menuCount = await context.MenuItems.CountAsync();
                var permissionCount = await context.RoleMenuPermissions.CountAsync();
                var activeSessionCount = await context.UserSessions.CountAsync(s => s.IsActive && s.ExpiresAt > DateTime.UtcNow);

                Console.WriteLine($"?? Database Status:");
                Console.WriteLine($"   Users: {userCount}");
                Console.WriteLine($"   Menu Items: {menuCount}");
                Console.WriteLine($"   Role Permissions: {permissionCount}");
                Console.WriteLine($"   Active Sessions: {activeSessionCount}");
            }
            else
            {
                Console.WriteLine("? Database connection failed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"?? Error during database initialization: {ex.Message}");
        }
    }
}

Console.WriteLine("?? TPA HR Management System API started successfully");
Console.WriteLine("?? Authentication: Session-based tokens");
Console.WriteLine("?? Menu System: Role-based permissions");

app.Run();