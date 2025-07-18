// =============================================================================
// COMPLETE FIXED TPADbContext.cs - Final Version
// File: TPAHRSystem.Infrastructure/Data/TPADbContext.cs (Replace entire file)
// =============================================================================

using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Core.Models;

namespace TPAHRSystem.Infrastructure.Data
{
    public class TPADbContext : DbContext
    {
        public TPADbContext(DbContextOptions<TPADbContext> options) : base(options)
        {
        }

        // Essential DbSets for Authentication
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<UserSession> UserSessions { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<DashboardStat> DashboardStats { get; set; } = null!;

        // Time and Attendance DbSets
        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;
        public DbSet<TimeSheet> TimeSheets { get; set; } = null!;
        public DbSet<Schedule> Schedules { get; set; } = null!;

        // Onboarding DbSets
        public DbSet<OnboardingTask> OnboardingTasks { get; set; } = null!;
        public DbSet<OnboardingTemplate> OnboardingTemplates { get; set; } = null!;
        public DbSet<OnboardingDocument> OnboardingDocuments { get; set; } = null!;
        public DbSet<OnboardingProgress> OnboardingProgress { get; set; } = null!;
        public DbSet<OnboardingChecklist> OnboardingChecklists { get; set; } = null!;

        // Other existing DbSets
        public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;
        public DbSet<QuickAction> QuickActions { get; set; } = null!;
        public DbSet<ActivityType> ActivityTypes { get; set; } = null!;
        public DbSet<RecentActivity> RecentActivities { get; set; } = null!;
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure all entities
            ConfigureUserEntities(modelBuilder);
            ConfigureTimeAttendanceEntities(modelBuilder);
            ConfigureOnboardingEntities(modelBuilder);
            ConfigureOtherEntities(modelBuilder);
        }

        private void ConfigureUserEntities(ModelBuilder modelBuilder)
        {
            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Salt).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Employee Configuration - Match existing database schema
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Configure properties that exist in database
                entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.JobTitle).HasMaxLength(100);
              //  entity.Property(e => e.EmployeeType).HasMaxLength(50);
               // entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Indexes for existing columns
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Configure relationships only for columns that exist in database
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Manager)
                      .WithMany(e => e.DirectReports)
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // IGNORE properties that don't exist in database schema
                // These properties exist in the model but not in the database
                entity.Ignore(e => e.DateOfBirth);
                entity.Ignore(e => e.Gender);
                entity.Ignore(e => e.Address);
                entity.Ignore(e => e.City);
                entity.Ignore(e => e.State);
                entity.Ignore(e => e.ZipCode);
                entity.Ignore(e => e.TerminationDate);
                entity.Ignore(e => e.Position);
                entity.Ignore(e => e.WorkLocation);
                entity.Ignore(e => e.Salary);
                entity.Ignore(e => e.EmploymentStatus);
                entity.Ignore(e => e.IsActive);

                // NOTE: Navigation properties marked as [NotMapped] in the model 
                // are automatically ignored, so no need to explicitly ignore them here
            });

            // UserSession Configuration
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionToken).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.SessionToken).IsUnique();

                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserSessions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Department Configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });
        }

        private void ConfigureTimeAttendanceEntities(ModelBuilder modelBuilder)
        {
            // TimeEntry Configuration
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasDefaultValue("Active").HasMaxLength(20);
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.EmployeeId, e.ClockIn });
            });

            // TimeSheet Configuration
            modelBuilder.Entity<TimeSheet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasDefaultValue("Draft").HasMaxLength(20);
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.RegularHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.OvertimeHours).HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Approver)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedBy)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.EmployeeId, e.WeekStartDate });
            });

            // Schedule Configuration
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DayOfWeek).IsRequired();

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.EmployeeId, e.DayOfWeek });
            });
        }

        private void ConfigureOnboardingEntities(ModelBuilder modelBuilder)
        {
            // OnboardingTask Configuration
            modelBuilder.Entity<OnboardingTask>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("PENDING");
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20).HasDefaultValue("MEDIUM");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Template)
                      .WithMany()
                      .HasForeignKey(e => e.TemplateId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.AssignedBy)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedById)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.EmployeeId, e.Status });
                entity.HasIndex(e => e.DueDate);
                entity.HasIndex(e => e.Category);
            });

            // OnboardingTemplate Configuration
            modelBuilder.Entity<OnboardingTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ForRole).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ForRole, e.IsActive });
            });

            // OnboardingDocument Configuration
            modelBuilder.Entity<OnboardingDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Task)
                      .WithMany()
                      .HasForeignKey(e => e.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UploadedBy)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedById)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.TaskId, e.Required });
            });

            // OnboardingProgress Configuration
            modelBuilder.Entity<OnboardingProgress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompletionPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.EmployeeId).IsUnique();
            });

            // OnboardingChecklist Configuration
            modelBuilder.Entity<OnboardingChecklist>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AssignedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("ASSIGNED");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Template)
                      .WithMany()
                      .HasForeignKey(e => e.TemplateId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedBy)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedById)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.EmployeeId, e.Status });
            });
        }

        private void ConfigureOtherEntities(ModelBuilder modelBuilder)
        {
            // DashboardStat Configuration
            modelBuilder.Entity<DashboardStat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StatKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StatName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StatValue).IsRequired().HasMaxLength(100);
            });

            // QuickAction Configuration
            modelBuilder.Entity<QuickAction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ActionKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Color).HasDefaultValue("#1976d2").HasMaxLength(50);
            });

            // ActivityType Configuration
            modelBuilder.Entity<ActivityType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Color).HasDefaultValue("#1976d2").HasMaxLength(50);
            });

            // *** FIXED RecentActivity Configuration - NO Employee Navigation ***
            modelBuilder.Entity<RecentActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Details).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.IPAddress).IsRequired().HasMaxLength(50);

                // Configure ONLY User and ActivityType relationships
                entity.HasOne(e => e.User)
                      .WithMany(u => u.RecentActivities)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ActivityType)
                      .WithMany(at => at.RecentActivities)
                      .HasForeignKey(e => e.ActivityTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // IGNORE Employee navigation property to avoid schema issues
                entity.Ignore(e => e.Employee);

                // Keep EmployeeId as a simple nullable integer column (no FK)
                entity.Property(e => e.EmployeeId).IsRequired(false);

                // Add indexes for performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ActivityTypeId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.EmployeeId); // Index the column but don't create FK
            });

            // LeaveRequest Configuration
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Reason).HasMaxLength(1000);

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.EmployeeId, e.Status });
                entity.HasIndex(e => e.StartDate);
            });

            // MenuItem Configuration
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Parent)
                      .WithMany(e => e.Children)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.IsActive, e.SortOrder });
            });

            // RoleMenuPermission Configuration
            modelBuilder.Entity<RoleMenuPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.MenuItem)
                      .WithMany()
                      .HasForeignKey(e => e.MenuItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.Role, e.MenuItemId }).IsUnique();
            });
        }
    }
}