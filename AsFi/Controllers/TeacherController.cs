using AsFi.Data;
using AsFi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AsFi.Controllers
{
    public class TeacherController : Controller
    {
        private readonly AsFiContext _context;
        private readonly IWebHostEnvironment _env;

        public TeacherController(AsFiContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================== ГЛАВНАЯ СТРАНИЦА ===========================
        [HttpGet]
        [Route("Teacher/Index")]
        public IActionResult Index()
        {
            return View(new List<Test>());
        }

        // =========================== СПРАВОЧНИКИ ===========================
        [HttpGet]
        [Route("Teacher/GetSubjects")]
        public async Task<IActionResult> GetSubjects()
        {
            var subjects = await _context.Subjects.Select(s => new { s.Id, s.Name }).ToListAsync();
            return Json(subjects);
        }

        [HttpGet]
        [Route("Teacher/GetSections")]
        public async Task<IActionResult> GetSections()
        {
            var sections = await _context.Sections.Select(s => new { s.Id, s.Name }).ToListAsync();
            return Json(sections);
        }

        [HttpGet]
        [Route("Teacher/GetLectureTypes")]
        public async Task<IActionResult> GetLectureTypes()
        {
            var types = await _context.LectureTypes.Select(t => new { t.Id, t.Name }).ToListAsync();
            return Json(types);
        }

        // =========================== ЛЕКЦИИ ===========================
        [HttpGet]
        [Route("Teacher/GetLectures")]
        public async Task<IActionResult> GetLectures()
        {
            var lectures = await _context.Lectures
                .Include(l => l.Topic).ThenInclude(t => t.Subject)
                .Include(l => l.Topic).ThenInclude(t => t.Section)
                .Include(l => l.LectureType)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.CreatedAt,
                    SubjectId = l.Topic.SubjectId,
                    SubjectName = l.Topic.Subject.Name,
                    SectionId = l.Topic.SectionId,
                    SectionName = l.Topic.Section.Name,
                    LectureTypeId = l.LectureTypeId,
                    LectureTypeName = l.LectureType.Name,
                    l.Content,
                    l.ImageUrl
                })
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
            return Json(lectures);
        }

        [HttpPost]
        [Route("Teacher/CreateLecture")]
        public async Task<IActionResult> CreateLecture([FromBody] CreateLectureDto dto)
        {
            if (dto == null || dto.SubjectId <= 0 || dto.SectionId <= 0 || string.IsNullOrWhiteSpace(dto.Title))
                return Json(new { success = false, message = "Заполните все поля" });

            var topic = new Topic
            {
                Name = dto.Title,
                SubjectId = dto.SubjectId,
                SectionId = dto.SectionId
            };
            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            var lecture = new Lecture
            {
                Title = dto.Title,
                TopicId = topic.Id,
                LectureTypeId = dto.LectureTypeId,
                Content = "",
                CreatedAt = DateTime.UtcNow
            };
            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = lecture.Id });
        }

        [HttpGet]
        [Route("Teacher/EditLecture/{id}")]
        public async Task<IActionResult> EditLecture(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null) return NotFound();
            ViewBag.Topics = await _context.Topics.ToListAsync();
            return View(lecture);
        }

        [HttpPost]
        [Route("Teacher/SaveLecture")]
        public async Task<IActionResult> SaveLecture([FromBody] LectureEditDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return Json(new { success = false, message = "Название обязательно" });

            var lecture = await _context.Lectures.FindAsync(dto.Id);
            if (lecture == null) return Json(new { success = false, message = "Лекция не найдена" });

            lecture.Title = dto.Title;
            lecture.Content = dto.Content ?? "";
            lecture.Comment = dto.Comment ?? "";
            lecture.LectureTypeId = dto.LectureTypeId ?? lecture.LectureTypeId;
            lecture.TopicId = dto.TopicId ?? lecture.TopicId;
            lecture.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("Teacher/DeleteLecture")]
        public async Task<IActionResult> DeleteLecture([FromBody] DeleteRequest request)
        {
            var lecture = await _context.Lectures.FindAsync(request.Id);
            if (lecture == null) return Json(new { success = false, message = "Лекция не найдена" });
            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // =========================== ТЕСТЫ ===========================
        [HttpGet]
        [Route("Teacher/GetTests")]
        public async Task<IActionResult> GetTests()
        {
            var tests = await _context.Tests
                .Include(t => t.Topic).ThenInclude(tp => tp.Subject)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.CreatedAt,
                    t.TimeLimitMinutes,
                    TopicId = t.TopicId,
                    TopicName = t.Topic.Name,
                    SubjectId = t.Topic.SubjectId,
                    SubjectName = t.Topic.Subject.Name
                })
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Json(tests);
        }

        [HttpGet("Teacher/GetTopics")]
        public async Task<IActionResult> GetTopics()
        {
            var topics = await _context.Topics.Select(t => new { t.Id, t.Name }).ToListAsync();
            return Json(topics);
        }

        [HttpPost]
        [Route("Teacher/CreateTest")]
        public async Task<IActionResult> CreateTest([FromBody] CreateTestDto dto)
        {
            if (dto == null || dto.SubjectId <= 0 || string.IsNullOrWhiteSpace(dto.TopicName) || string.IsNullOrWhiteSpace(dto.Title))
                return Json(new { success = false, message = "Заполните все поля" });

            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == dto.TopicName && t.SubjectId == dto.SubjectId);
            if (topic == null)
            {
                topic = new Topic
                {
                    Name = dto.TopicName,
                    SubjectId = dto.SubjectId,
                    SectionId = 1
                };
                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();
            }

            var test = new Test
            {
                Title = dto.Title,
                TopicId = topic.Id,
                TimeLimitMinutes = dto.TimeLimitMinutes,
                CreatedAt = DateTime.UtcNow
            };
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = test.Id });
        }

        [HttpGet("Teacher/EditTest/{id}")]
        public async Task<IActionResult> EditTest(int id)
        {
            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (test == null) return NotFound();
            ViewBag.Topics = await _context.Topics.ToListAsync();
            return View(test);
        }

        [HttpPost]
        [Route("Teacher/SaveTest")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> SaveTest([FromBody] TestEditDto incoming)
        {
            if (incoming == null)
                return Json(new { success = false, message = "Нет данных" });

            
            var existingResults = await _context.TestAttemptResults.Where(tr => tr.TestId == incoming.Id).ToListAsync();
            if (existingResults.Any())
            {
                
                var resultIds = existingResults.Select(r => r.Id).ToList();
                var studentAnswers = await _context.StudentAnswers.Where(sa => resultIds.Contains(sa.TestAttemptResultId)).ToListAsync();
                _context.StudentAnswers.RemoveRange(studentAnswers);
                _context.TestAttemptResults.RemoveRange(existingResults);
                await _context.SaveChangesAsync();
            }

            var existing = await _context.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.Id == incoming.Id);
            if (existing == null) return Json(new { success = false, message = "Тест не найден" });

            
            if (existing.TestQuestions != null)
            {
                var allOpts = existing.TestQuestions.SelectMany(q => q.AnswerOptions).ToList();
                if (allOpts.Any()) _context.AnswerOptions.RemoveRange(allOpts);
                _context.TestQuestions.RemoveRange(existing.TestQuestions);
                await _context.SaveChangesAsync();
            }

            _context.ChangeTracker.Clear();
            existing = await _context.Tests.FindAsync(incoming.Id);
            if (existing == null) return Json(new { success = false, message = "Тест не найден после очистки" });

            existing.Title = incoming.Title ?? existing.Title;
            existing.TimeLimitMinutes = incoming.TimeLimitMinutes > 0 ? incoming.TimeLimitMinutes : existing.TimeLimitMinutes;
            existing.TopicId = incoming.TopicId;
            existing.AttemptsAllowed = incoming.AttemptsAllowed > 0 ? incoming.AttemptsAllowed : 1; // добавьте поле в DTO

            existing.TestQuestions = new List<TestQuestion>();
            if (incoming.Questions != null)
            {
                foreach (var qdto in incoming.Questions)
                {
                    var q = new TestQuestion
                    {
                        QuestionText = qdto.Text ?? "",
                        Points = qdto.Points > 0 ? qdto.Points : 1,
                        ImageUrl = qdto.ImageUrl,
                        AnswerOptions = new List<AnswerOption>()
                    };
                    if (qdto.AnswerOptions != null)
                    {
                        foreach (var adto in qdto.AnswerOptions)
                        {
                            q.AnswerOptions.Add(new AnswerOption
                            {
                                IsCorrect = adto.IsCorrect,
                                ImageUrl = adto.ImageUrl,
                                Text = adto.Text ?? ""
                            });
                        }
                    }
                    existing.TestQuestions.Add(q);
                }
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("Teacher/DeleteTest")]
        public async Task<IActionResult> DeleteTest([FromBody] DeleteRequest request)
        {
            var test = await _context.Tests.FindAsync(request.Id);
            if (test == null) return Json(new { success = false, message = "Тест не найден" });
            if (await _context.TestAttemptResults.AnyAsync(tr => tr.TestId == request.Id))
                return Json(new { success = false, message = "Тест уже пройден, удаление запрещено" });

            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // =========================== ДОСТУПЫ ===========================
        [HttpGet]
        [Route("Teacher/GetGroups")]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.Groups.Select(g => new { g.Id, Name = g.Name }).ToListAsync();
            return Json(groups);
        }

        [HttpPost]
        [Route("Teacher/MakeLectureAvailable")]
        public async Task<IActionResult> MakeLectureAvailable([FromBody] MakeLectureAvailableDto request)
        {
            if (request == null || request.LectureId <= 0 || request.GroupId <= 0)
                return Json(new { success = false, message = "Неверные данные" });

            var existing = await _context.AvailableLectures
                .FirstOrDefaultAsync(al => al.LectureId == request.LectureId && al.GroupId == request.GroupId);
            if (existing != null)
            {
                existing.StartDate = request.StartDate;
                existing.EndDate = request.EndDate;
            }
            else
            {
                _context.AvailableLectures.Add(new AvailableLecture
                {
                    LectureId = request.LectureId,
                    GroupId = request.GroupId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                });
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        [Route("Teacher/GetLectureAvailability/{lectureId}")]
        public async Task<IActionResult> GetLectureAvailability(int lectureId)
        {
            var now = DateTime.UtcNow;
            var list = await _context.AvailableLectures
                .Include(al => al.Group)
                .Where(al => al.LectureId == lectureId && al.EndDate >= now)
                .Select(al => new { al.Id, al.GroupId, GroupName = al.Group.Name, al.StartDate, al.EndDate })
                .ToListAsync();
            return Json(list);
        }

        [HttpPost]
        [Route("Teacher/RemoveLectureAvailability")]
        public async Task<IActionResult> RemoveLectureAvailability([FromBody] RemoveAvailabilityDto request)
        {
            var avail = await _context.AvailableLectures.FindAsync(request.AvailabilityId);
            if (avail == null) return Json(new { success = false, message = "Запись не найдена" });
            _context.AvailableLectures.Remove(avail);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("Teacher/MakeTestAvailable")]
        public async Task<IActionResult> MakeTestAvailable([FromBody] MakeTestAvailableDto request)
        {
            if (request == null || request.TestId <= 0 || request.GroupId <= 0)
                return Json(new { success = false, message = "Неверные данные" });

            var existing = await _context.AvailableTests
                .FirstOrDefaultAsync(at => at.TestId == request.TestId && at.GroupId == request.GroupId);
            if (existing != null)
            {
                existing.StartDate = request.StartDate;
                existing.EndDate = request.EndDate;
            }
            else
            {
                _context.AvailableTests.Add(new AvailableTest
                {
                    TestId = request.TestId,
                    GroupId = request.GroupId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                });
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        [Route("Teacher/GetTestAvailability/{testId}")]
        public async Task<IActionResult> GetTestAvailability(int testId)
        {
            var now = DateTime.UtcNow;
            var list = await _context.AvailableTests
                .Include(at => at.Group)
                .Where(at => at.TestId == testId && at.EndDate >= now)
                .Select(at => new { at.Id, at.GroupId, GroupName = at.Group.Name, at.StartDate, at.EndDate })
                .ToListAsync();
            return Json(list);
        }

        [HttpPost]
        [Route("Teacher/RemoveTestAvailability")]
        public async Task<IActionResult> RemoveTestAvailability([FromBody] RemoveAvailabilityDto request)
        {
            var avail = await _context.AvailableTests.FindAsync(request.AvailabilityId);
            if (avail == null) return Json(new { success = false, message = "Запись не найдена" });
            _context.AvailableTests.Remove(avail);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        
        [HttpPost("Teacher/CreateSubject")]
        public async Task<IActionResult> CreateSubject([FromBody] SubjectDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return Json(new { success = false });
            var subject = new Subject { Name = dto.Name };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = subject.Id });
        }

        
        [HttpPost("Teacher/CreateSection")]
        public async Task<IActionResult> CreateSection([FromBody] SectionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return Json(new { success = false });
            var section = new Section { Name = dto.Name };
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = section.Id });
        }

        
        [HttpPost("Teacher/CreateTopic")]
        public async Task<IActionResult> CreateTopic([FromBody] CreateTopicDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.SubjectId <= 0) return Json(new { success = false });
            var topic = new Topic { Name = dto.Name, SubjectId = dto.SubjectId, SectionId = 1 }; // раздел по умолчанию 1
            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = topic.Id });
        }

        // =========================== КОДОВЫЕ СЛОВА ===========================
        [HttpGet("Teacher/GenerateGroupInviteCode/{groupId}")]
        public async Task<IActionResult> GenerateGroupInviteCode(int groupId)
        {
            try
            {
                var group = await _context.Groups.FindAsync(groupId);
                if (group == null) return Json(new { success = false, message = "Группа не найдена" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;

                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new Random();
                string code;
                do
                {
                    code = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
                } while (await _context.GroupJoinCodes.AnyAsync(gc => gc.Code == code));

                var invite = new GroupJoinCode
                {
                    GroupId = groupId,
                    Code = code,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    UserId = userId ?? 0
                };
                _context.GroupJoinCodes.Add(invite);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    code = code,
                    expiresAt = invite.ExpiryDate.ToString("dd.MM.yyyy HH:mm"),
                    groupName = group.Name
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Teacher/GetGroupInviteCodes")]
        public async Task<IActionResult> GetGroupInviteCodes()
        {
            var codes = await _context.GroupJoinCodes
                .Include(gc => gc.Group)
                .OrderByDescending(gc => gc.ExpiryDate)
                .Select(gc => new
                {
                    gc.Id,
                    gc.Code,
                    gc.GroupId,
                    groupName = gc.Group.Name,
                    createdAt = gc.ExpiryDate.AddDays(-7).ToString("dd.MM.yyyy HH:mm"),
                    expiresAt = gc.ExpiryDate.ToString("dd.MM.yyyy HH:mm"),
                    isActive = gc.ExpiryDate > DateTime.UtcNow
                })
                .ToListAsync();
            return Json(new { success = true, inviteCodes = codes });
        }

        [HttpPost]
        [Route("Teacher/RevokeInviteCode/{codeId}")]
        public async Task<IActionResult> RevokeInviteCode(int codeId)
        {
            var code = await _context.GroupJoinCodes.FindAsync(codeId);
            if (code == null) return Json(new { success = false, message = "Код не найден" });
            _context.GroupJoinCodes.Remove(code);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // =========================== ТЕКУЩИЙ ПОЛЬЗОВАТЕЛЬ ===========================
        [HttpGet]
        [Route("Teacher/GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Json(new { success = false, message = "Не авторизован" });

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
            catch
            {
                return Json(new { success = false, message = "Ошибка сервера" });
            }
        }

    

        // =========================== ЗАГРУЗКА ФАЙЛОВ ===========================
        [HttpPost]
        [Route("Teacher/UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Файл не выбран" });

                var uploadFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var savePath = Path.Combine(uploadFolder, fileName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                    await file.CopyToAsync(stream);
                var relativePath = $"/uploads/{fileName}";
                return Json(new { success = true, path = relativePath, fileName = file.FileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        
        [HttpGet]
        [Route("Teacher/Profile")]
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

        
        [HttpPost]
        [Route("Teacher/UpdateProfile")]
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

        
        [HttpPost]
        [Route("Teacher/UploadAvatar")]
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

        
        public class UpdateProfileDto
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Patronymic { get; set; }
            public string? AvatarUrl { get; set; }
            public string? Theme { get; set; }
            public string? PrimaryColor { get; set; }
        }

        // =========================== DTO ===========================
        public class DeleteRequest { public int Id { get; set; } }
        public class LectureEditDto
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Content { get; set; }
            public string? Comment { get; set; }
            public int? LectureTypeId { get; set; }
            public int? TopicId { get; set; }
            public string? ImageUrl { get; set; }
        }
        public class MakeLectureAvailableDto
        {
            public int LectureId { get; set; }
            public int GroupId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        public class MakeTestAvailableDto
        {
            public int TestId { get; set; }
            public int GroupId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        public class RemoveAvailabilityDto { public int AvailabilityId { get; set; } }
        public class CreateLectureDto
        {
            public int SubjectId { get; set; }
            public int SectionId { get; set; }
            public string Title { get; set; } = "";
            public int LectureTypeId { get; set; }
        }
        public class CreateTestDto
        {
            public int SubjectId { get; set; }
            public string TopicName { get; set; } = "";
            public string Title { get; set; } = "";
            public int TimeLimitMinutes { get; set; }
        }
        public class TestEditDto
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public int TimeLimitMinutes { get; set; }
            public int TopicId { get; set; }
            public int AttemptsAllowed { get; set; } = 1;
            public List<QuestionDto>? Questions { get; set; }
        }
        public class QuestionDto
        {
            public string? Text { get; set; }
            public int Points { get; set; }
            public string? ImageUrl { get; set; }
            public List<AnswerOptionDto>? AnswerOptions { get; set; }
        }
        public class AnswerOptionDto
        {
            public bool IsCorrect { get; set; }
            public string? ImageUrl { get; set; }
            public string? Text { get; set; }
        }

        public class SubjectDto { public string Name { get; set; } }
        public class SectionDto { public string Name { get; set; } }
        public class CreateTopicDto { public string Name { get; set; } public int SubjectId { get; set; } }

    }
}