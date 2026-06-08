using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("topics")]
    public class Topic
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Column("section_id")]
        public int SectionId { get; set; }

        public virtual Subject Subject { get; set; } = null!;
        public virtual Section Section { get; set; } = null!;
        public virtual ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();
        public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
    }
}