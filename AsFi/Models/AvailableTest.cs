using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("available_tests")]
    public class AvailableTest
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        public virtual Test Test { get; set; } = null!;
        public virtual Group Group { get; set; } = null!;
    }
}