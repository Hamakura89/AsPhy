using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("student_answers")]
    public class StudentAnswer

    {

        [Column("test_attempt_result_id")]
        public int TestAttemptResultId { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("answer_option_id")]
        public int AnswerOptionId { get; set; }

        [Column("points_earned")]
        public double PointsEarned { get; set; }

        public virtual TestAttemptResult TestAttemptResult { get; set; } = null!;
        public virtual TestQuestion Question { get; set; } = null!;
        public virtual AnswerOption AnswerOption { get; set; } = null!;
    }
}