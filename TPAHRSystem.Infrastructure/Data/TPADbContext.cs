// =============================================================================
// THE EXACT FIX: Replace your current TPADbContext.cs with this complete version
// File: TPAHRSystem.Infrastructure/Data/TPADbContext.cs (REPLACE ENTIRE FILE)
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

        // Core Authentication & User Management
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<UserSession> UserSessions { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;

        // Menu & Permissions System - THESE WERE MISSING IN YOUR CURRENT VERSION
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; } = null!;

        // Dashboard & UI - THESE WERE MISSING IN YOUR CURRENT VERSION
        public DbSet<DashboardStat> DashboardStats { get; set; } = null!;
        public DbSet<QuickAction> QuickActions { get; set; } = null!;

        // Activity Tracking - THESE WERE MISSING IN YOUR CURRENT VERSION
        public DbSet<ActivityType> ActivityTypes { get; set; } = null!;
        public DbSet<RecentActivity> RecentActivities { get; set; } = null!;

        // Leave Management
        public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;

        // Time & Attendance
        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;
        public DbSet<TimeSheet> TimeSheets { get; set; } = null!;
        public DbSet<Schedule> Schedules { get; set; } = null!;

        // Onboarding System
        public DbSet<OnboardingTask> OnboardingTasks { get; set; } = null!;
        public DbSet<OnboardingTemplate> OnboardingTemplates { get; set; } = null!;
        public DbSet<OnboardingDocument> OnboardingDocuments { get; set; } = null!;
        public DbSet<OnboardingProgress> OnboardingProgress { get; set; } = null!;
        public DbSet<OnboardingChecklist> OnboardingChecklists { get; set; } = null!;



       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUserEntities(modelBuilder);
            ConfigureMenuEntities(modelBuilder);
            ConfigureDashboardEntities(modelBuilder);
            ConfigureActivityEntities(modelBuilder);
            ConfigureOnboardingEntities(modelBuilder);
            ConfigureTimeAttendanceEntities(modelBuilder);
            ConfigureLeaveEntities(modelBuilder);
        }

        private void ConfigureUserEntities(ModelBuilder modelBuilder)
        {
            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Salt).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // UserSession Configuration
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.ToTable("UserSessions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionToken).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.SessionToken).IsUnique();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserSessions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Employee Configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(20);
                entity.Property(e => e.JobTitle).HasMaxLength(50);
                entity.Property(e => e.Position).HasMaxLength(50);
                entity.Property(e => e.WorkLocation).HasMaxLength(50);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
                entity.Property(e => e.EmploymentStatus).HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsOnboardingLocked).HasDefaultValue(true);
                entity.Property(e => e.HireDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Employees)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Manager)
                      .WithMany(m => m.DirectReports)
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Department Configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Departments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Code).IsUnique();
            });
        }

        private void ConfigureMenuEntities(ModelBuilder modelBuilder)
        {
            // MenuItem Configuration
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.ToTable("MenuItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Route).HasMaxLength(200);
                entity.Property(e => e.Icon).HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Parent)
                      .WithMany(p => p.Children)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Route);
            });

            // RoleMenuPermission Configuration
            modelBuilder.Entity<RoleMenuPermission>(entity =>
            {
                entity.ToTable("RoleMenuPermissions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CanView).HasDefaultValue(false);
                entity.Property(e => e.CanEdit).HasDefaultValue(false);
                entity.Property(e => e.CanDelete).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.MenuItem)
                      .WithMany(m => m.RolePermissions)
                      .HasForeignKey(e => e.MenuItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.Role, e.MenuItemId }).IsUnique();
            });
        }

        private void ConfigureDashboardEntities(ModelBuilder modelBuilder)
        {
            // DashboardStat Configuration
            modelBuilder.Entity<DashboardStat>(entity =>
            {
                entity.ToTable("DashboardStats");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StatName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StatValue).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Subtitle).HasMaxLength(200);
                entity.Property(e => e.IconName).HasMaxLength(50);
                entity.Property(e => e.StatColor).HasMaxLength(20);
                entity.Property(e => e.ApplicableRoles).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // QuickAction Configuration
            modelBuilder.Entity<QuickAction>(entity =>
            {
                entity.ToTable("QuickActions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ActionKey).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IconName).HasMaxLength(50);
                entity.Property(e => e.Route).HasMaxLength(200);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.Property(e => e.ApplicableRoles).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.ActionKey).IsUnique();
            });
        }

        private void ConfigureActivityEntities(ModelBuilder modelBuilder)
        {
            // ActivityType Configuration
            modelBuilder.Entity<ActivityType>(entity =>
            {
                entity.ToTable("ActivityTypes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IconName).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(20).HasDefaultValue("#1976d2");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.Name).IsUnique();
            });

            // RecentActivity Configuration
            modelBuilder.Entity<RecentActivity>(entity =>
            {
                entity.ToTable("RecentActivities");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.EntityType).HasMaxLength(50);
                entity.Property(e => e.IPAddress).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                      .WithMany(u => u.RecentActivities)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ActivityType)
                      .WithMany(at => at.RecentActivities)
                      .HasForeignKey(e => e.ActivityTypeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.EntityType });
            });
        }

        private void ConfigureOnboardingEntities(ModelBuilder modelBuilder)
        {
            // OnboardingTask Configuration
            modelBuilder.Entity<OnboardingTask>(entity =>
            {
                entity.ToTable("OnboardingTasks");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.AssignedToRole).HasMaxLength(50);
                entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("Medium");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
               
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                

                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.AssignedToRole);
                entity.HasIndex(e => e.Status);
            });

            // Other onboarding entities configuration would go here
        }

        private void ConfigureTimeAttendanceEntities(ModelBuilder modelBuilder)
        {
            // TimeEntry Configuration
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.ToTable("TimeEntries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClockIn).IsRequired();
                entity.Property(e => e.Location).HasMaxLength(200);
               
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                //entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.ClockIn);
                entity.HasIndex(e => new { e.EmployeeId, e.ClockIn });
            });

            // Other time & attendance entities would go here
        }

        private void ConfigureLeaveEntities(ModelBuilder modelBuilder)
        {
            // LeaveRequest Configuration
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.ToTable("LeaveRequests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.EmployeeId, e.StartDate });
            });
        }

        // =============================================================================
        // ADD THESE METHODS TO YOUR TPADbContext.cs FILE
        // =============================================================================

        // Add this at the end of your TPADbContext class, before the closing brace

        /// <summary>
        /// Check if user role has permission for a specific route
        /// </summary>
        public async Task<bool> HasRoutePermissionAsync(string userRole, string route, string permissionType = "VIEW")
        {
            try
            {
                // If we don't have the RoleMenuPermissions table set up yet, allow all access
                if (!RoleMenuPermissions.Any())
                {
                    return true; // Allow access when no permissions are configured
                }

                var hasPermission = await RoleMenuPermissions
                    .Include(rmp => rmp.MenuItem)
                    .AnyAsync(rmp => rmp.Role == userRole &&
                                   rmp.MenuItem.Route == route &&
                                   rmp.CanView == true);

                return hasPermission;
            }
            catch (Exception)
            {
                // On error, allow access to prevent blocking
                return true;
            }
        }

        /// <summary>
        /// Check if user role has permission for a specific menu
        /// </summary>
        public async Task<bool> CheckUserMenuPermissionAsync(string userRole, string menuName, string permissionType = "VIEW")
        {
            try
            {
                // If we don't have the RoleMenuPermissions table set up yet, allow all access
                if (!RoleMenuPermissions.Any())
                {
                    return true; // Allow access when no permissions are configured
                }

                var hasPermission = await RoleMenuPermissions
                    .Include(rmp => rmp.MenuItem)
                    .AnyAsync(rmp => rmp.Role == userRole &&
                                   rmp.MenuItem.Name == menuName &&
                                   rmp.CanView == true);

                return hasPermission;
            }
            catch (Exception)
            {
                // On error, allow access to prevent blocking
                return true;
            }
        }
    }
}