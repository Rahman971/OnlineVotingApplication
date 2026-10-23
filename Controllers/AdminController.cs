using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineVotingApplication.Data;
using OnlineVotingApplication.Models;
using OnlineVotingApplication.Services;

namespace OnlineVotingApplication.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Admin> _passwordHasher;
        private readonly IPasswordHasher<Student> _studentPasswordHasher;
        private readonly EmailService _emailService;

        public AdminController(
            ApplicationDbContext context,
            IPasswordHasher<Admin> passwordHasher,
            IPasswordHasher<Student> studentPasswordHasher,
            EmailService emailService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _studentPasswordHasher = studentPasswordHasher;
            _emailService = emailService;
        }


        // =========================================
        // ADMIN LOGIN
        // =========================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error =
                    "Please enter username and password.";

                return View();
            }

            username = username.Trim();

            var admin = _context.Admins
                .FirstOrDefault(a =>
                    a.Username == username &&
                    a.IsActive);

            if (admin == null)
            {
                ViewBag.Error =
                    "Invalid username or password.";

                return View();
            }

            var result =
                _passwordHasher.VerifyHashedPassword(
                    admin,
                    admin.PasswordHash,
                    password);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error =
                    "Invalid username or password.";

                return View();
            }

            HttpContext.Session.SetInt32(
                "AdminId",
                admin.Id);

            HttpContext.Session.SetString(
                "AdminName",
                admin.FullName);

            return RedirectToAction("Dashboard");
        }


        // =========================================
        // ADMIN DASHBOARD
        // =========================================

        [HttpGet]
        public IActionResult Dashboard()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            ViewBag.TotalStudents =
                _context.Students.Count();

            ViewBag.ActiveStudents =
                _context.Students.Count(
                    s => s.IsActive);

            ViewBag.TotalCandidates =
                _context.Candidates.Count();

            ViewBag.TotalElections =
                _context.Elections.Count();

            ViewBag.ActiveElections =
                _context.Elections.Count(
                    e => e.IsActive);

            ViewBag.TotalVotes =
                _context.Votes.Count();

            ViewBag.PendingApplications =
                _context.CandidateApplications.Count(
                    a => a.Status == "Pending");

            return View();
        }


        // =========================================
        // STUDENT MANAGEMENT
        // =========================================

        [HttpGet]
        public IActionResult Students()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var students = _context.Students
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            return View(students);
        }


        // =========================================
        // ADD STUDENT - GET
        // =========================================

        [HttpGet]
        public IActionResult AddStudent()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            return View();
        }


        // =========================================
        // ADD STUDENT - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStudent(
            string studentId,
            string fullName,
            string email,
            string year,
            string department,
            string phoneNumber,
            string password)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            studentId = studentId?.Trim() ?? "";
            fullName = fullName?.Trim() ?? "";
            email = email?.Trim() ?? "";
            year = year?.Trim() ?? "";
            department = department?.Trim() ?? "";
            phoneNumber = phoneNumber?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(studentId) ||
                string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(year) ||
                string.IsNullOrWhiteSpace(department) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error =
                    "Please fill in all required fields.";

                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error =
                    "Password must contain at least 6 characters.";

                return View();
            }

            if (_context.Students.Any(
                s => s.StudentId == studentId))
            {
                ViewBag.Error =
                    "Student ID already exists.";

                return View();
            }

            if (_context.Students.Any(
                s => s.Email == email))
            {
                ViewBag.Error =
                    "Email address already exists.";

                return View();
            }

            var student = new Student
            {
                StudentId = studentId,
                FullName = fullName,
                Email = email,
                Year = year,
                Department = department,
                PhoneNumber = phoneNumber,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            student.PasswordHash =
                _studentPasswordHasher.HashPassword(
                    student,
                    password);

            _context.Students.Add(student);
            _context.SaveChanges();

            TempData["Success"] =
                "Student account created successfully.";

            return RedirectToAction("Students");
        }


        // =========================================
        // EDIT STUDENT - GET
        // =========================================

        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var student = _context.Students
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            return View(student);
        }


        // =========================================
        // EDIT STUDENT - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStudent(
            int id,
            string studentId,
            string fullName,
            string email,
            string year,
            string department,
            string phoneNumber,
            string password)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var student = _context.Students
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            studentId = studentId?.Trim() ?? "";
            fullName = fullName?.Trim() ?? "";
            email = email?.Trim() ?? "";
            year = year?.Trim() ?? "";
            department = department?.Trim() ?? "";
            phoneNumber = phoneNumber?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(studentId) ||
                string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(year) ||
                string.IsNullOrWhiteSpace(department) ||
                string.IsNullOrWhiteSpace(phoneNumber))
            {
                ViewBag.Error =
                    "Please fill in all required fields.";

                return View(student);
            }

            if (_context.Students.Any(
                s => s.StudentId == studentId &&
                     s.Id != id))
            {
                ViewBag.Error =
                    "Student ID already exists.";

                return View(student);
            }

            if (_context.Students.Any(
                s => s.Email == email &&
                     s.Id != id))
            {
                ViewBag.Error =
                    "Email address already exists.";

                return View(student);
            }

            student.StudentId = studentId;
            student.FullName = fullName;
            student.Email = email;
            student.Year = year;
            student.Department = department;
            student.PhoneNumber = phoneNumber;

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < 6)
                {
                    ViewBag.Error =
                        "Password must contain at least 6 characters.";

                    return View(student);
                }

                student.PasswordHash =
                    _studentPasswordHasher.HashPassword(
                        student,
                        password);
            }

            _context.SaveChanges();

            TempData["Success"] =
                "Student details updated successfully.";

            return RedirectToAction("Students");
        }


        // =========================================
        // DELETE STUDENT
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStudent(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var student = _context.Students
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            bool hasVotes = _context.Votes
                .Any(v => v.StudentId == id);

            if (hasVotes)
            {
                TempData["Error"] =
                    "This student cannot be deleted because voting activity is already associated with this account.";

                return RedirectToAction("Students");
            }

            _context.Students.Remove(student);
            _context.SaveChanges();

            TempData["Success"] =
                "Student account deleted successfully.";

            return RedirectToAction("Students");
        }


        // =========================================
        // ACTIVATE / DEACTIVATE STUDENT
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStudentStatus(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var student = _context.Students
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            student.IsActive = !student.IsActive;

            _context.SaveChanges();

            TempData["Success"] =
                student.IsActive
                ? "Student account activated successfully."
                : "Student account deactivated successfully.";

            return RedirectToAction("Students");
        }


        // =========================================
        // IMPORT COLLEGE STUDENTS
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ImportCollegeStudents()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var students = new[]
            {
                new { Name = "J. Jeya Dharshini", Id = "BCA26001", Email = "jeys4341@gmail.com", Year = "1st", Dept = "BCA", Phone = "9842678121" },
                new { Name = "H. Kohila", Id = "BCA26002", Email = "kohilakohila092@gmail.com", Year = "1st", Dept = "BCA", Phone = "6383641217" },
                new { Name = "M. Muthu Lakshmi", Id = "BCA26003", Email = "msmuthulakshmi2@gmail.com", Year = "1st", Dept = "BCA", Phone = "9361564800" },
                new { Name = "K. Noorul Dhasfiha", Id = "BCA26004", Email = "dhasfiha112@gmail.com", Year = "1st", Dept = "BCA", Phone = "7397001421" },
                new { Name = "J. Serina", Id = "BCA26005", Email = "serinajeevaraj@gmail.com", Year = "1st", Dept = "BCA", Phone = "8012229006" },
                new { Name = "A. Usha", Id = "BCA26006", Email = "usha.am38@gmail.com", Year = "1st", Dept = "BCA", Phone = "7550532257" },

                new { Name = "S. Bagavathi Muthu", Id = "BCA25001", Email = "kalamuthu0507@gmail.com", Year = "2nd", Dept = "BCA", Phone = "6385747465" },
                new { Name = "S. Murugeshwari", Id = "BCA25002", Email = "murugeskumar95@gmail.com", Year = "2nd", Dept = "BCA", Phone = "9003500910" },

                new { Name = "K. Kowsalya", Id = "BCA24002", Email = "kandhasamyk504@gmail.com", Year = "3rd", Dept = "BCA", Phone = "7397636011" },
                new { Name = "R. Sathya Priya", Id = "BCA24004", Email = "sathyarajesh009@gmail.com", Year = "3rd", Dept = "BCA", Phone = "9655344936" },
                new { Name = "A. Thoufika", Id = "BCA24006", Email = "thoufika556@gmail.com", Year = "3rd", Dept = "BCA", Phone = "9865905714" },
                new { Name = "A. Vanisha", Id = "BCA24001", Email = "vanishaa990@gmail.com", Year = "3rd", Dept = "BCA", Phone = "9042537473" },

                new { Name = "M. Arumuga Lakshmi", Id = "BIO24001", Email = "kaveriaru23@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "9361347285" },
                new { Name = "S. Athisakthi", Id = "BIO24002", Email = "athisakthisekar02@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "8072357311" },
                new { Name = "E. Esakkiammal", Id = "BIO24003", Email = "esakkiammale2006@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "9943944294" },
                new { Name = "M. Gomathi Sankari", Id = "BIO24004", Email = "msankari573@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "7010486925" },
                new { Name = "K. Hema", Id = "BIO24005", Email = "hemakanthan417@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "6381123077" },
                new { Name = "S. Keerthana", Id = "BIO24006", Email = "keerthanaias@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "9789437651" },
                new { Name = "P. Muthumari", Id = "BIO24007", Email = "mp1613067@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "8610993141" },
                new { Name = "C. Pavithra", Id = "BIO24008", Email = "cpavithra292@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "9042697912" },
                new { Name = "M. Safrin Fathima", Id = "BIO24009", Email = "safrin8675148661@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "9345189864" },
                new { Name = "M. Selvi", Id = "BIO24010", Email = "mselvi28012007@gmail.com", Year = "3rd", Dept = "BIO--TECH", Phone = "7845617110" }
            };

            const string defaultPassword = "Student@123";

            int addedCount = 0;

            foreach (var item in students)
            {
                if (_context.Students.Any(
                    s => s.StudentId == item.Id))
                    continue;

                if (_context.Students.Any(
                    s => s.Email == item.Email))
                    continue;

                var student = new Student
                {
                    StudentId = item.Id,
                    FullName = item.Name,
                    Email = item.Email,
                    Year = item.Year,
                    Department = item.Dept,
                    PhoneNumber = item.Phone,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                student.PasswordHash =
                    _studentPasswordHasher.HashPassword(
                        student,
                        defaultPassword);

                _context.Students.Add(student);
                addedCount++;
            }

            _context.SaveChanges();

            TempData["Success"] =
                $"{addedCount} college student accounts added successfully.";

            return RedirectToAction("Students");
        }


        // =========================================
        // CANDIDATE APPLICATIONS
        // =========================================

        [HttpGet]
        public async Task<IActionResult> CandidateApplications()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var applications =
                await _context.CandidateApplications
                    .OrderByDescending(a => a.AppliedAt)
                    .ToListAsync();

            return View(applications);
        }


        // =========================================
        // APPROVE CANDIDATE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCandidate(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var application =
                await _context.CandidateApplications
                    .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                TempData["Error"] =
                    "Candidate application not found.";

                return RedirectToAction("CandidateApplications");
            }

            if (application.Status == "Approved")
            {
                TempData["Error"] =
                    "This application is already approved.";

                return RedirectToAction("CandidateApplications");
            }

            if (application.Status == "Rejected")
            {
                TempData["Error"] =
                    "A rejected application cannot be approved directly.";

                return RedirectToAction("CandidateApplications");
            }

            var existingCandidate =
                await _context.Candidates
                    .FirstOrDefaultAsync(c =>
                        c.ElectionId == application.ElectionId &&
                        c.CandidateId == application.StudentId);

            if (existingCandidate == null)
            {
                var candidate = new Candidate
                {
                    ElectionId = application.ElectionId,
                    CandidateId = application.StudentId,
                    FullName = application.FullName,
                    Department = application.Department,
                    Position = application.Position,
                    Manifesto = application.CandidateDetails,
                    PhotoPath = application.CandidatePhoto
                };

                _context.Candidates.Add(candidate);
            }

            application.Status = "Approved";
            application.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"{application.FullName} has been approved as a candidate.";

            return RedirectToAction("CandidateApplications");
        }


        // =========================================
        // REJECT CANDIDATE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCandidate(
            int id,
            string? rejectionReason)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var application =
                await _context.CandidateApplications
                    .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                TempData["Error"] =
                    "Candidate application not found.";

                return RedirectToAction("CandidateApplications");
            }

            if (application.Status == "Approved")
            {
                TempData["Error"] =
                    "An approved candidate cannot be rejected from this page.";

                return RedirectToAction("CandidateApplications");
            }

            application.Status = "Rejected";

            application.RejectionReason =
                string.IsNullOrWhiteSpace(rejectionReason)
                    ? "Application rejected by administrator."
                    : rejectionReason.Trim();

            application.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"{application.FullName}'s application has been rejected.";

            return RedirectToAction("CandidateApplications");
        }


        // =========================================
        // ADMIN LOGOUT
        // =========================================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }


        // =========================================
        // CREATE FIRST ADMIN
        // =========================================

        [HttpGet]
        public IActionResult CreateFirstAdmin()
        {
            if (_context.Admins.Any())
            {
                return NotFound();
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFirstAdmin(
            string username,
            string fullName,
            string email,
            string password)
        {
            if (_context.Admins.Any())
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error =
                    "Please fill in all required fields.";

                return View();
            }

            username = username.Trim();
            fullName = fullName.Trim();
            email = email.Trim();

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                    .IsValid(email))
            {
                ViewBag.Error =
                    "Please enter a valid email address.";

                return View();
            }

            if (password.Length < 8)
            {
                ViewBag.Error =
                    "Password must contain at least 8 characters.";

                return View();
            }

            var usernameExists = _context.Admins
                .Any(a => a.Username == username);

            if (usernameExists)
            {
                ViewBag.Error =
                    "This username is already in use.";

                return View();
            }

            var emailExists = _context.Admins
                .Any(a => a.Email == email);

            if (emailExists)
            {
                ViewBag.Error =
                    "This email address is already registered.";

                return View();
            }

            var admin = new Admin
            {
                Username = username,
                FullName = fullName,
                Email = email,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            admin.PasswordHash =
                _passwordHasher.HashPassword(
                    admin,
                    password);

            _context.Admins.Add(admin);
            _context.SaveChanges();

            TempData["Success"] =
                "Administrator account created successfully. Please login.";

            return RedirectToAction("Login");
        }


        // =========================================
        // ADMIN FORGOT PASSWORD - GET
        // =========================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        // =========================================
        // ADMIN FORGOT PASSWORD - SEND OTP
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error =
                    "Please enter your registered email address.";

                return View();
            }

            email = email.Trim();

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a =>
                    a.Email == email &&
                    a.IsActive);

            /*
             * Security:
             * Do not reveal whether the email
             * exists in the system.
             */
            if (admin == null)
            {
                TempData["Success"] =
                    "If the email address is registered, an OTP has been sent.";

                return RedirectToAction("ForgotPassword");
            }


            // =========================================
            // REMOVE OLD UNUSED OTPs
            // =========================================

            var oldTokens =
                await _context.PasswordResetTokens
                    .Where(t =>
                        t.AdminId == admin.Id &&
                        !t.IsUsed)
                    .ToListAsync();

            if (oldTokens.Count > 0)
            {
                _context.PasswordResetTokens
                    .RemoveRange(oldTokens);
            }


            // =========================================
            // GENERATE SECURE 6-DIGIT OTP
            // =========================================

            int otpNumber =
                RandomNumberGenerator.GetInt32(
                    100000,
                    1000000);

            string otp =
                otpNumber.ToString("D6");


            // =========================================
            // CREATE OTP RECORD
            // =========================================

            var resetToken = new PasswordResetToken
            {
                AdminId = admin.Id,

                // Existing Token column is used
                // to store the OTP.
                Token = otp,

                // OTP expires after 5 minutes.
                ExpiresAt =
                    DateTime.UtcNow.AddMinutes(5),

                IsUsed = false,

                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetTokens
                .Add(resetToken);

            await _context.SaveChangesAsync();


            try
            {
                // =========================================
                // SEND OTP TO ADMIN EMAIL
                // =========================================

                await _emailService.SendAdminOtpEmailAsync(
                    admin.Email,
                    admin.FullName,
                    otp);


                // =========================================
                // STORE RESET EMAIL IN SESSION
                // =========================================

                HttpContext.Session.SetString(
                    "AdminResetEmail",
                    admin.Email);


                TempData["Success"] =
                    "A 6-digit OTP has been sent to your registered email.";

                return RedirectToAction("VerifyOtp");
            }
            catch
            {
                // Remove OTP if email sending failed.

                _context.PasswordResetTokens
                    .Remove(resetToken);

                await _context.SaveChangesAsync();

                ViewBag.Error =
                    "We could not send the OTP. Please try again later.";

                return View();
            }
        }


        // =========================================
        // VERIFY OTP - GET
        // =========================================

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var email =
                HttpContext.Session.GetString(
                    "AdminResetEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] =
                    "Please request a new OTP.";

                return RedirectToAction(
                    "ForgotPassword");
            }

            ViewBag.Email = email;

            return View();
        }


        // =========================================
        // VERIFY OTP - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(
            string otp)
        {
            var email =
                HttpContext.Session.GetString(
                    "AdminResetEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] =
                    "Your OTP session has expired. Please request a new OTP.";

                return RedirectToAction(
                    "ForgotPassword");
            }


            if (string.IsNullOrWhiteSpace(otp))
            {
                ViewBag.Error =
                    "Please enter the OTP.";

                ViewBag.Email = email;

                return View();
            }


            otp = otp.Trim();


            // OTP must contain exactly 6 digits.

            if (otp.Length != 6 ||
                !otp.All(char.IsDigit))
            {
                ViewBag.Error =
                    "Please enter a valid 6-digit OTP.";

                ViewBag.Email = email;

                return View();
            }


            // =========================================
            // FIND ADMIN
            // =========================================

            var admin =
                await _context.Admins
                    .FirstOrDefaultAsync(a =>
                        a.Email == email &&
                        a.IsActive);

            if (admin == null)
            {
                TempData["Error"] =
                    "Unable to verify the OTP.";

                return RedirectToAction(
                    "ForgotPassword");
            }


            // =========================================
            // FIND OTP
            // =========================================

            var resetToken =
                await _context.PasswordResetTokens
                    .Where(t =>
                        t.AdminId == admin.Id &&
                        t.Token == otp &&
                        !t.IsUsed)
                    .OrderByDescending(
                        t => t.CreatedAt)
                    .FirstOrDefaultAsync();


            if (resetToken == null)
            {
                ViewBag.Error =
                    "Invalid OTP. Please check the OTP and try again.";

                ViewBag.Email = email;

                return View();
            }


            // =========================================
            // CHECK OTP EXPIRY
            // =========================================

            if (resetToken.ExpiresAt <= DateTime.UtcNow)
            {
                resetToken.IsUsed = true;

                await _context.SaveChangesAsync();

                ViewBag.Error =
                    "This OTP has expired. Please request a new OTP.";

                ViewBag.Email = email;

                return View();
            }


            // =========================================
            // OTP VERIFIED
            // =========================================

            resetToken.IsUsed = true;

            await _context.SaveChangesAsync();


            // Store successful verification in session.

            HttpContext.Session.SetString(
                "AdminPasswordResetVerified",
                "true");

            HttpContext.Session.SetString(
                "AdminPasswordResetAdminId",
                admin.Id.ToString());


            return RedirectToAction(
                "ResetPassword");
        }


        // =========================================
        // RESEND OTP
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp()
        {
            var email =
                HttpContext.Session.GetString(
                    "AdminResetEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] =
                    "Please enter your email address again.";

                return RedirectToAction(
                    "ForgotPassword");
            }


            var admin =
                await _context.Admins
                    .FirstOrDefaultAsync(a =>
                        a.Email == email &&
                        a.IsActive);

            if (admin == null)
            {
                TempData["Success"] =
                    "If the email address is registered, an OTP has been sent.";

                return RedirectToAction(
                    "ForgotPassword");
            }


            // =========================================
            // REMOVE PREVIOUS OTP
            // =========================================

            var oldTokens =
                await _context.PasswordResetTokens
                    .Where(t =>
                        t.AdminId == admin.Id &&
                        !t.IsUsed)
                    .ToListAsync();

            if (oldTokens.Count > 0)
            {
                _context.PasswordResetTokens
                    .RemoveRange(oldTokens);
            }


            // =========================================
            // GENERATE NEW OTP
            // =========================================

            int otpNumber =
                RandomNumberGenerator.GetInt32(
                    100000,
                    1000000);

            string otp =
                otpNumber.ToString("D6");


            var resetToken =
                new PasswordResetToken
                {
                    AdminId = admin.Id,

                    Token = otp,

                    ExpiresAt =
                        DateTime.UtcNow.AddMinutes(5),

                    IsUsed = false,

                    CreatedAt = DateTime.UtcNow
                };

            _context.PasswordResetTokens
                .Add(resetToken);

            await _context.SaveChangesAsync();


            try
            {
                await _emailService.SendAdminOtpEmailAsync(
                    admin.Email,
                    admin.FullName,
                    otp);


                TempData["Success"] =
                    "A new OTP has been sent to your registered email.";

                return RedirectToAction(
                    "VerifyOtp");
            }
            catch
            {
                _context.PasswordResetTokens
                    .Remove(resetToken);

                await _context.SaveChangesAsync();

                TempData["Error"] =
                    "Unable to send the OTP. Please try again later.";

                return RedirectToAction(
                    "VerifyOtp");
            }
        }


        // =========================================
        // RESET PASSWORD - GET
        // =========================================

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var verified =
                HttpContext.Session.GetString(
                    "AdminPasswordResetVerified");

            if (verified != "true")
            {
                TempData["Error"] =
                    "Please verify the OTP before resetting your password.";

                return RedirectToAction(
                    "ForgotPassword");
            }

            return View();
        }


        // =========================================
        // RESET PASSWORD - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            string newPassword,
            string confirmPassword)
        {
            var verified =
                HttpContext.Session.GetString(
                    "AdminPasswordResetVerified");

            var adminIdString =
                HttpContext.Session.GetString(
                    "AdminPasswordResetAdminId");


            if (verified != "true" ||
                string.IsNullOrWhiteSpace(
                    adminIdString) ||
                !int.TryParse(
                    adminIdString,
                    out int adminId))
            {
                TempData["Error"] =
                    "Your password reset session has expired.";

                return RedirectToAction(
                    "ForgotPassword");
            }


            // =========================================
            // VALIDATE PASSWORD
            // =========================================

            if (string.IsNullOrWhiteSpace(
                    newPassword) ||
                string.IsNullOrWhiteSpace(
                    confirmPassword))
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


            if (newPassword.Length < 8)
            {
                ViewBag.Error =
                    "Password must contain at least 8 characters.";

                return View();
            }


            // =========================================
            // FIND ADMIN
            // =========================================

            var admin =
                await _context.Admins
                    .FirstOrDefaultAsync(a =>
                        a.Id == adminId &&
                        a.IsActive);


            if (admin == null)
            {
                TempData["Error"] =
                    "The administrator account could not be found.";

                return RedirectToAction(
                    "ForgotPassword");
            }


            // =========================================
            // HASH NEW PASSWORD
            // =========================================

            admin.PasswordHash =
                _passwordHasher.HashPassword(
                    admin,
                    newPassword);


            await _context.SaveChangesAsync();


            // =========================================
            // CLEAR PASSWORD RESET SESSION
            // =========================================

            HttpContext.Session.Remove(
                "AdminResetEmail");

            HttpContext.Session.Remove(
                "AdminPasswordResetVerified");

            HttpContext.Session.Remove(
                "AdminPasswordResetAdminId");


            TempData["Success"] =
                "Password reset successfully. Please login with your new password.";


            return RedirectToAction(
                "Login");
        }


        // =========================================
        // ADMIN LOGIN CHECK
        // =========================================

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session
                .GetInt32("AdminId") != null;
        }
    }
}