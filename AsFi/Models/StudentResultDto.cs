namespace AsFi.Models
{
    public class StudentResultDto
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public string TestTopic { get; set; } = string.Empty;
        public double ScoredPoints { get; set; }
        public double PossiblePoints { get; set; }
        public int Grade { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public int Date { get; set; }
    }
}