using Microsoft.AspNetCore.Mvc;
using OnlineVotingApplication.Data;

namespace OnlineVotingApplication.Controllers
{
    public class ElectionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ElectionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // PUBLIC / STUDENT ELECTION LIST
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            var now = DateTime.Now;

            var elections = _context.Elections
                .Where(e =>
                    e.IsActive &&
                    e.StartDate <= now &&
                    e.EndDate >= now)
                .OrderBy(e => e.EndDate)
                .ToList();

            return View(elections);
        }

        // ==========================================
        // ELECTION DETAILS
        // ==========================================

        [HttpGet]
        public IActionResult Details(int id)
        {
            var now = DateTime.Now;

            var election = _context.Elections
                .FirstOrDefault(e =>
                    e.Id == id &&
                    e.IsActive &&
                    e.StartDate <= now &&
                    e.EndDate >= now);

            if (election == null)
                return NotFound();

            // Only approved candidates belonging
            // to this particular election
            var candidates = _context.Candidates
                .Where(c =>
                    c.IsApproved &&
                    c.ElectionId == id)
                .OrderBy(c => c.FullName)
                .ToList();

            ViewBag.Candidates = candidates;

            // Check whether logged-in student
            // has already voted in this election
            var studentId =
                HttpContext.Session.GetInt32("StudentId");

            if (studentId != null)
            {
                ViewBag.HasVoted = _context.Votes.Any(v =>
                    v.StudentId == studentId &&
                    v.ElectionId == id);
            }
            else
            {
                ViewBag.HasVoted = false;
            }

            return View(election);
        }
    }
}