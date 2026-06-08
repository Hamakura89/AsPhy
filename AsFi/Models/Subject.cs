using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("subjects")]
    public class Subject
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
    }
}