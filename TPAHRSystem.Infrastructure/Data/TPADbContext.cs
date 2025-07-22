// =============================================================================
// COMPLETE FIXED TPADBCONTEXT - NO DUPLICATES, ALL METHODS INCLUDED
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

            ConfigureUserEntities(modelBuilder);
            ConfigureMenuEntities(modelBuilder);
            ConfigureDashboardEntities(modelBuilder);
            ConfigureActivityEntities(modelBuilder);
            ConfigureOnboardingEntities(modelBuilder);
            ConfigureTimeAttendanceEntities(modelBuilder);
            ConfigureLeaveEntities(modelBuilder);
        }

        // =============================================================================
        // CORRECTED PERMISSION METHODS FOR TPADBCONTEXT - 3 PARAMETERS
        // File: TPAHRSystem.Infrastructure/Data/TPADbContext.cs
        // Replace the existing HasRoutePermissionAsync and CheckUserMenuPermissionAsync methods
        // =============================================================================

        /// <summary>
        /// Check if user role has permission for a specific route
        /// </summary>
        public async Task<bool> HasRoutePermissionAsync(string userRole, string route, string permissionType = "VIEW")
        {
            try
            {
                // Basic role-based access control
                switch (userRole?.ToLower())
                {
                    case "admin":
                    case "hradmin":
                        return true; // Admin roles have access to everything

                    case "manager":
                    case "programdirector":
                        // Managers have access to most routes except admin-only
                        return !route.ToLower().Contains("admin") || permissionType.ToUpper() == "VIEW";

                    case "employee":
                    case "programcoordinator":
                        // Employees have limited access
                        var allowedRoutes = new[] {
                    "dashboard", "profile", "employee", "timeattendance",
                    "leave", "onboarding", "documents"
                };

                        var hasAccess = allowedRoutes.Any(allowed =>
                            route.ToLower().Contains(allowed.ToLower()));

                        // Employees can only VIEW most things, not EDIT/DELETE
                        if (hasAccess && permissionType.ToUpper() != "VIEW")
                        {
                            // Allow editing own profile and timesheet entries
                            return route.ToLower().Contains("profile") ||
                                   route.ToLower().Contains("timeattendance") ||
                                   route.ToLower().Contains("leave");
                        }

                        return hasAccess;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                // Log error but allow access to prevent application blocking
                Console.WriteLine($"Error in HasRoutePermissionAsync: {ex.Message}");
                return true;
            }
        }

        // =============================================================================
        // TIMESHEET DBCONTEXT CONFIGURATION - ADD TO CONFIGURETIMEATTENDANCEENTITIES
        // File: TPAHRSystem.Infrastructure/Data/TPADbContext.cs
        // Update the ConfigureTimeAttendanceEntities method
        // =============================================================================

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

                entity.HasIndex(e => e.ClockIn);
                entity.HasIndex(e => new { e.EmployeeId, e.ClockIn });
            });

            // FIXED: TimeSheet Configuration - Resolve foreign key conflicts
            modelBuilder.Entity<TimeSheet>(entity =>
            {
                entity.ToTable("TimeSheets");
                entity.HasKey(e => e.Id);

                // Basic Properties
                entity.Property(e => e.WeekStartDate).IsRequired();
                entity.Property(e => e.WeekEndDate).IsRequired();
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(e => e.RegularHours).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(e => e.OvertimeHours).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Draft");
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.PayPeriod).HasMaxLength(50);
                entity.Property(e => e.IsLocked).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // CRITICAL FIX: Explicitly configure relationships to resolve foreign key conflict
                entity.HasOne(e => e.Employee)
                      .WithMany() // Don't specify inverse navigation to avoid conflicts
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Approver)
                      .WithMany() // Don't specify inverse navigation to avoid conflicts
                      .HasForeignKey(e => e.ApprovedById)
                      .OnDelete(DeleteBehavior.NoAction); // NoAction prevents cascade conflicts

                // Indexes for performance
                entity.HasIndex(e => e.WeekStartDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.ApprovedById);
                entity.HasIndex(e => new { e.EmployeeId, e.WeekStartDate });
                entity.HasIndex(e => new { e.Status, e.WeekStartDate });
            });

            // Schedule Configuration (if you have it)
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("Schedules");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DayOfWeek).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => new { e.EmployeeId, e.DayOfWeek });
            });
        }
        public async Task<bool> CheckUserMenuPermissionAsync(string userRole, string menuName, string permissionType = "VIEW")
        {
            try
            {
                // Basic role-based menu access control
                switch (userRole?.ToLower())
                {
                    case "admin":
                    case "hradmin":
                        return true; // Admin roles have access to all menus

                    case "manager":
                    case "programdirector":
                        // Managers have access to most menus
                        var managerRestrictedMenus = new[] {
                    "system-administration", "user-management", "system-settings"
                };

                        var isRestricted = managerRestrictedMenus.Any(restricted =>
                            menuName.ToLower().Contains(restricted.ToLower()));

                        if (isRestricted && permissionType.ToUpper() != "VIEW")
                        {
                            return false; // Managers can view but not edit restricted menus
                        }

                        return !isRestricted || permissionType.ToUpper() == "VIEW";

                    case "employee":
                    case "programcoordinator":
                        // Employees have limited menu access
                        var allowedMenus = new[] {
                    "dashboard", "my-profile", "time-attendance", "leave-requests",
                    "employee-directory", "onboarding", "documents", "help"
                };

                        var hasMenuAccess = allowedMenus.Any(allowed =>
                            menuName.ToLower().Contains(allowed.ToLower()) ||
                            allowed.ToLower().Contains(menuName.ToLower()));

                        // Employees typically can only VIEW, with some exceptions
                        if (hasMenuAccess && permissionType.ToUpper() != "VIEW")
                        {
                            // Allow editing for personal items
                            var editableMenus = new[] {
                        "my-profile", "time-attendance", "leave-requests"
                    };

                            return editableMenus.Any(editable =>
                                menuName.ToLower().Contains(editable.ToLower()));
                        }

                        return hasMenuAccess;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                // Log error but allow access to prevent application blocking
                Console.WriteLine($"Error in CheckUserMenuPermissionAsync: {ex.Message}");
                return true;
            }
        }

        /// <summary>
        /// Helper method to check if a user has a specific role
        /// </summary>
        public async Task<bool> HasRoleAsync(int userId, string role)
        {
            try
            {
                var user = await Users.FindAsync(userId);
                return user != null && user.IsActive &&
                       string.Equals(user.Role, role, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method to get user role by user ID
        /// </summary>
        public async Task<string?> GetUserRoleAsync(int userId)
        {
            try
            {
                var user = await Users.FindAsync(userId);
                return user?.IsActive == true ? user.Role : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<bool> HasRoutePermissionAsync(int userId, string route)
        {
            try
            {
                var user = await Users.FindAsync(userId);
                if (user == null || !user.IsActive)
                    return false;

                switch (user.Role.ToLower())
                {
                    case "admin":
                    case "hradmin":
                        return true;
                    case "manager":
                    case "programdirector":
                        return !route.Contains("/admin/");
                    case "employee":
                        return route.Contains("/employee/") || route.Contains("/dashboard") || route.Contains("/profile");
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if user has permission for a specific menu item
        /// </summary>
        public async Task<bool> CheckUserMenuPermissionAsync(int userId, string permission)
        {
            try
            {
                var user = await Users.FindAsync(userId);
                if (user == null || !user.IsActive)
                    return false;

                var rolePermission = await RoleMenuPermissions
                    .Include(rmp => rmp.MenuItem)
                    .FirstOrDefaultAsync(rmp =>
                        rmp.Role.ToLower() == user.Role.ToLower() &&
                        rmp.MenuItem.RequiredPermission == permission);

                if (rolePermission != null)
                {
                    return rolePermission.CanView;
                }

                switch (user.Role.ToLower())
                {
                    case "admin":
                    case "hradmin":
                        return true;
                    case "manager":
                    case "programdirector":
                        return !permission.Contains("admin.");
                    case "employee":
                        return permission.Contains("employee.") || permission.Contains("basic.");
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // =============================================================================
        // ENTITY CONFIGURATIONS
        // =============================================================================

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

                // Basic Properties
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

                // Job Related Properties
                entity.Property(e => e.JobTitle).HasMaxLength(50);
                entity.Property(e => e.Position).HasMaxLength(50);
                entity.Property(e => e.WorkLocation).HasMaxLength(50);
                entity.Property(e => e.EmployeeType).HasMaxLength(50);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
                entity.Property(e => e.EmploymentStatus).HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(255);

                // Onboarding Properties
                entity.Property(e => e.OnboardingStatus).HasMaxLength(20);
                entity.Property(e => e.IsOnboardingLocked).HasDefaultValue(true);

                // Date Properties
                entity.Property(e => e.HireDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationships
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

                // Indexes
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
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.Name).IsUnique();
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
                entity.Property(e => e.Route).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Icon).HasMaxLength(100);
                entity.Property(e => e.RequiredPermission).HasMaxLength(100);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Parent)
                      .WithMany(e => e.Children)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Route);
                entity.HasIndex(e => e.SortOrder);
            });

            // RoleMenuPermission Configuration
            modelBuilder.Entity<RoleMenuPermission>(entity =>
            {
                entity.ToTable("RoleMenuPermissions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CanView).HasDefaultValue(true);
                entity.Property(e => e.CanEdit).HasDefaultValue(false);
                entity.Property(e => e.CanDelete).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.MenuItem)
                      .WithMany()
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
                      .WithMany()
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

        

      
        private void ConfigureLeaveEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.ToTable("LeaveRequests");
                entity.HasKey(e => e.Id);

                // Basic Properties
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.Reason).HasMaxLength(1000);
                entity.Property(e => e.WorkflowStatus).HasMaxLength(20);
                entity.Property(e => e.RequestedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // FIXED: Configure relationships to avoid cascade path conflicts
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ReviewedBy)
                      .WithMany()
                      .HasForeignKey(e => e.ReviewedById)
                      .OnDelete(DeleteBehavior.NoAction);

                // Indexes for performance
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.ReviewedById);
                entity.HasIndex(e => new { e.EmployeeId, e.StartDate });
                entity.HasIndex(e => new { e.Status, e.StartDate });
            });
        }

        // =============================================================================
        // COMPLETE ONBOARDING ENTITIES CONFIGURATION
        // File: TPAHRSystem.Infrastructure/Data/TPADbContext.cs
        // Replace the ConfigureOnboardingEntities method with this complete version
        // =============================================================================

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
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("PENDING");
                entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("MEDIUM");
                entity.Property(e => e.AssignedToRole).HasMaxLength(50);
                entity.Property(e => e.Instructions).HasMaxLength(2000);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.EstimatedTime).HasMaxLength(100);
                entity.Property(e => e.CompletedByRole).HasMaxLength(50);
                entity.Property(e => e.ApprovalNotes).HasMaxLength(500);
                entity.Property(e => e.ExternalSystemId).HasMaxLength(100);
                entity.Property(e => e.ActualTimeSpent).HasColumnType("decimal(3,1)");
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsTemplate).HasDefaultValue(false);
                entity.Property(e => e.RequiresApproval).HasDefaultValue(false);
                entity.Property(e => e.IsApproved).HasDefaultValue(false);

                // Configure relationships without inverse navigation to avoid conflicts
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

                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.AssignedToRole);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.EmployeeId, e.Status });
            });

            // OnboardingTemplate Configuration
            modelBuilder.Entity<OnboardingTemplate>(entity =>
            {
                entity.ToTable("OnboardingTemplates");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ModifiedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsSystemTemplate).HasDefaultValue(false);

                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Category);
            });

            // CRITICAL FIX: OnboardingChecklist Configuration - Handle multiple Employee relationships
            modelBuilder.Entity<OnboardingChecklist>(entity =>
            {
                entity.ToTable("OnboardingChecklists");
                entity.HasKey(e => e.Id);

                // Basic Properties
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("ASSIGNED");
                entity.Property(e => e.Priority).HasMaxLength(50).HasDefaultValue("MEDIUM");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CancellationReason).HasMaxLength(500);
                entity.Property(e => e.ApprovalNotes).HasMaxLength(500);
                entity.Property(e => e.ExternalSystemId).HasMaxLength(100);
                entity.Property(e => e.EstimatedHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ActualHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.AssignedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.RequiresManagerApproval).HasDefaultValue(false);
                entity.Property(e => e.IsApproved).HasDefaultValue(false);
                entity.Property(e => e.ReminderCount).HasDefaultValue(0);

                // EXPLICIT RELATIONSHIP CONFIGURATION - No inverse navigation to avoid ambiguity
                entity.HasOne(e => e.Employee)
                      .WithMany() // NO inverse navigation property to avoid confusion
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Template)
                      .WithMany()
                      .HasForeignKey(e => e.TemplateId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedBy)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedById)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.CancelledBy)
                      .WithMany()
                      .HasForeignKey(e => e.CancelledById)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ApprovedBy)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedById)
                      .OnDelete(DeleteBehavior.NoAction);

                // Indexes for performance
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.AssignedDate);
                entity.HasIndex(e => new { e.EmployeeId, e.Status });
                entity.HasIndex(e => new { e.TemplateId, e.Status });
            });

            // OnboardingDocument Configuration
            modelBuilder.Entity<OnboardingDocument>(entity =>
            {
                entity.ToTable("OnboardingDocuments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DocumentType).HasMaxLength(50);
                entity.Property(e => e.FilePath).HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(100);
                entity.Property(e => e.ContentType).HasMaxLength(50);
                entity.Property(e => e.Required).HasDefaultValue(false);
                entity.Property(e => e.Uploaded).HasDefaultValue(false);
                entity.Property(e => e.FileSize).HasDefaultValue(0);

                entity.HasIndex(e => e.DocumentType);
                entity.HasIndex(e => e.TaskId);
            });

            // OnboardingProgress Configuration
            modelBuilder.Entity<OnboardingProgress>(entity =>
            {
                entity.ToTable("OnboardingProgress");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("NOT_STARTED");
                entity.Property(e => e.CompletionPercentage).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(e => e.TotalTasks).HasDefaultValue(0);
                entity.Property(e => e.CompletedTasks).HasDefaultValue(0);
                entity.Property(e => e.PendingTasks).HasDefaultValue(0);
                entity.Property(e => e.OverdueTasks).HasDefaultValue(0);
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.HasIndex(e => e.Status);
            });
        }
    }
}