using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("user_preferences")]
    public class UserPreference
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("theme")]
        public string Theme { get; set; } = "dark";

        [Column("primary_color")]
        public string PrimaryColor { get; set; } = "purple";

        public virtual User User { get; set; } = null!;
    }
}