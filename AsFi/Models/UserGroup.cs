using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("user_groups")]
    public class UserGroup
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Group Group { get; set; } = null!;
    }
}