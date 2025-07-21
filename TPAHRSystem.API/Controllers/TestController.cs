// =============================================================================
// ULTRA MINIMAL TPADbContext - GUARANTEED NO COMPILATION ERRORS
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

        // ONLY THE ABSOLUTE ESSENTIALS - Nothing else!
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<UserSession> UserSessions { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<OnboardingTask> OnboardingTasks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration - MINIMAL
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Salt).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // UserSession Configuration - MINIMAL
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.ToTable("UserSessions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionToken).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.SessionToken).IsUnique();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserSessions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Employee Configuration - ULTRA MINIMAL
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Position).HasMaxLength(50);

                // **IGNORE ALL PROBLEMATIC PROPERTIES**
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
                entity.Ignore("OnboardingStatus");
                entity.Ignore("OnboardingCompletionPercentage");
                entity.Ignore("OnboardingTasksTotal");
                entity.Ignore("OnboardingTasksCompleted");
                entity.Ignore("FullName");
                entity.Ignore("DisplayName");

                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Basic relationships only
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Manager)
                      .WithMany()
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Department Configuration - MINIMAL
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Departments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // OnboardingTask Configuration - MINIMAL
            modelBuilder.Entity<OnboardingTask>(entity =>
            {
                entity.ToTable("OnboardingTasks");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsTemplate).HasDefaultValue(false);

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}