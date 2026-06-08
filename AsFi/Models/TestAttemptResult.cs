using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("test_attempt_results")]
    public class TestAttemptResult
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("grade")]
        public double Grade { get; set; }

        [Column("explanation")]
        public string Explanation { get; set; } = null!;

        [Column("total_possible_points")]
        public int TotalPossiblePoints { get; set; }

        [Column("earned_points")]
        public double EarnedPoints { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Test Test { get; set; } = null!;
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}