using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("test_questions")]
    public class TestQuestion
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("question_text")]
        public string QuestionText { get; set; } = null!;

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("points")]
        public int Points { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        public virtual Test Test { get; set; } = null!;
        public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}