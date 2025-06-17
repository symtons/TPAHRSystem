// TPAHRSystem.Infrastructure/Data/TPADbContext.cs
using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Core.Models;

namespace TPAHRSystem.Infrastructure.Data
{
    public class TPADbContext : DbContext
    {
        public TPADbContext(DbContextOptions<TPADbContext> options) : base(options)
        {
        }

        // Main Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        // Menu Management
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; }

        // Dashboard Data
        public DbSet<DashboardStat> DashboardStats { get; set; }
        public DbSet<QuickAction> QuickActions { get; set; }

        // Activity Tracking
        public DbSet<ActivityType> ActivityTypes { get; set; }
        public DbSet<RecentActivity> RecentActivities { get; set; }

        // HR Data (Basic for dashboard)
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Salt).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // UserSession Configuration
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SessionToken).IsUnique();
                entity.Property(e => e.SessionToken).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IPAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserSessions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Employee Configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.JobTitle).HasMaxLength(100);
                entity.Property(e => e.EmployeeType).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(e => e.User)
                    .WithOne(u => u.Employee)
                    .HasForeignKey<Employee>(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Manager)
                    .WithMany(e => e.DirectReports)
                    .HasForeignKey(e => e.ManagerId)
                    .OnDelete(DeleteBehavior.NoAction);  // Changed from Restrict to NoAction
            });

            // Department Configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // MenuItem Configuration
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Route).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Icon).HasMaxLength(100);
                entity.Property(e => e.RequiredPermission).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Parent)
                    .WithMany(e => e.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RoleMenuPermission Configuration
            modelBuilder.Entity<RoleMenuPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.MenuItem)
                    .WithMany(m => m.RolePermissions)
                    .HasForeignKey(e => e.MenuItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.Role, e.MenuItemId }).IsUnique();
            });

            // DashboardStat Configuration
            modelBuilder.Entity<DashboardStat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StatKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StatName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StatValue).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StatColor).HasMaxLength(50).HasDefaultValue("primary");
                entity.Property(e => e.IconName).HasMaxLength(100);
                entity.Property(e => e.Subtitle).HasMaxLength(200);
                entity.Property(e => e.ApplicableRoles).HasMaxLength(500);
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // QuickAction Configuration
            modelBuilder.Entity<QuickAction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ActionKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IconName).HasMaxLength(100);
                entity.Property(e => e.Route).HasMaxLength(255);
                entity.Property(e => e.Color).HasMaxLength(50).HasDefaultValue("#1976d2");
                entity.Property(e => e.ApplicableRoles).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // ActivityType Configuration
            modelBuilder.Entity<ActivityType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.IconName).HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(50).HasDefaultValue("#1976d2");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // RecentActivity Configuration
            modelBuilder.Entity<RecentActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EntityType).HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IPAddress).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RecentActivities)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Employee)
                    .WithMany(e => e.RecentActivities)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ActivityType)
                    .WithMany(at => at.RecentActivities)
                    .HasForeignKey(e => e.ActivityTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => e.CreatedAt);
            });

            // LeaveRequest Configuration
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StartDate).HasColumnType("date");
                entity.Property(e => e.EndDate).HasColumnType("date");
                entity.Property(e => e.Reason).HasMaxLength(1000);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.RequestedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                    .WithMany(emp => emp.LeaveRequests)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ReviewedByEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewedBy)
                    .OnDelete(DeleteBehavior.NoAction);  // Changed from SetNull to NoAction

                entity.HasIndex(e => new { e.EmployeeId, e.Status });
            });

            // TimeEntry Configuration
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.Location).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Employee)
                    .WithMany(emp => emp.TimeEntries)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.EmployeeId, e.ClockIn });
            });
        }
    }
}