using CourseRegistrationHelper.Data;
using CourseRegistrationHelper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public IActionResult AddSection()
        {
            var model = new AddSectionViewModel
            {
                Universities = GetUniversities(),
                Colleges = new List<SelectListItem>(), // Initially empty
                Departments = new List<SelectListItem>(), // Initially empty
                Courses = new List<SelectListItem>(), // Initially empty
                Instructors = new List<SelectListItem>(), // Initially empty

                StartTimes = GenerateTimeOptions("08:00", "21:00", 60), // For hours
                EndTimes = GenerateTimeOptions("08:50", "21:50", 60) // For end times
            };

            return View(model);
        }

        private List<SelectListItem> GetUniversities()
        {
            // Fetch from the database and convert to SelectListItem
            // Example:
            return _context.Universities.Select(u => new SelectListItem { Value = u.UniversityId.ToString(), Text = u.Name }).ToList();
        }

        public IActionResult GetColleges(int universityId)
        {
            var colleges = _context.Colleges.Where(c => c.UniversityId == universityId)
                .Select(c => new SelectListItem { Value = c.CollegeId.ToString(), Text = c.Name })
                .ToList();
            return Json(colleges);
        }

        public IActionResult GetDepartments(int collegeId)
        {
            var departments = _context.Departments.Where(d => d.CollegeId == collegeId)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.Name })
                .ToList();
            return Json(departments);
        }

        public IActionResult GetCourses(int departmentId)
        {
            var courses = _context.Courses.Where(c => c.DepartmentId == departmentId)
                .Select(c => new SelectListItem { Value = c.CourseId.ToString(), Text = c.Title })
                .ToList();
            return Json(courses);
        }

        public IActionResult GetInstructors(int departmentId)
        {
            var instructors = _context.Instructors
                .Where(i => i.DepartmentId == departmentId)
                .Select(i => new SelectListItem { Value = i.InstructorId.ToString(), Text = i.Name })
                .ToList();

            return Json(instructors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(AddSectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var section = new Section
                {
                    CRN = model.CRN,
                    Capacity = model.Capacity,
                    Location = model.Location,
                    Room = model.Room,
                    Days = model.Days,
                    StartTime = TimeSpan.Parse(model.StartTime),
                    EndTime = TimeSpan.Parse(model.EndTime),
                    CourseId = model.CourseId,
                    InstructorId = model.InstructorId
                    // Other properties as needed
                };

                _context.Sections.Add(section);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");

                // Redirect or show success message
            }

            else
            {
                // Re-populate dropdowns
                model.Universities = GetUniversities();
                // ... similarly for other dropdowns ...

                // If validation fails, re-render the form with validation messages
                return View(model);
            }

            // Reload dropdown data if needed
            return View(model);
        }



        //// GET: Admin/AddSection
        //public IActionResult AddSection()
        //{
        //    // Populate any required data for the view here
        //    return View();
        //}

        //// POST: Admin/AddSection
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddSection(SectionViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var section = new Section
        //        {
        //            // Map the ViewModel to your Section model
        //        };

        //        _context.Add(section);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

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

        private List<SelectListItem> GenerateTimeOptions(string startTime, string endTime, int interval)
        {
            var times = new List<SelectListItem>();
            var time = TimeSpan.Parse(startTime);

            while (time <= TimeSpan.Parse(endTime))
            {
                times.Add(new SelectListItem(time.ToString(@"hh\:mm"), time.ToString(@"hh\:mm")));
                time = time.Add(TimeSpan.FromMinutes(interval));
            }

            return times;
        }

    }
}
