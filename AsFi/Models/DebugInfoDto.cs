using System.Collections.Generic;

namespace AsFi.Models
{
    public class DebugInfoDto
    {
        public List<TestDto> Tests { get; set; } = new List<TestDto>();
        public List<GroupDto> Groups { get; set; } = new List<GroupDto>();
        public List<AvailableTestDto> AvailableTests { get; set; } = new List<AvailableTestDto>();
        public List<UserWithGroupsDto> UsersWithGroups { get; set; } = new List<UserWithGroupsDto>();
    }

    public class TestDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public int Duration { get; set; }
    }

    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class AvailableTestDto
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserWithGroupsDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<GroupDto> Groups { get; set; } = new List<GroupDto>();
    }

    public class RemoveAvailabilityDto
    {
        public int AvailabilityId { get; set; }
    }

    public class AnswerOptionDto
    {
        public int Id { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int? AnswerImageId { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Score { get; set; }
        public int? QuestionImageId { get; set; }
        public List<AnswerOptionDto>? AnswerOptions { get; set; }
    }

    public class TestEditDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string? Description { get; set; }
        public int PassingScore { get; set; }
        public List<QuestionDto>? Questions { get; set; }
    }

    public class MakeLectureAvailableDto
    {
        public int LectureId { get; set; }
        public int GroupId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsMandatory { get; set; } = false;
    }

    public class MakeTestAvailableDto
    {
        public int TestId { get; set; }
        public int GroupId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AttemptsAllowed { get; set; } = 1;
    }
}