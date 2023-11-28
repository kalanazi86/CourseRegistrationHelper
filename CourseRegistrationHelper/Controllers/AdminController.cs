using CourseRegistrationHelper.Data;
using CourseRegistrationHelper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseRegistrationHelper.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var sections = await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                .Select(s => new SectionViewModel
                {
                    SectionId = s.SectionId,
                    CourseName = s.Course.Title,
                    CourseSymbol = s.Course.Code,
                    CreditHours = s.Course.CreditHours,
                    CRN = s.CRN,
                    Days = s.Days,
                    Time = s.StartTime + "-" + s.EndTime,
                    AvailableSeats = s.Capacity - s.EnrolledStudents,
                    Location = s.Location,
                    Instructor = s.Instructor.Name
                }).ToListAsync();

            return View(sections);
        }

        // GET: Admin/AddSection
        public IActionResult AddSection()
        {
            // Populate any required data for the view here
            return View();
        }

        // POST: Admin/AddSection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(SectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var section = new Section
                {
                    // Map the ViewModel to your Section model
                };

                _context.Add(section);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: Admin/EditSection/5
        public async Task<IActionResult> EditSection(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                return NotFound();
            }

            var model = new SectionViewModel
            {
                // Map the Section to your SectionViewModel
            };

            return View(model);
        }

        // POST: Admin/EditSection/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSection(int id, SectionViewModel model)
        {
            if (id != model.SectionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var section = await _context.Sections.FindAsync(id);
                    // Map the ViewModel to your Section model

                    _context.Update(section);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SectionExists(model.SectionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/DeleteSection/5
        public async Task<IActionResult> DeleteSection(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                .FirstOrDefaultAsync(m => m.SectionId == id);

            if (section == null)
            {
                return NotFound();
            }

            // Map to ViewModel if necessary, or directly pass the entity if you're using it in the view
            var model = new SectionViewModel
            {
                // Map the Section to your SectionViewModel
            };

            return View(model);
        }

        // POST: Admin/DeleteSection/5
        [HttpPost, ActionName("DeleteSection")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SectionExists(int id)
        {
            return _context.Sections.Any(e => e.SectionId == id);
        }
    }
}
