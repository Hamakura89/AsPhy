using System.ComponentModel.DataAnnotations.Schema;

namespace AsFi.Models
{
    [Table("tests")]
    public class Test
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; } = null!;

        [Column("time_limit_minutes")]
        public int TimeLimitMinutes { get; set; }

        [Column("topic_id")]
        public int TopicId { get; set; }

        [Column("attempts_allowed")]
        public int? AttemptsAllowed { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Topic Topic { get; set; } = null!;
        public virtual ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();
        public virtual ICollection<AvailableTest> AvailableTests { get; set; } = new List<AvailableTest>();
        public virtual ICollection<TestAttemptResult> TestAttemptResults { get; set; } = new List<TestAttemptResult>();
    }
}