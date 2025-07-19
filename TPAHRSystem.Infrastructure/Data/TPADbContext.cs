// =============================================================================
// MENU-ONLY TPADbContext - COMPLETELY SKIPS ONBOARDING CONFIGURATION
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

        // Essential DbSets for Authentication and Menu System ONLY
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<UserSession> UserSessions { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; } = null!;

        // Other essential DbSets (minimal)
        public DbSet<DashboardStat> DashboardStats { get; set; } = null!;
        public DbSet<QuickAction> QuickActions { get; set; } = null!;
        public DbSet<ActivityType> ActivityTypes { get; set; } = null!;
        public DbSet<RecentActivity> RecentActivities { get; set; } = null!;
        public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;

        // Onboarding DbSets - NO CONFIGURATION
        public DbSet<OnboardingTask> OnboardingTasks { get; set; } = null!;
        public DbSet<OnboardingTemplate> OnboardingTemplates { get; set; } = null!;
        public DbSet<OnboardingDocument> OnboardingDocuments { get; set; } = null!;
        public DbSet<OnboardingProgress> OnboardingProgress { get; set; } = null!;
        public DbSet<OnboardingChecklist> OnboardingChecklists { get; set; } = null!;

        // Time and Attendance DbSets - WITH EXPLICIT CONFIGURATION
        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;
        public DbSet<TimeSheet> TimeSheets { get; set; } = null!;
        public DbSet<Schedule> Schedules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ONLY User/Employee/Menu entities
            ConfigureUserEntities(modelBuilder);
            ConfigureMenuEntities(modelBuilder);
            ConfigureTimeAttendanceEntities(modelBuilder); // Add this to resolve foreign key conflicts

            // SKIP ALL OTHER ENTITY CONFIGURATION TO AVOID PROPERTY ERRORS
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

            // Employee Configuration - ONLY essential properties
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);

                // Basic relationships
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
            // TimeSheet Configuration - EXPLICIT FOREIGN KEY CONFIGURATION
            modelBuilder.Entity<TimeSheet>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Configure the Employee relationship explicitly
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure the Approver relationship explicitly with different foreign key
                entity.HasOne(e => e.Approver)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // TimeEntry Configuration
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Schedule Configuration
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // LeaveRequest Configuration - EXPLICIT FOREIGN KEY CONFIGURATION
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Configure the Employee relationship explicitly
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure the Reviewer relationship explicitly with different foreign key
                entity.HasOne(e => e.Reviewer)
                      .WithMany()
                      .HasForeignKey(e => e.ReviewedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private void ConfigureMenuEntities(ModelBuilder modelBuilder)
        {
            // MenuItem Configuration
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Route).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Icon).HasMaxLength(100);
                entity.Property(e => e.RequiredPermission).HasMaxLength(100);

                // Self-referencing relationship
                entity.HasOne(e => e.Parent)
                      .WithMany(e => e.Children)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Route).IsUnique();
                entity.HasIndex(e => new { e.ParentId, e.SortOrder });
            });

            // RoleMenuPermission Configuration
            modelBuilder.Entity<RoleMenuPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.MenuItem)
                      .WithMany(m => m.RolePermissions)
                      .HasForeignKey(e => e.MenuItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.Role, e.MenuItemId }).IsUnique();
            });
        }

        // =============================================================================
        // MENU SYSTEM HELPER METHODS
        // =============================================================================

        public async Task<List<UserMenuItemResult>> GetUserMenuItemsAsync(string userRole, int? departmentId = null)
        {
            try
            {
                var menuItems = await MenuItems
                    .Include(m => m.RolePermissions)
                    .Include(m => m.Parent)
                    .Where(m => m.IsActive &&
                               m.RolePermissions.Any(rp => rp.Role == userRole && rp.CanView))
                    .OrderBy(m => m.SortOrder)
                    .Select(m => new UserMenuItemResult
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Route = m.Route,
                        Icon = m.Icon,
                        ParentId = m.ParentId,
                        SortOrder = m.SortOrder,
                        IsActive = m.IsActive,
                        RequiredPermission = m.RequiredPermission,
                        CanView = m.RolePermissions.First(rp => rp.Role == userRole).CanView,
                        CanEdit = m.RolePermissions.First(rp => rp.Role == userRole).CanEdit,
                        CanDelete = m.RolePermissions.First(rp => rp.Role == userRole).CanDelete,
                        IsChild = m.ParentId.HasValue ? 1 : 0,
                        ParentName = m.Parent != null ? m.Parent.Name : null
                    })
                    .ToListAsync();

                return menuItems;
            }
            catch (Exception)
            {
                return new List<UserMenuItemResult>();
            }
        }

        public async Task<bool> CheckUserMenuPermissionAsync(string userRole, string menuName, string permissionType)
        {
            try
            {
                var menuItem = await MenuItems
                    .Include(m => m.RolePermissions)
                    .FirstOrDefaultAsync(m => m.Name == menuName && m.IsActive);

                if (menuItem == null) return false;

                var permission = menuItem.RolePermissions
                    .FirstOrDefault(rp => rp.Role == userRole);

                if (permission == null) return false;

                return permissionType.ToUpper() switch
                {
                    "VIEW" => permission.CanView,
                    "EDIT" => permission.CanEdit,
                    "DELETE" => permission.CanDelete,
                    _ => false
                };
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> HasRoutePermissionAsync(string userRole, string route, string permissionType = "VIEW")
        {
            try
            {
                var menuItem = await MenuItems
                    .Include(m => m.RolePermissions)
                    .FirstOrDefaultAsync(m => m.Route == route && m.IsActive);

                if (menuItem == null) return true;

                var permission = menuItem.RolePermissions
                    .FirstOrDefault(rp => rp.Role == userRole);

                if (permission == null) return false;

                return permissionType.ToUpper() switch
                {
                    "VIEW" => permission.CanView,
                    "EDIT" => permission.CanEdit,
                    "DELETE" => permission.CanDelete,
                    _ => false
                };
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    // DTO for menu results
    public class UserMenuItemResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string? RequiredPermission { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public int IsChild { get; set; }
        public string? ParentName { get; set; }
    }
}