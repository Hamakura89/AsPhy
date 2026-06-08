using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("lectures")] 
    public class Lecture
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; } = null!;

        [Column("content")]
        public string Content { get; set; } = null!;

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("lecture_type_id")]
        public int LectureTypeId { get; set; }

        [Column("topic_id")]
        public int TopicId { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        public virtual LectureType LectureType { get; set; } = null!;
        public virtual Topic Topic { get; set; } = null!;
        public virtual ICollection<AvailableLecture> AvailableLectures { get; set; } = new List<AvailableLecture>();
    }
}