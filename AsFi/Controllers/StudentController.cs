using AsFi.Data;
using AsFi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Drawing;
using System.Security.Claims;
using System.Text;
using ClosedXML.Excel;
using static AsFi.Controllers.TeacherController;

namespace AsFi.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly AsFiContext _context;
        public StudentController(AsFiContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var groupIds = await _context.UserGroups.Where(ug => ug.UserId == userId).Select(ug => ug.GroupId).ToListAsync();
            if (!groupIds.Any())
            {
                ViewData["ErrorMessage"] = "Вы не привязаны к группе";
                return View(new StudentDashboardDto());
            }

            var now = DateTime.UtcNow;
            var lectures = await _context.AvailableLectures
                .Include(al => al.Lecture)
                .Where(al => groupIds.Contains(al.GroupId) && al.StartDate <= now && al.EndDate >= now)
                .Select(al => al.Lecture).Distinct().ToListAsync();

            var tests = await _context.AvailableTests
                .Include(at => at.Test)
                .Where(at => groupIds.Contains(at.GroupId) && at.StartDate <= now && at.EndDate >= now)
                .Select(at => at.Test).Distinct().ToListAsync();

            return View(new StudentDashboardDto { AvailableLectures = lectures, AvailableTests = tests });
        }

        [HttpGet, Route("Student/GetStudentInfo")]
        public async Task<IActionResult> GetStudentInfo()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Json(new { success = false, message = "Пользователь не найден" });
            return Json(new
            {
                success = true,
                lastName = user.LastName,
                firstName = user.FirstName,
                patronymic = user.Patronymic,
                email = user.Email,
                avatarUrl = user.AvatarUrl
            });
        }

        [HttpGet, Route("Student/GetLectures")]
        public async Task<IActionResult> GetLectures()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var groupIds = await _context.UserGroups.Where(ug => ug.UserId == userId).Select(ug => ug.GroupId).ToListAsync();
            if (!groupIds.Any()) return Json(new { success = false, message = "Нет групп" });

            var now = DateTime.UtcNow;
            var lectures = await _context.AvailableLectures
                .Include(al => al.Lecture).ThenInclude(l => l.Topic).ThenInclude(t => t.Subject)
                .Include(al => al.Lecture).ThenInclude(l => l.Topic).ThenInclude(t => t.Section)
                .Where(al => groupIds.Contains(al.GroupId) && al.StartDate <= now && al.EndDate >= now)
                .Select(al => new
                {
                    al.Lecture.Id,
                    al.Lecture.Title,
                    al.Lecture.Content,
                    LectureType = al.Lecture.LectureType.Name,
                    TopicName = al.Lecture.Topic.Name,
                    SubjectId = al.Lecture.Topic.SubjectId,
                    SubjectName = al.Lecture.Topic.Subject.Name,
                    SectionId = al.Lecture.Topic.SectionId,
                    SectionName = al.Lecture.Topic.Section.Name
                })
                .ToListAsync();
            return Json(lectures);
        }

        [HttpGet, Route("Student/JoinGroupForm")]
        public IActionResult JoinGroupForm() => View();

        [HttpPost, Route("Student/JoinGroup")]
        public async Task<IActionResult> JoinGroup([FromBody] JoinGroupDto model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var invite = await _context.GroupJoinCodes
                .Include(gc => gc.Group)
                .FirstOrDefaultAsync(gc => gc.Code == model.InviteCode && gc.ExpiryDate > DateTime.UtcNow);
            if (invite == null) return Json(new { success = false, message = "Неверный или просроченный код" });

            if (await _context.UserGroups.AnyAsync(ug => ug.UserId == userId && ug.GroupId == invite.GroupId))
                return Json(new { success = false, message = "Вы уже в этой группе" });

            _context.UserGroups.Add(new UserGroup { UserId = userId, GroupId = invite.GroupId });
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Вы вступили в группу {invite.Group.Name}" });
        }

        [HttpGet, Route("Student/GetMyGroups")]
        public async Task<IActionResult> GetMyGroups()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var groups = await _context.UserGroups
                .Where(ug => ug.UserId == userId)
                .Include(ug => ug.Group)
                .Select(ug => new { id = ug.Group.Id, name = ug.Group.Name })
                .ToListAsync();
            return Json(new { success = true, groups });
        }

        [HttpPost, Route("Student/LeaveGroup/{groupId}")]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return Json(new { success = false, message = "Группа не найдена" });
            if (group.Name == "Без группы") return Json(new { success = false, message = "Нельзя покинуть базовую группу" });

            var ug = await _context.UserGroups.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);
            if (ug == null) return Json(new { success = false, message = "Вы не состоите в группе" });

            _context.UserGroups.Remove(ug);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Вы покинули группу {group.Name}" });
        }

        [HttpGet, Route("Student/GetTests")]
        public async Task<IActionResult> GetTests()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var groupIds = await _context.UserGroups.Where(ug => ug.UserId == userId).Select(ug => ug.GroupId).ToListAsync();
            if (!groupIds.Any()) return Json(new { success = false, message = "Нет групп" });

            var now = DateTime.UtcNow;
            var tests = await _context.AvailableTests
                .Include(at => at.Test).ThenInclude(t => t.Topic).ThenInclude(t => t.Subject)
                .Where(at => groupIds.Contains(at.GroupId) && at.StartDate <= now && at.EndDate >= now)
                .Select(at => new
                {
                    at.Test.Id,
                    at.Test.Title,
                    TopicName = at.Test.Topic.Name,
                    SubjectId = at.Test.Topic.SubjectId,
                    SubjectName = at.Test.Topic.Subject.Name,
                    TimeLimitMinutes = at.Test.TimeLimitMinutes,
                    at.Test.AttemptsAllowed
                })
                .ToListAsync();
            return Json(tests);
        }

        [HttpGet, Route("Student/TakeTest/{testId}")]
        public async Task<IActionResult> TakeTest(int testId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var groupIds = await _context.UserGroups.Where(ug => ug.UserId == userId).Select(ug => ug.GroupId).ToListAsync();
            var now = DateTime.UtcNow;

            if (!await _context.AvailableTests.AnyAsync(at => at.TestId == testId && groupIds.Contains(at.GroupId) && at.StartDate <= now && at.EndDate >= now))
            {
                TempData["Error"] = "Тест недоступен";
                return RedirectToAction("Index");
            }

            var test = await _context.Tests
                .Include(t => t.Topic)
                    .ThenInclude(tp => tp.Subject)
                .Include(t => t.TestQuestions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.Id == testId);
            if (test == null) return NotFound();

            return View(test);
        }

        [HttpPost, Route("Student/SubmitTest")]
        public async Task<IActionResult> SubmitTest([FromBody] TestSubmissionDto submission)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var test = await _context.Tests
                    .Include(t => t.TestQuestions)
                    .ThenInclude(q => q.AnswerOptions)
                    .FirstOrDefaultAsync(t => t.Id == submission.TestId);
                if (test == null) return Json(new { success = false, message = "Тест не найден" });

                var result = CalculateTestResult(test, submission.Answers);
                var attemptResult = new TestAttemptResult
                {
                    UserId = userId,
                    TestId = submission.TestId,
                    Grade = result.Grade,
                    Explanation = result.Explanation,
                    TotalPossiblePoints = (int)result.TotalPossiblePoints,
                    EarnedPoints = result.TotalScoredPoints
                };
                _context.TestAttemptResults.Add(attemptResult);
                await _context.SaveChangesAsync();

                
                var uniqueAnswers = submission.Answers
                    .GroupBy(a => a.QuestionId)
                    .Select(g => g.First()) 
                    .ToList();

                foreach (var ans in uniqueAnswers)
                {
                    if (string.IsNullOrEmpty(ans.AnswerOptionId)) continue;
                    if (!int.TryParse(ans.AnswerOptionId, out int optId)) continue;

                    _context.StudentAnswers.Add(new StudentAnswer
                    {
                        TestAttemptResultId = attemptResult.Id,
                        QuestionId = ans.QuestionId,
                        AnswerOptionId = optId,
                        PointsEarned = ans.ReceivedPoints
                    });
                }
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    result = new
                    {
                        totalScored = result.TotalScoredPoints,
                        totalPossible = result.TotalPossiblePoints,
                        grade = result.Grade,
                        explanation = result.Explanation,
                        testResultId = attemptResult.Id
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private TestResultDto CalculateTestResult(Test test, List<StudentAnswerDto> studentAnswers)
        {
            double totalScored = 0, totalPossible = 0;
            foreach (var q in test.TestQuestions)
            {
                totalPossible += q.Points;
                var answer = studentAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
                if (answer != null && int.TryParse(answer.AnswerOptionId, out int optId))
                {
                    var selected = q.AnswerOptions.FirstOrDefault(o => o.Id == optId);
                    if (selected != null && selected.IsCorrect)
                    {
                        totalScored += q.Points;
                        answer.ReceivedPoints = q.Points;
                    }
                    else answer.ReceivedPoints = 0;
                }
            }
            int grade = totalPossible > 0 ? (int)Math.Round((totalScored / totalPossible) * 10) : 0;
            grade = Math.Clamp(grade, 0, 10);
            return new TestResultDto
            {
                TotalScoredPoints = totalScored,
                TotalPossiblePoints = totalPossible,
                Grade = grade,
                Explanation = $"Набрано {totalScored:F1} из {totalPossible:F1}. Оценка: {grade}/10"
            };
        }

        [HttpGet, Route("Student/GetUserGroup")]
        public async Task<IActionResult> GetUserGroup()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userGroup = await _context.UserGroups
                .Include(ug => ug.Group)
                .Where(ug => ug.UserId == userId && ug.Group.Name != "Без группы")
                .Select(ug => ug.Group)
                .FirstOrDefaultAsync();

            if (userGroup == null)
                return Json(new { success = false, message = "Вы не состоите в группе" });

            return Json(new { success = true, groupName = userGroup.Name });
        }

        [HttpGet, Route("Student/ViewLecture/{lectureId}")]
        public async Task<IActionResult> ViewLecture(int lectureId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var groupIds = await _context.UserGroups.Where(ug => ug.UserId == userId).Select(ug => ug.GroupId).ToListAsync();
            var now = DateTime.UtcNow;

            var isAvailable = await _context.AvailableLectures
                .AnyAsync(al => al.LectureId == lectureId &&
                                groupIds.Contains(al.GroupId) &&
                                al.StartDate <= now && al.EndDate >= now);

            if (!isAvailable)
            {
                TempData["Error"] = "Лекция недоступна";
                return RedirectToAction("Index");
            }

            var lecture = await _context.Lectures
                .Include(l => l.LectureType)
                .Include(l => l.Topic)
                    .ThenInclude(t => t.Subject)
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null)
                return NotFound();

            return View(lecture);
        }

        [HttpGet, Route("Student/GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Json(new { success = false, message = "Пользователь не найден" });
            return Json(new
            {
                success = true,
                lastName = user.LastName,
                firstName = user.FirstName,
                patronymic = user.Patronymic,
                email = user.Email,
                avatarUrl = user.AvatarUrl
            });
        }

        [HttpGet, Route("Student/Profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            var prefs = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (prefs == null)
            {
                prefs = new UserPreference { UserId = userId, Theme = "dark", PrimaryColor = "purple" };
                _context.UserPreferences.Add(prefs);
                await _context.SaveChangesAsync();
            }
            ViewBag.Preferences = prefs;
            return View(user);
        }

        [HttpPost, Route("Student/UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Json(new { success = false, message = "Пользователь не найден" });

            if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
            if (dto.Patronymic != null) user.Patronymic = dto.Patronymic;
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl)) user.AvatarUrl = dto.AvatarUrl;

            var prefs = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (prefs != null)
            {
                prefs.Theme = dto.Theme ?? prefs.Theme;
                prefs.PrimaryColor = dto.PrimaryColor ?? prefs.PrimaryColor;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost, Route("Student/UploadAvatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Файл не выбран" });

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "avatars");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var savePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(savePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var relativePath = $"/avatars/{fileName}";
            return Json(new { success = true, path = relativePath });
        }

        [HttpGet, Route("Student/TestResult/{id}")]
        public async Task<IActionResult> TestResult(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _context.TestAttemptResults
                .Include(r => r.Test)
                    .ThenInclude(t => t.TestQuestions)
                        .ThenInclude(q => q.AnswerOptions)
                .Include(r => r.StudentAnswers)
                    .ThenInclude(sa => sa.AnswerOption)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (result == null) return NotFound();
            return View(result);
        }

        [HttpGet, Route("Student/TestResults")]
        public async Task<IActionResult> TestResults()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var results = await _context.TestAttemptResults
                .Include(r => r.Test)
                    .ThenInclude(t => t.Topic)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .Select(r => new StudentResultDto
                {
                    Id = r.Id,
                    TestId = r.TestId,
                    TestTitle = r.Test.Title,
                    TestTopic = r.Test.Topic.Name,
                    ScoredPoints = r.EarnedPoints,
                    PossiblePoints = r.TotalPossiblePoints,
                    Grade = (int)r.Grade,
                    Explanation = r.Explanation,
                    Date = r.Id
                })
                .ToListAsync();
            return View(results);
        }

        [HttpGet, Route("Student/ExportTestResultsToExcel")]
        public async Task<IActionResult> ExportTestResultsToExcel()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var results = await _context.TestAttemptResults
                .Include(r => r.Test)
                    .ThenInclude(t => t.Topic)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .Select(r => new
                {
                    TestTitle = r.Test.Title,
                    Topic = r.Test.Topic.Name,
                    Score = r.EarnedPoints,
                    MaxScore = r.TotalPossiblePoints,
                    Grade = r.Grade,
                    Percent = r.TotalPossiblePoints > 0 ? (r.EarnedPoints / r.TotalPossiblePoints) * 100 : 0,
                    Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
                })
                .ToListAsync();

            
            var html = new StringBuilder();
            html.Append("<html><head><meta charset='UTF-8'><title>Результаты тестов</title></head><body>");
            html.Append("<h2>Мои результаты</h2>");
            html.Append("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse;'>");
            html.Append("<thead>");
            html.Append("<tr bgcolor='#DDDDDD'><th>Название теста</th><th>Тема</th><th>Набрано баллов</th><th>Максимум баллов</th><th>Оценка (10)</th><th>Процент</th><th>Дата</th><tr>");
            html.Append("</thead><tbody>");

            double totalGrade = 0;
            double totalPercent = 0;
            int count = results.Count;

            foreach (var r in results)
            {
                totalGrade += r.Grade;
                totalPercent += r.Percent;

                html.Append($"<tr>");
                html.Append($"<td>{EscapeHtml(r.TestTitle)}</td>");
                html.Append($"<td>{EscapeHtml(r.Topic)}</td>");
                html.Append($"<td>{r.Score}</td>");
                html.Append($"<td>{r.MaxScore}</td>");
                html.Append($"<td>{r.Grade}</td>");
                html.Append($"<td>{Math.Round(r.Percent, 2)}%</td>");
                html.Append($"<td>{r.Date}</td>");
                html.Append($"</tr>");
            }

           
            if (count > 0)
            {
                double avgGrade = totalGrade / count;
                double avgPercent = totalPercent / count;

                html.Append("<tr bgcolor='#EEEEEE'>");
                html.Append("<td colspan='4' align='center'><strong>Среднее</strong></td>");
                html.Append($"<td align='center'><strong>{Math.Round(avgGrade, 2)}</strong></td>");
                html.Append($"<td align='center'><strong>{Math.Round(avgPercent, 2)}%</strong></td>");
                html.Append("<td></td>");
                html.Append("</tr>");
            }

            html.Append("</tbody><table>");
            html.Append("</body></html>");

            var bytes = Encoding.UTF8.GetBytes(html.ToString());
            return File(bytes, "application/vnd.ms-excel", $"TestResults_{DateTime.Now:yyyyMMdd_HHmmss}.xls");
        }

        private string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;")
                        .Replace("'", "&#39;");
        }



    }

    
    public class JoinGroupDto { public string InviteCode { get; set; } = ""; }
    public class TestSubmissionDto { public int TestId { get; set; } public List<StudentAnswerDto> Answers { get; set; } = new(); }
    public class StudentAnswerDto { public int QuestionId { get; set; } public string AnswerOptionId { get; set; } = ""; public double ReceivedPoints { get; set; } }
    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Patronymic { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Theme { get; set; }
        public string? PrimaryColor { get; set; }
    }
    public class TestResultDto { public double TotalScoredPoints { get; set; } public double TotalPossiblePoints { get; set; } public int Grade { get; set; } public string Explanation { get; set; } = ""; }
}