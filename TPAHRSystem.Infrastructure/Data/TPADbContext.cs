// =============================================================================
// UPDATED TPADBCONTEXT - UNCOMMENT REQUIRED DBSETS
// File: TPAHRSystem.Infrastructure/Data/TPADbContext.cs (Replace existing)
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

        // UNCOMMENTED - Required DbSets for Dashboard functionality
        public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;
        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;
        public DbSet<QuickAction> QuickActions { get; set; } = null!;
        public DbSet<ActivityType> ActivityTypes { get; set; } = null!;
        public DbSet<RecentActivity> RecentActivities { get; set; } = null!;
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            // Employee Configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();

                // User relationship
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Employees)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Department relationship
                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Manager relationship
                entity.HasOne(e => e.Manager)
                      .WithMany(e => e.DirectReports)
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.SetNull);
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

            // DashboardStat Configuration
            modelBuilder.Entity<DashboardStat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StatKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StatName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StatValue).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.StatKey).IsUnique();
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

            // RecentActivity Configuration
            modelBuilder.Entity<RecentActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Details).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.IPAddress).IsRequired().HasMaxLength(50);

                // User relationship
                entity.HasOne(e => e.User)
                      .WithMany(u => u.RecentActivities)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Employee relationship
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.SetNull);

                // ActivityType relationship
                entity.HasOne(e => e.ActivityType)
                      .WithMany(at => at.RecentActivities)
                      .HasForeignKey(e => e.ActivityTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // LeaveRequest Configuration
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasDefaultValue("Pending").HasMaxLength(20);

                // Employee relationship
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Reviewer relationship
                entity.HasOne(e => e.Reviewer)
                      .WithMany()
                      .HasForeignKey(e => e.ReviewedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // TimeEntry Configuration
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasDefaultValue("Active").HasMaxLength(20);
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)");

                // Employee relationship
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // MenuItem Configuration
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Route).IsRequired().HasMaxLength(255);

                // Self-referencing relationship
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
                entity.HasIndex(e => new { e.Role, e.MenuItemId }).IsUnique();

                // MenuItem relationship
                entity.HasOne(e => e.MenuItem)
                      .WithMany(m => m.RolePermissions)
                      .HasForeignKey(e => e.MenuItemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}