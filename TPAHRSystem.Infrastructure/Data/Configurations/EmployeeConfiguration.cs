// =============================================================================
// EMPLOYEE ENTITY CONFIGURATION - EXPLICIT PROPERTY MAPPING
// File: TPAHRSystem.Infrastructure/Data/Configurations/EmployeeConfiguration.cs (NEW FILE)
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TPAHRSystem.Core.Models;

namespace TPAHRSystem.Infrastructure.Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties - ONLY map what actually exists in database
            builder.Property(e => e.Id).HasColumnName("Id");
            builder.Property(e => e.EmployeeNumber).HasColumnName("EmployeeNumber").HasMaxLength(20).IsRequired();
            builder.Property(e => e.FirstName).HasColumnName("FirstName").HasMaxLength(100).IsRequired();
            builder.Property(e => e.LastName).HasColumnName("LastName").HasMaxLength(100).IsRequired();
            builder.Property(e => e.Email).HasColumnName("Email").HasMaxLength(255).IsRequired();
            builder.Property(e => e.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(20);
            builder.Property(e => e.DateOfBirth).HasColumnName("DateOfBirth");
            builder.Property(e => e.Gender).HasColumnName("Gender").HasMaxLength(10);
            builder.Property(e => e.Address).HasColumnName("Address").HasMaxLength(500);
            builder.Property(e => e.City).HasColumnName("City").HasMaxLength(100);
            builder.Property(e => e.State).HasColumnName("State").HasMaxLength(50);
            builder.Property(e => e.ZipCode).HasColumnName("ZipCode").HasMaxLength(20);
            builder.Property(e => e.HireDate).HasColumnName("HireDate");
            builder.Property(e => e.TerminationDate).HasColumnName("TerminationDate");
            builder.Property(e => e.JobTitle).HasColumnName("JobTitle").HasMaxLength(50);
            builder.Property(e => e.Position).HasColumnName("Position").HasMaxLength(50);
            builder.Property(e => e.WorkLocation).HasColumnName("WorkLocation").HasMaxLength(50);
            builder.Property(e => e.Salary).HasColumnName("Salary").HasColumnType("decimal(18,2)");
            builder.Property(e => e.EmploymentStatus).HasColumnName("EmploymentStatus").HasMaxLength(20);
            builder.Property(e => e.Status).HasColumnName("Status").HasMaxLength(20);
            builder.Property(e => e.IsActive).HasColumnName("IsActive");
            builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
            builder.Property(e => e.OnboardingCompletedDate).HasColumnName("OnboardingCompletedDate");

            // Foreign Keys
            builder.Property(e => e.UserId).HasColumnName("UserId");
            builder.Property(e => e.DepartmentId).HasColumnName("DepartmentId");
            builder.Property(e => e.ManagerId).HasColumnName("ManagerId");

            // **CRITICAL: Explicitly ignore computed properties that don't exist in database**
            builder.Ignore(e => e.OnboardingStatus);
            builder.Ignore(e => e.OnboardingCompletionPercentage);
            builder.Ignore(e => e.OnboardingTasksTotal);
            builder.Ignore(e => e.OnboardingTasksCompleted);
            builder.Ignore(e => e.FullName);
            builder.Ignore(e => e.DisplayName);

            // **CRITICAL: Ignore all the properties that were causing the error**
            builder.Ignore("IsOnboardingLocked");
            builder.Ignore("IsOnboardingOnTrack");
            builder.Ignore("LastOnboardingReminderDate");
            builder.Ignore("OnboardingApprovedById");
            builder.Ignore("OnboardingApprovedDate");
            builder.Ignore("OnboardingCompletionDate");
            builder.Ignore("OnboardingExpectedDate");
            builder.Ignore("OnboardingMentorId");
            builder.Ignore("OnboardingNotes");
            builder.Ignore("OnboardingPhase");
            builder.Ignore("OnboardingReminderCount");
            builder.Ignore("OnboardingStartDate");

            // Relationships
            builder.HasOne(e => e.User)
                .WithMany(u => u.Employees)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.Manager)
                .WithMany(m => m.DirectReports)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.OnboardingTasks)
                .WithOne(t => t.Employee)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

          

            // Indexes
            builder.HasIndex(e => e.EmployeeNumber).IsUnique();
            builder.HasIndex(e => e.Email).IsUnique();
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.DepartmentId);
            builder.HasIndex(e => e.ManagerId);
        }
    }
}