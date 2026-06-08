using AsFi.Data;
using AsFi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AsFi.Controllers
{
    public class AccountController : Controller
    {
        private readonly AsFiContext _context;

        public AccountController(AsFiContext context) => _context = context;

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var hashed = HashPassword(model.Password);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == hashed);
            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(model);
            }
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Аккаунт деактивирован");
                return View(model);
            }

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("Login", user.Login),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Email", user.Email)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var props = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 30 : 1)
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);
            return RedirectToAction(user.Role == "Teacher" ? "Index" : "Index", user.Role == "Teacher" ? "Teacher" : "Student");
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { exists = false });
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            return Json(new { exists });
        }

        [HttpGet]
        public async Task<IActionResult> CheckLoginExists(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return Json(new { exists = false });
            var exists = await _context.Users.AnyAsync(u => u.Login == login);
            return Json(new { exists });
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            if (await _context.Users.AnyAsync(u => u.Login == model.Login))
            {
                ModelState.AddModelError("Login", "Логин уже существует");
                return View(model);
            }
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email уже существует");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Patronymic = model.Patronymic?.Trim() ?? "",
                Login = model.Login,
                Email = model.Email,
                Password = HashPassword(model.Password),
                Role = "Student",
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var defaultGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Name == "Без группы");
            if (defaultGroup == null)
            {
                defaultGroup = new Group { Name = "Без группы" };
                _context.Groups.Add(defaultGroup);
                await _context.SaveChangesAsync();
            }
            _context.UserGroups.Add(new UserGroup { UserId = user.Id, GroupId = defaultGroup.Id });
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("Login", user.Login),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Email", user.Email)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return RedirectToAction("Index", "Student");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).ToLower();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}