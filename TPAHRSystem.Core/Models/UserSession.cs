﻿using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class UserSession
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public string SessionToken { get; set; } = string.Empty;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; } = null!;
    }
}