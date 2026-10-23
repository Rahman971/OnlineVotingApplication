using Microsoft.AspNetCore.Mvc;
using OnlineVotingApplication.Data;

namespace OnlineVotingApplication.Controllers
{
    public class VoteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VoteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // CAST VOTE PAGE
        // =========================================

        [HttpGet]
        public IActionResult Cast(int electionId, int candidateId)
        {
            var studentId =
                HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
                return RedirectToAction("Login", "Student");

            var now = DateTime.Now;

            // Check active election
            var election = _context.Elections
                .FirstOrDefault(e =>
                    e.Id == electionId &&
                    e.IsActive &&
                    e.StartDate <= now &&
                    e.EndDate >= now);

            if (election == null)
            {
                TempData["Error"] =
                    "Election is not currently available.";

                return RedirectToAction(
                    "Index",
                    "Election");
            }

            // Check approved candidate
            var candidate = _context.Candidates
                .FirstOrDefault(c =>
                    c.Id == candidateId &&
                    c.ElectionId == electionId &&
                    c.IsApproved);

            if (candidate == null)
            {
                TempData["Error"] =
                    "Selected candidate is not available.";

                return RedirectToAction(
                    "Details",
                    "Election",
                    new { id = electionId });
            }

            // Check whether student already voted
            var alreadyVoted = _context.Votes.Any(v =>
                v.StudentId == studentId.Value &&
                v.ElectionId == electionId);

            if (alreadyVoted)
            {
                TempData["Error"] =
                    "You have already voted in this election.";

                return RedirectToAction(
                    "Details",
                    "Election",
                    new { id = electionId });
            }

            ViewBag.Election = election;

            return View(candidate);
        }


        // =========================================
        // CONFIRM AND SUBMIT VOTE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm(
            int electionId,
            int candidateId)
        {
            var studentId =
                HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
                return RedirectToAction("Login", "Student");

            var now = DateTime.Now;

            // Check active election
            var election = _context.Elections
                .FirstOrDefault(e =>
                    e.Id == electionId &&
                    e.IsActive &&
                    e.StartDate <= now &&
                    e.EndDate >= now);

            if (election == null)
            {
                TempData["Error"] =
                    "This election is not currently available.";

                return RedirectToAction(
                    "Index",
                    "Election");
            }

            // Check approved candidate belongs to election
            var candidate = _context.Candidates
                .FirstOrDefault(c =>
                    c.Id == candidateId &&
                    c.ElectionId == electionId &&
                    c.IsApproved);

            if (candidate == null)
            {
                TempData["Error"] =
                    "The selected candidate is not available.";

                return RedirectToAction(
                    "Details",
                    "Election",
                    new { id = electionId });
            }

            // Check duplicate vote
            var alreadyVoted = _context.Votes.Any(v =>
                v.StudentId == studentId.Value &&
                v.ElectionId == electionId);

            if (alreadyVoted)
            {
                TempData["Error"] =
                    "You have already voted in this election.";

                return RedirectToAction(
                    "Details",
                    "Election",
                    new { id = electionId });
            }

            // Create vote
            var vote = new Models.Vote
            {
                StudentId = studentId.Value,
                CandidateId = candidateId,
                ElectionId = electionId,
                VotedAt = DateTime.Now
            };

            _context.Votes.Add(vote);

            try
            {
                _context.SaveChanges();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                // Database unique constraint protects
                // against duplicate votes.

                TempData["Error"] =
                    "Your vote could not be recorded because you may have already voted.";

                return RedirectToAction(
                    "Details",
                    "Election",
                    new { id = electionId });
            }

            return RedirectToAction(
                "Success",
                new
                {
                    electionId = electionId
                });
        }


        // =========================================
        // VOTE SUCCESS PAGE
        // =========================================

        [HttpGet]
        public IActionResult Success(int electionId)
        {
            var studentId =
                HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
                return RedirectToAction("Login", "Student");

            var election = _context.Elections
                .FirstOrDefault(e =>
                    e.Id == electionId);

            if (election == null)
                return NotFound();

            return View(election);
        }
    }
}