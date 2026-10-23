using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineVotingApplication.Data;
using OnlineVotingApplication.Models;

namespace OnlineVotingApplication.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Student> _passwordHasher;

        public StudentController(
            ApplicationDbContext context,
            IPasswordHasher<Student> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // =========================
        // LOGIN
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(
            string studentId,
            string password)
        {
            if (string.IsNullOrWhiteSpace(studentId) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error =
                    "Please enter Student ID and Password.";

                return View();
            }

            studentId = studentId.Trim();

            var student = _context.Students
                .FirstOrDefault(s =>
                    s.StudentId == studentId &&
                    s.IsActive);

            if (student == null)
            {
                ViewBag.Error =
                    "Invalid Student ID or Password.";

                return View();
            }

            PasswordVerificationResult result;

            try
            {
                result =
                    _passwordHasher.VerifyHashedPassword(
                        student,
                        student.PasswordHash,
                        password);
            }
            catch (FormatException)
            {
                ViewBag.Error =
                    "This student account has an invalid password record. Please reset the password.";

                return View();
            }

            if (result ==
                PasswordVerificationResult.Failed)
            {
                ViewBag.Error =
                    "Invalid Student ID or Password.";

                return View();
            }

            HttpContext.Session.SetInt32(
                "StudentId",
                student.Id);

            HttpContext.Session.SetString(
                "StudentName",
                student.FullName);

            return RedirectToAction("Dashboard");
        }


        // =========================
        // REGISTER
        // =========================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Student student)
        {
            if (!ModelState.IsValid)
                return View(student);

            student.StudentId =
                student.StudentId.Trim();

            student.FullName =
                student.FullName.Trim();

            student.Email =
                student.Email.Trim();

            student.Department =
                student.Department.Trim();

            if (_context.Students.Any(
                s => s.StudentId == student.StudentId))
            {
                ModelState.AddModelError(
                    "StudentId",
                    "Student ID already exists.");

                return View(student);
            }

            if (_context.Students.Any(
                s => s.Email == student.Email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Email already exists.");

                return View(student);
            }

            student.IsActive = true;
            student.CreatedAt = DateTime.Now;

            student.PasswordHash =
                _passwordHasher.HashPassword(
                    student,
                    student.PasswordHash);

            _context.Students.Add(student);

            _context.SaveChanges();

            TempData["Success"] =
                "Registration successful. You can now login.";

            return RedirectToAction("Login");
        }


        // =========================
        // FORGOT PASSWORD
        // =========================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(
            string studentId,
            string email)
        {
            if (string.IsNullOrWhiteSpace(studentId) ||
                string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error =
                    "Please enter Student ID and registered email.";

                return View();
            }

            studentId = studentId.Trim();
            email = email.Trim();

            var student =
                _context.Students.FirstOrDefault(
                    s =>
                        s.StudentId == studentId &&
                        s.Email == email &&
                        s.IsActive);

            if (student == null)
            {
                ViewBag.Error =
                    "Student ID and email do not match our records.";

                return View();
            }

            HttpContext.Session.SetInt32(
                "PasswordResetStudentId",
                student.Id);

            return RedirectToAction("ResetPassword");
        }


        // =========================
        // RESET PASSWORD
        // =========================

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var studentId =
                HttpContext.Session.GetInt32(
                    "PasswordResetStudentId");

            if (studentId == null)
                return RedirectToAction("ForgotPassword");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(
            string newPassword,
            string confirmPassword)
        {
            var studentId =
                HttpContext.Session.GetInt32(
                    "PasswordResetStudentId");

            if (studentId == null)
                return RedirectToAction("ForgotPassword");

            if (string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error =
                    "Please enter and confirm your new password.";

                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error =
                    "Passwords do not match.";

                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error =
                    "Password must contain at least 6 characters.";

                return View();
            }

            var student =
                _context.Students.FirstOrDefault(
                    s => s.Id == studentId.Value);

            if (student == null)
            {
                HttpContext.Session.Remove(
                    "PasswordResetStudentId");

                return RedirectToAction("ForgotPassword");
            }

            student.PasswordHash =
                _passwordHasher.HashPassword(
                    student,
                    newPassword);

            _context.SaveChanges();

            HttpContext.Session.Remove(
                "PasswordResetStudentId");

            TempData["Success"] =
                "Password reset successfully. Please login with your new password.";

            return RedirectToAction("Login");
        }


        // =========================
        // DASHBOARD
        // =========================

        [HttpGet]
        public IActionResult Dashboard()
        {
            var studentId =
                HttpContext.Session.GetInt32(
                    "StudentId");

            if (studentId == null)
                return RedirectToAction("Login");

            var student =
                _context.Students.FirstOrDefault(
                    s => s.Id == studentId.Value);

            if (student == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login");
            }

            return View(
                "~/Views/Student/Dashboard.cshtml",
                student);
        }


        // =========================
        // LOGOUT
        // =========================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}