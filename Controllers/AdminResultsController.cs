using Microsoft.AspNetCore.Mvc;
using OnlineVotingApplication.Data;

namespace OnlineVotingApplication.Controllers
{
    public class AdminResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // ADMIN RESULTS
        // =========================

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var elections = _context.Elections
                .OrderByDescending(e => e.CreatedAt)
                .ToList();

            return View(elections);
        }


        // =========================
        // RESULT DETAILS
        // =========================

        [HttpGet]
        public IActionResult Details(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var election = _context.Elections
                .FirstOrDefault(e => e.Id == id);

            if (election == null)
                return NotFound();

            var results = _context.Candidates
                .Where(c =>
                    c.ElectionId == id &&
                    c.IsApproved)
                .Select(c => new CandidateResultViewModel
                {
                    CandidateId = c.Id,
                    CandidateName = c.FullName,
                    CandidateCode = c.CandidateId,
                    Department = c.Department,
                    Position = c.Position,
                    PhotoPath = c.PhotoPath,

                    VoteCount = _context.Votes.Count(v =>
                        v.CandidateId == c.Id &&
                        v.ElectionId == id)
                })
                .OrderByDescending(r => r.VoteCount)
                .ThenBy(r => r.CandidateName)
                .ToList();

            ViewBag.Election = election;

            ViewBag.TotalVotes =
                _context.Votes.Count(
                    v => v.ElectionId == id);

            return View(results);
        }


        // =========================
        // PUBLISH RESULTS
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Publish(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var election = _context.Elections
                .FirstOrDefault(e => e.Id == id);

            if (election == null)
                return NotFound();

            // Election must be completed
            if (election.EndDate > DateTime.Now)
            {
                TempData["Error"] =
                    "Results can be published only after the election has ended.";

                return RedirectToAction("Index");
            }

            if (election.IsPublished)
            {
                TempData["Error"] =
                    "Results have already been published.";

                return RedirectToAction("Index");
            }

            election.IsPublished = true;

            // Make sure election is no longer active
            election.IsActive = false;

            _context.SaveChanges();

            TempData["Success"] =
                "Election results published successfully.";

            return RedirectToAction("Index");
        }


        // =========================
        // UNPUBLISH RESULTS
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Unpublish(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var election = _context.Elections
                .FirstOrDefault(e => e.Id == id);

            if (election == null)
                return NotFound();

            election.IsPublished = false;

            _context.SaveChanges();

            TempData["Success"] =
                "Public result publishing has been disabled.";

            return RedirectToAction("Index");
        }


        // =========================
        // ADMIN LOGIN CHECK
        // =========================

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session
                .GetInt32("AdminId") != null;
        }
    }


    // =========================
    // RESULT VIEW MODEL
    // =========================

    public class CandidateResultViewModel
    {
        public int CandidateId { get; set; }

        public string CandidateName { get; set; }
            = string.Empty;

        public string CandidateCode { get; set; }
            = string.Empty;

        public string Department { get; set; }
            = string.Empty;

        public string Position { get; set; }
            = string.Empty;

        public string? PhotoPath { get; set; }

        public int VoteCount { get; set; }
    }
}