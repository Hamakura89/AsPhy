using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("available_lectures")]
    public class AvailableLecture
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("lecture_id")]
        public int LectureId { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        public virtual Lecture Lecture { get; set; } = null!;
        public virtual Group Group { get; set; } = null!;
    }
}