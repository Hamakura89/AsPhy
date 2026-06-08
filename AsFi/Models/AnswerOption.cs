using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("answer_options")]
    public class AnswerOption
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("is_correct")]
        public bool IsCorrect { get; set; }

        [Column("text")]
        public string Text { get; set; } = null!;

        public virtual TestQuestion Question { get; set; } = null!;
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}