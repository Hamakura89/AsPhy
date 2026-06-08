using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("lecture_types")]
    public class LectureType
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        public virtual ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();
    }
}