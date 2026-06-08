using System.Collections.Generic;

namespace AsFi.Models
{
    public class StudentTestDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }
        public class StudentQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Score { get; set; }
        public string Image { get; set; } = string.Empty;
        public List<StudentAnswerOptionDto> AnswerOptions { get; set; } = new();
    }

    public class StudentAnswerOptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string? CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class TestSubmissionDto
    {
        public int TestId { get; set; }
        public List<StudentAnswerDto> Answers { get; set; } = new();
    }

    public class StudentAnswerDto
    {
        public int QuestionId { get; set; }
        public string AnswerOptionId { get; set; } = string.Empty;
        public double ReceivedPoints { get; set; }
    }

    public class TestResultDto
    {
        public double TotalScoredPoints { get; set; }
        public double TotalPossiblePoints { get; set; }
        public int Grade { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}