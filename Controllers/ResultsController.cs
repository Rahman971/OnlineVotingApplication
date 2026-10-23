using Microsoft.AspNetCore.Mvc;
using OnlineVotingApplication.Data;

namespace OnlineVotingApplication.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var elections = _context.Elections
                .Where(e => !e.IsActive &&
                            e.EndDate < DateTime.Now)
                .OrderByDescending(e => e.EndDate)
                .ToList();

            return View(elections);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var election = _context.Elections
                .FirstOrDefault(e =>
                    e.Id == id &&
                    !e.IsActive &&
                    e.EndDate < DateTime.Now);

            if (election == null)
                return NotFound();

            var results = _context.Candidates
                .Where(c =>
                    c.ElectionId == id &&
                    c.IsApproved)
                .Select(c => new ResultViewModel
                {
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
    }

    public class ResultViewModel
    {
        public string CandidateName { get; set; } = string.Empty;

        public string CandidateCode { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        public string? PhotoPath { get; set; }

        public int VoteCount { get; set; }
    }
}