using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("group_join_codes")]
    public class GroupJoinCode
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; } = null!;

        [Column("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Group Group { get; set; } = null!;
    }
}