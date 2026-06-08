using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("last_name")]
        public string LastName { get; set; } = null!;

        [Column("first_name")]
        public string FirstName { get; set; } = null!;

        [Column("patronymic")]
        public string Patronymic { get; set; } = null!;

        [Column("login")]
        public string Login { get; set; } = null!;

        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("role")]
        public string Role { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
        public virtual ICollection<GroupJoinCode> GroupJoinCodes { get; set; } = new List<GroupJoinCode>();
        public virtual ICollection<TestAttemptResult> TestAttemptResults { get; set; } = new List<TestAttemptResult>();
    }
}