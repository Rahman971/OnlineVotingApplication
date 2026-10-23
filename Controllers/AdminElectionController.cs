using Microsoft.AspNetCore.Mvc;
using OnlineVotingApplication.Data;
using OnlineVotingApplication.Models;

namespace OnlineVotingApplication.Controllers
{
    public class AdminElectionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminElectionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================
        // ELECTION MANAGEMENT
        // ================================

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

        // ================================
        // CREATE ELECTION
        // ================================

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Election election)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            if (!ModelState.IsValid)
                return View(election);

            if (election.EndDate <= election.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date must be later than start date.");

                return View(election);
            }

            bool duplicateName = _context.Elections.Any(
                e => e.ElectionName == election.ElectionName);

            if (duplicateName)
            {
                ModelState.AddModelError(
                    "ElectionName",
                    "An election with this name already exists.");

                return View(election);
            }

            election.IsActive = false;
            election.CreatedAt = DateTime.Now;

            _context.Elections.Add(election);
            _context.SaveChanges();

            TempData["Success"] =
                "Election created successfully.";

            return RedirectToAction("Index");
        }

        // ================================
        // EDIT ELECTION
        // ================================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var election = _context.Elections
                .FirstOrDefault(e => e.Id == id);

            if (election == null)
                return NotFound();

            return View(election);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Election election)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            if (id != election.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(election);

            if (election.EndDate <= election.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date must be later than start date.");

                return View(election);
            }

            var existingElection = _context.Elections
                .FirstOrDefault(e => e.Id == id);

            if (existingElection == null)
                return NotFound();

            existingElection.ElectionName =
                election.ElectionName;

            existingElection.Description =
                election.Description;

            existingElection.StartDate =
                election.StartDate;

            existingElection.EndDate =
                election.EndDate;

            _context.SaveChanges();

            TempData["Success"] =
                "Election updated successfully.";

            return RedirectToAction("Index");
        }

        // ================================
        // START ELECTION
        // ================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Start(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var election = _context.Elections
                .FirstOrDefault(e => e.Id == id);

            if (election == null)
                return NotFound();

            // Only one election can be active
            var activeElections = _context.Elections
                .Where(e => e.IsActive && e.Id != id)
                .ToList();

            foreach (var activeElection in activeElections)
            {
                activeElection.IsActive = false;
            }

            election.IsActive = true;

            _context.SaveChanges();

            TempData["Success"] =
                "Election started successfully.";

            return RedirectToAction("Index");
        }

        // ================================
        // STOP ELECTION
        // ================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Stop(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var election = _context.Elections
                .FirstOrDefault(e => e.Id == id);

            if (election == null)
                return NotFound();

            election.IsActive = false;

            _context.SaveChanges();

            TempData["Success"] =
                "Election stopped successfully.";

            return RedirectToAction("Index");
        }

        // ================================
        // DELETE ELECTION
        // ================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            var election = _context.Elections
                .FirstOrDefault(e => e.Id == id);

            if (election == null)
                return NotFound();

            if (election.IsActive)
            {
                TempData["Error"] =
                    "Active election cannot be deleted. Stop the election first.";

                return RedirectToAction("Index");
            }

            _context.Elections.Remove(election);
            _context.SaveChanges();

            TempData["Success"] =
                "Election deleted successfully.";

            return RedirectToAction("Index");
        }

        // ================================
        // ADMIN SESSION CHECK
        // ================================

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminId") != null;
        }
    }
}