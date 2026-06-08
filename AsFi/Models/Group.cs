using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("group")]
    public class Group
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
        public virtual ICollection<AvailableLecture> AvailableLectures { get; set; } = new List<AvailableLecture>();
        public virtual ICollection<AvailableTest> AvailableTests { get; set; } = new List<AvailableTest>();
        public virtual ICollection<GroupJoinCode> GroupJoinCodes { get; set; } = new List<GroupJoinCode>();
    }
}