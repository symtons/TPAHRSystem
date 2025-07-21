// =============================================================================
// COMPLETE FULL FUNCTIONALITY TPADbContext - ALL FEATURES INCLUDED
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

        // Menu & Permissions System
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; } = null!;

        // Dashboard & UI
        public DbSet<DashboardStat> DashboardStats { get; set; } = null!;
        public DbSet<QuickAction> QuickActions { get; set; } = null!;

        // Activity Tracking
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

            // Configure all entity types
            ConfigureUserEntities(modelBuilder);
            ConfigureEmployeeEntities(modelBuilder);
            ConfigureDepartmentEntities(modelBuilder);
            ConfigureMenuEntities(modelBuilder);
            ConfigureDashboardEntities(modelBuilder);
            ConfigureActivityEntities(modelBuilder);
            ConfigureLeaveEntities(modelBuilder);
            ConfigureTimeAttendanceEntities(modelBuilder);
            ConfigureOnboardingEntities(modelBuilder);
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
        }

        private void ConfigureEmployeeEntities(ModelBuilder modelBuilder)
        {
            // Employee Configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.Id);

                // Core properties
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
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.HireDate).HasDefaultValueSql("GETUTCDATE()");

                // **CRITICAL: Ignore problematic onboarding properties**
                entity.Ignore("IsOnboardingLocked");
                entity.Ignore("IsOnboardingOnTrack");
                entity.Ignore("LastOnboardingReminderDate");
                entity.Ignore("OnboardingApprovedById");
                entity.Ignore("OnboardingApprovedDate");
                entity.Ignore("OnboardingCompletionDate");
                entity.Ignore("OnboardingExpectedDate");
                entity.Ignore("OnboardingMentorId");
                entity.Ignore("OnboardingNotes");
                entity.Ignore("OnboardingPhase");
                entity.Ignore("OnboardingReminderCount");
                entity.Ignore("OnboardingStartDate");

                // Ignore computed properties
                entity.Ignore(e => e.OnboardingStatus);
                entity.Ignore(e => e.OnboardingCompletionPercentage);
                entity.Ignore(e => e.OnboardingTasksTotal);
                entity.Ignore(e => e.OnboardingTasksCompleted);
                entity.Ignore(e => e.FullName);
                entity.Ignore(e => e.DisplayName);

                // Indexes
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Relationships
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Employees)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Manager)
                      .WithMany(m => m.DirectReports)
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureDepartmentEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Departments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Code).HasMaxLength(10);
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
                entity.Property(e => e.RequiredPermission).HasMaxLength(100);
                //entity.Property(e => e.Description).HasMaxLength(500);
                //entity.Property(e => e.Target).HasMaxLength(50).HasDefaultValue("_self");
                //entity.Property(e => e.CssClass).HasMaxLength(200);
                //entity.Property(e => e.IsActive).HasDefaultValue(true);
                //entity.Property(e => e.IsVisible).HasDefaultValue(true);
                //entity.Property(e => e.IsSystemMenu).HasDefaultValue(false);
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
                //entity.Property(e => e.CanCreate).HasDefaultValue(false);
                //entity.Property(e => e.CanApprove).HasDefaultValue(false);
                //entity.Property(e => e.CanExport).HasDefaultValue(false);
                //entity.Property(e => e.AdditionalPermissions).HasMaxLength(500);
                //entity.Property(e => e.CreatedBy).HasMaxLength(100);
                //entity.Property(e => e.UpdatedBy).HasMaxLength(100);
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
                //entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                //entity.Property(e => e.ValueType).IsRequired().HasMaxLength(20);
                //entity.Property(e => e.Icon).HasMaxLength(50);
                //entity.Property(e => e.Color).HasMaxLength(20);
                //entity.Property(e => e.Category).HasMaxLength(50);
                //entity.Property(e => e.Description).HasMaxLength(500);
                //entity.Property(e => e.QuerySource).HasMaxLength(100);
                //entity.Property(e => e.IsActive).HasDefaultValue(true);
                //entity.Property(e => e.IsRealTime).HasDefaultValue(false);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                //entity.HasIndex(e => e.Title);
                //entity.HasIndex(e => e.Category);
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
                //entity.Property(e => e.Description).HasMaxLength(500);
                //entity.Property(e => e.Category).HasMaxLength(50);
                //entity.Property(e => e.ConfirmationMessage).HasMaxLength(200);
                //entity.Property(e => e.Permission).HasMaxLength(100);
                //entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                //entity.Property(e => e.RequiresConfirmation).HasDefaultValue(false);
                //entity.Property(e => e.IsSystemAction).HasDefaultValue(false);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.ActionKey).IsUnique();
                //entity.HasIndex(e => e.Category);
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
                //entity.Property(e => e.Category).HasMaxLength(50);
                //entity.Property(e => e.CreatedBy).HasMaxLength(100);
                //entity.Property(e => e.IsActive).HasDefaultValue(true);
                //entity.Property(e => e.IsSystemType).HasDefaultValue(false);
                //entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.Name);
                //entity.HasIndex(e => e.Category);
            });

            // RecentActivity Configuration
            modelBuilder.Entity<RecentActivity>(entity =>
            {
                entity.ToTable("RecentActivities");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IPAddress).HasMaxLength(45);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                      .WithMany(u => u.RecentActivities)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.RecentActivities)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.SetNull);

               

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            });
        }

        private void ConfigureLeaveEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.ToTable("LeaveRequests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
                //entity.Property(e => e.WorkflowStatus).HasMaxLength(50);
                //entity.Property(e => e.CurrentApprovalStep).HasMaxLength(100);
                //entity.Property(e => e.ReviewerComments).HasMaxLength(1000);
                //entity.Property(e => e.HRComments).HasMaxLength(1000);
                //entity.Property(e => e.AttachmentPath).HasMaxLength(200);
                //entity.Property(e => e.ActualDaysUsed).HasColumnType("decimal(3,1)");
                //entity.Property(e => e.IsEmergencyLeave).HasDefaultValue(false);
                //entity.Property(e => e.IsHalfDay).HasDefaultValue(false);
                entity.Property(e => e.RequestedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Reviewer)
                      .WithMany()
                      .HasForeignKey(e => e.ReviewedBy)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.EmployeeId, e.StartDate });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.LeaveType);
            });
        }

        private void ConfigureTimeAttendanceEntities(ModelBuilder modelBuilder)
        {
            // TimeEntry Configuration
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.ToTable("TimeEntries");
                entity.HasKey(e => e.Id);
                //entity.Property(e => e.ClockInTime).IsRequired();
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)");
                //entity.Property(e => e.BreakHours).HasColumnType("decimal(5,2)");
              //  entity.Property(e => e.OvertimeHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                //entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Location).HasMaxLength(100);
               // entity.Property(e => e.IPAddress).HasMaxLength(45);
                //entity.Property(e => e.ApprovalNotes).HasMaxLength(500);
                //entity.Property(e => e.IsManualEntry).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                //entity.HasOne(e => e.Approver)
                //      .WithMany()
                //      .HasForeignKey(e => e.ApprovedBy)
                //      .OnDelete(DeleteBehavior.SetNull);

                //entity.HasOne(e => e.TimeSheet)
                //      .WithMany(ts => ts.TimeEntries)
                //      .HasForeignKey(e => e.TimeSheetId)
                //      .OnDelete(DeleteBehavior.SetNull);

                //entity.HasIndex(e => new { e.EmployeeId, e.ClockInTime });
                entity.HasIndex(e => e.Status);
            });

            // TimeSheet Configuration
            modelBuilder.Entity<TimeSheet>(entity =>
            {
                entity.ToTable("TimeSheets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(e => e.RegularHours).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(e => e.OvertimeHours).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                //entity.Property(e => e.BreakHours).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                //entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Draft");
                //entity.Property(e => e.ApprovalComments).HasMaxLength(1000);
                //entity.Property(e => e.EmployeeComments).HasMaxLength(1000);
                //entity.Property(e => e.IsLocked).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Approver)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedBy)
                      .OnDelete(DeleteBehavior.SetNull);

                //entity.HasOne(e => e.LockedByUser)
                //      .WithMany()
                //      .HasForeignKey(e => e.LockedBy)
                //      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.EmployeeId, e.WeekStartDate }).IsUnique();
                entity.HasIndex(e => e.Status);
            });

            // Schedule Configuration
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("Schedules");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DayOfWeek).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                //entity.Property(e => e.BreakDuration).HasColumnType("decimal(3,1)");
                //entity.Property(e => e.ShiftType).HasMaxLength(50);
                //entity.Property(e => e.Location).HasMaxLength(100);
                //entity.Property(e => e.Notes).HasMaxLength(500);
                //entity.Property(e => e.RecurrenceType).HasMaxLength(20).HasDefaultValue("Weekly");
                //entity.Property(e => e.IsActive).HasDefaultValue(true);
                //entity.Property(e => e.IsRecurring).HasDefaultValue(true);
                //entity.Property(e => e.EffectiveFrom).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                //entity.HasOne(e => e.Creator)
                //      .WithMany()
                //      .HasForeignKey(e => e.CreatedBy)
                //      .OnDelete(DeleteBehavior.SetNull);

                //entity.HasOne(e => e.Updater)
                //      .WithMany()
                //      .HasForeignKey(e => e.UpdatedBy)
                //      .OnDelete(DeleteBehavior.SetNull);

                //entity.HasIndex(e => new { e.EmployeeId, e.DayOfWeek, e.EffectiveFrom });
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
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("PENDING");
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20).HasDefaultValue("MEDIUM");
                entity.Property(e => e.EstimatedTime).HasMaxLength(100);
                entity.Property(e => e.Instructions).HasMaxLength(2000);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.CompletedByRole).HasMaxLength(50);
                entity.Property(e => e.ActualTimeSpent).HasColumnType("decimal(3,1)");
                entity.Property(e => e.ApprovalNotes).HasMaxLength(500);
                entity.Property(e => e.ExternalSystemId).HasMaxLength(100);
                entity.Property(e => e.IsTemplate).HasDefaultValue(false);
                entity.Property(e => e.RequiresApproval).HasDefaultValue(false);
                entity.Property(e => e.IsApproved).HasDefaultValue(false);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.OnboardingTasks)
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

                entity.HasOne(e => e.CompletedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CompletedById)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.EmployeeId, e.Status });
                entity.HasIndex(e => e.DueDate);
                entity.HasIndex(e => e.Category);
            });

            // OnboardingTemplate Configuration
            modelBuilder.Entity<OnboardingTemplate>(entity =>
            {
                entity.ToTable("OnboardingTemplates");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ForRole).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ForDepartment).HasMaxLength(50);
                entity.Property(e => e.TemplateVersion).HasMaxLength(20).HasDefaultValue("1.0");
                entity.Property(e => e.EmployeeType).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Prerequisites).HasMaxLength(500);
                entity.Property(e => e.SuccessCriteria).HasMaxLength(500);
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.ExternalSystemId).HasMaxLength(100);
                entity.Property(e => e.EstimatedHoursTotal).HasColumnType("decimal(5,2)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.RequiresManagerApproval).HasDefaultValue(false);
                entity.Property(e => e.AutoAssign).HasDefaultValue(false);
                entity.Property(e => e.IsSystemTemplate).HasDefaultValue(false);
                entity.Property(e => e.Priority).HasDefaultValue(1);
                entity.Property(e => e.UsageCount).HasDefaultValue(0);
                entity.Property(e => e.EstimatedDaysToComplete).HasDefaultValue(14);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedBy)
                      .WithMany()
                      .HasForeignKey(e => e.ModifiedById)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.ForRole);
                entity.HasIndex(e => e.ForDepartment);
                entity.HasIndex(e => e.IsActive);
            });

            // OnboardingProgress Configuration
            modelBuilder.Entity<OnboardingProgress>(entity =>
            {
                entity.ToTable("OnboardingProgress");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("NOT_STARTED");
                entity.Property(e => e.CompletionPercentage).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(e => e.EstimatedHoursTotal).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ActualHoursSpent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CurrentPhase).HasMaxLength(100);
                entity.Property(e => e.TotalTasks).HasDefaultValue(0);
                entity.Property(e => e.CompletedTasks).HasDefaultValue(0);
                entity.Property(e => e.PendingTasks).HasDefaultValue(0);
                entity.Property(e => e.OverdueTasks).HasDefaultValue(0);
                entity.Property(e => e.InProgressTasks).HasDefaultValue(0);
                entity.Property(e => e.TasksRequiringApproval).HasDefaultValue(0);
                entity.Property(e => e.TasksApproved).HasDefaultValue(0);
                entity.Property(e => e.DocumentsRequired).HasDefaultValue(0);
                entity.Property(e => e.DocumentsUploaded).HasDefaultValue(0);
                entity.Property(e => e.IsOnTrack).HasDefaultValue(true);
                entity.Property(e => e.StartDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.HasIndex(e => e.Status);
            });

            // OnboardingChecklist Configuration - WITH EXPLICIT RELATIONSHIPS
            modelBuilder.Entity<OnboardingChecklist>(entity =>
            {
                entity.ToTable("OnboardingChecklists");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("ASSIGNED");
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(50).HasDefaultValue("MEDIUM");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CancellationReason).HasMaxLength(500);
                entity.Property(e => e.ExternalSystemId).HasMaxLength(100);
                entity.Property(e => e.ApprovalNotes).HasMaxLength(500);
                entity.Property(e => e.EstimatedHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ActualHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.RequiresManagerApproval).HasDefaultValue(false);
                entity.Property(e => e.IsApproved).HasDefaultValue(false);
                entity.Property(e => e.ReminderCount).HasDefaultValue(0);
                entity.Property(e => e.AssignedDate).HasDefaultValueSql("GETUTCDATE()");

                // **EXPLICIT RELATIONSHIP CONFIGURATION**
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.OnboardingChecklists)
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

                entity.HasOne(e => e.CancelledBy)
                      .WithMany()
                      .HasForeignKey(e => e.CancelledById)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ApprovedBy)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedById)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.EmployeeId, e.TemplateId }).IsUnique();
                entity.HasIndex(e => e.Status);
            });

            // OnboardingDocument Configuration
            modelBuilder.Entity<OnboardingDocument>(entity =>
            {
                entity.ToTable("OnboardingDocuments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FilePath).HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(100);
                entity.Property(e => e.ContentType).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Instructions).HasMaxLength(1000);
                entity.Property(e => e.ApprovalNotes).HasMaxLength(500);
                entity.Property(e => e.RejectionReason).HasMaxLength(500);
                entity.Property(e => e.FileHash).HasMaxLength(32);
                entity.Property(e => e.VirusScanResult).HasMaxLength(500);
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.ExternalSystemId).HasMaxLength(100);
                entity.Property(e => e.Required).HasDefaultValue(false);
                entity.Property(e => e.Uploaded).HasDefaultValue(false);
                entity.Property(e => e.IsApproved).HasDefaultValue(false);
                entity.Property(e => e.IsRejected).HasDefaultValue(false);
                entity.Property(e => e.IsVirusScanRequired).HasDefaultValue(false);
                entity.Property(e => e.IsVirusScanPassed).HasDefaultValue(false);
                entity.Property(e => e.IsConfidential).HasDefaultValue(false);
                entity.Property(e => e.Version).HasDefaultValue(1);
                entity.Property(e => e.AccessCount).HasDefaultValue(0);

                entity.HasOne(e => e.Task)
                      .WithMany()
                      .HasForeignKey(e => e.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UploadedBy)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedById)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ApprovedBy)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedById)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.TaskId);
                entity.HasIndex(e => e.DocumentType);
                entity.HasIndex(e => e.FileHash);
            });
        }
    }
}