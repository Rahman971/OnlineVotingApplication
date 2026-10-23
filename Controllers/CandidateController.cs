using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineVotingApplication.Data;
using OnlineVotingApplication.Models;

namespace OnlineVotingApplication.Controllers
{
    public class CandidateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CandidateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CANDIDATE APPLICATION - CREATE PAGE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var studentId = HttpContext.Session.GetString("StudentId");

            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Student");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s =>
                    s.StudentId == studentId &&
                    s.IsActive);

            if (student == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Student");
            }

            var elections = await _context.Elections
                .Where(e => e.IsActive &&
                            e.StartDate <= DateTime.Now &&
                            e.EndDate >= DateTime.Now)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            ViewBag.Student = student;
            ViewBag.Elections = elections;

            return View();
        }


        // =========================================================
        // SUBMIT CANDIDATE APPLICATION
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int electionId,
            string position,
            string? candidateDetails,
            IFormFile? photoFile)
        {
            var studentId = HttpContext.Session.GetString("StudentId");

            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Student");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s =>
                    s.StudentId == studentId &&
                    s.IsActive);

            if (student == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Student");
            }


            // ---------------------------------------------------------
            // Validate Election
            // ---------------------------------------------------------

            var election = await _context.Elections
                .FirstOrDefaultAsync(e =>
                    e.Id == electionId &&
                    e.IsActive &&
                    e.StartDate <= DateTime.Now &&
                    e.EndDate >= DateTime.Now);

            if (election == null)
            {
                TempData["Error"] =
                    "The selected election is not currently available.";

                return RedirectToAction(nameof(Create));
            }


            // ---------------------------------------------------------
            // Validate Position
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(position))
            {
                TempData["Error"] =
                    "Please select a contesting position.";

                return RedirectToAction(nameof(Create));
            }


            // ---------------------------------------------------------
            // Prevent Duplicate Application
            // ---------------------------------------------------------

            var existingApplication =
                await _context.CandidateApplications
                    .FirstOrDefaultAsync(c =>
                        c.StudentId == student.StudentId &&
                        c.ElectionId == electionId);

            if (existingApplication != null)
            {
                TempData["Error"] =
                    "You have already submitted a candidate application for this election.";

                return RedirectToAction(nameof(MyApplications));
            }


            // ---------------------------------------------------------
            // Candidate Photo
            // ---------------------------------------------------------

            string? photoPath = null;

            if (photoFile != null && photoFile.Length > 0)
            {
                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                var extension =
                    Path.GetExtension(photoFile.FileName)
                        .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] =
                        "Only JPG, JPEG, PNG and WEBP images are allowed.";

                    return RedirectToAction(nameof(Create));
                }


                // Maximum 2 MB
                if (photoFile.Length > 2 * 1024 * 1024)
                {
                    TempData["Error"] =
                        "Candidate photo must be less than 2 MB.";

                    return RedirectToAction(nameof(Create));
                }


                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "candidates");


                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }


                var uniqueFileName =
                    Guid.NewGuid().ToString("N") +
                    extension;


                var filePath =
                    Path.Combine(
                        uploadsFolder,
                        uniqueFileName);


                using (var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }


                photoPath =
                    "/uploads/candidates/" +
                    uniqueFileName;
            }


            // ---------------------------------------------------------
            // Create Candidate Application
            // ---------------------------------------------------------

            var application = new CandidateApplication
            {
                StudentId = student.StudentId,

                FullName = student.FullName,

                Email = student.Email,

                PhoneNumber = student.PhoneNumber,

                Department = student.Department,

                Year = student.Year,

                ElectionId = election.Id,

                Position = position.Trim(),

                CandidatePhoto = photoPath,

                CandidateDetails =
                    string.IsNullOrWhiteSpace(candidateDetails)
                        ? null
                        : candidateDetails.Trim(),

                Status = "Pending",

                AppliedAt = DateTime.Now
            };


            _context.CandidateApplications.Add(application);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Your candidate application has been submitted successfully. " +
                "Please wait for admin approval.";


            return RedirectToAction(nameof(MyApplications));
        }


        // =========================================================
        // MY APPLICATIONS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var studentId =
                HttpContext.Session.GetString("StudentId");

            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Student");
            }


            var applications =
                await _context.CandidateApplications
                    .Where(c =>
                        c.StudentId == studentId)
                    .OrderByDescending(c =>
                        c.AppliedAt)
                    .ToListAsync();


            return View(applications);
        }


        // =========================================================
        // OLD REGISTER URL SUPPORT
        // =========================================================

        [HttpGet]
        public IActionResult Register()
        {
            return RedirectToAction(nameof(Create));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            int electionId,
            string position,
            string? candidateDetails,
            IFormFile? candidatePhoto)
        {
            return await Create(
                electionId,
                position,
                candidateDetails,
                candidatePhoto);
        }
    }
}