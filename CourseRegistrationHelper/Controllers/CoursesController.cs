using CourseRegistrationHelper.Models;
using CourseRegistrationHelper.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CourseRegistrationHelper.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // Displays the initial search form
        public IActionResult Search()
        {
            var model = new CourseSearchViewModel
            {
                Universities = _context.Universities
                                       .Select(u => new SelectListItem { Value = u.UniversityId.ToString(), Text = u.Name })
                                       .ToList()
            };

            return View(model);
        }
        

        // Handle the search logic when user applies filters
        [HttpPost]
        public IActionResult Search(CourseSearchViewModel searchModel)
        {
            // Logic to filter sections based on selected values
            searchModel.Sections = GetFilteredSections(searchModel.SelectedUniversityId, searchModel.SelectedCollegeId, searchModel.SelectedDepartmentId, searchModel.SelectedCourseId);

            // Repopulate dropdown lists and retain the selected value
            searchModel.Universities = _context.Universities
                .Select(u => new SelectListItem { Value = u.UniversityId.ToString(), Text = u.Name, Selected = u.UniversityId == searchModel.SelectedUniversityId })
                .ToList();
            searchModel.Colleges = _context.Colleges
                .Where(c => c.UniversityId == searchModel.SelectedUniversityId)
                .Select(c => new SelectListItem { Value = c.CollegeId.ToString(), Text = c.Name, Selected = c.CollegeId == searchModel.SelectedCollegeId })
                .ToList();
            searchModel.Departments = _context.Departments
                .Where(d => d.CollegeId == searchModel.SelectedCollegeId)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.Name, Selected = d.DepartmentId == searchModel.SelectedDepartmentId })
                .ToList();
            searchModel.Courses = _context.Courses
                .Where(c => c.DepartmentId == searchModel.SelectedDepartmentId)
                .Select(c => new SelectListItem { Value = c.CourseId.ToString(), Text = c.Title, Selected = c.CourseId == searchModel.SelectedCourseId })
                .ToList();

            // Check if the current user is an admin
            if (User.IsInRole("Admin"))
            {
                searchModel.IsAdminView = true;
                searchModel.AdminActions = searchModel.Sections.Select(s => new AdminActionViewModel
                {
                    SectionId = s.SectionId,
                    EditActionLink = Url.Action("EditSection", "Admin", new { id = s.SectionId }),
                    DeleteActionLink = Url.Action("DeleteSection", "Admin", new { id = s.SectionId })
                }).ToList();
            }

            return View(searchModel);
        }

        
        private List<SectionViewModel> GetFilteredSections(int? universityId, int? collegeId, int? departmentId, int? courseId)
        {
            var query = _context.Sections.AsQueryable();

            // Apply filters
            if (universityId.HasValue)
            {
                query = query.Where(s => s.Course.Department.College.UniversityId == universityId.Value);
            }
            if (collegeId.HasValue)
            {
                query = query.Where(s => s.Course.Department.CollegeId == collegeId.Value);
            }
            if (departmentId.HasValue)
            {
                query = query.Where(s => s.Course.DepartmentId == departmentId.Value);
            }
            if (courseId.HasValue)
            {
                query = query.Where(s => s.CourseId == courseId.Value);
            }
            // Apply other filters for college, department, course similarly

            // Project the query to a list of SectionViewModel
            return query.Select(s => new SectionViewModel
            {
                CourseName = s.Course.Title,
                CourseSymbol = s.Course.Code,
                CreditHours = s.Course.CreditHours,
                SectionId = s.SectionId,
                CRN = s.CRN,
                Capacity = s.Capacity,
                Location = s.Location,
                Room = s.Room,
                Days = s.Days,
                Time = $"{s.StartTime.ToString(@"hh\:mm")} - {s.EndTime.ToString(@"hh\:mm")}", // Format the time here
                Instructor = s.Instructor.Name,

                EnrolledStudents = s.EnrolledStudents,
                // ... other mappings
            }).ToList();
        }
        
        // New action to get colleges based on university selection
        [HttpGet]
        public IActionResult GetColleges(int universityId)
        {
            var colleges = _context.Colleges
                .Where(c => c.UniversityId == universityId)
                .Select(c => new { c.CollegeId, c.Name })
                .ToList();

            return Json(colleges);
        }

        // New action to get departments based on college selection
        [HttpGet]
        public IActionResult GetDepartments(int collegeId)
        {
            var departments = _context.Departments
                .Where(d => d.CollegeId == collegeId)
                .Select(d => new { d.DepartmentId, d.Name })
                .ToList();

            return Json(departments);
        }

        // New action to get courses based on department selection
        [HttpGet]
        public IActionResult GetCourses(int departmentId)
        {
            var courses = _context.Courses
                .Where(c => c.DepartmentId == departmentId)
                .Select(c => new { c.CourseId, c.Code, c.Title })
                .ToList();

            return Json(courses);
        }
        [HttpPost]
        public IActionResult GenerateSchedules(CourseSearchViewModel model)
        {
            // Extract the selected SectionViewModels from the model
            var selectedSections = model.Sections
                                        .Where(s => s.IsSelected)
                                        .ToList();

            // Use the method to generate non-overlapping schedules
            var nonOverlappingSchedules = GenerateNonOverlappingSchedules(selectedSections, new List<SectionViewModel>());

            // Now you have a list of non-overlapping schedules
            // You need to decide how you want to present these to the user.
            // For example, you could return a view that lists these schedules, or you could select the 'best' schedule based on some criteria.

            return View("SuggestedSchedules", nonOverlappingSchedules);
        }

        private bool SectionsOverlap(SectionViewModel first, SectionViewModel second)
        {
            // Check if days intersect
            var daysIntersect = first.DaysOfWeek.Intersect(second.DaysOfWeek).Any();
            if (!daysIntersect) return false;

            // Check if times overlap
            var timesOverlap = first.StartTime < second.EndTime && second.StartTime < first.EndTime;
            return timesOverlap;
        }

        private List<List<SectionViewModel>> GenerateNonOverlappingSchedules(List<SectionViewModel> sections, List<SectionViewModel> currentSchedule)
        {
            // The list to hold all non-overlapping schedules
            var allSchedules = new List<List<SectionViewModel>>();

            // The base case: if there are no more sections to add, return the current schedule
            if (!sections.Any())
            {
                allSchedules.Add(new List<SectionViewModel>(currentSchedule));
                return allSchedules;
            }

            for (int i = 0; i < sections.Count; i++)
            {
                var nextSection = sections[i];

                // Check if the next section overlaps with any section in the current schedule
                bool overlaps = currentSchedule.Any(current => SectionsOverlap(current, nextSection));
                if (!overlaps)
                {
                    // If it doesn't overlap, add it to the current schedule and continue building the schedule recursively
                    currentSchedule.Add(nextSection);
                    var schedulesWithNext = GenerateNonOverlappingSchedules(sections.Skip(i + 1).ToList(), currentSchedule);
                    allSchedules.AddRange(schedulesWithNext);
                    currentSchedule.Remove(nextSection);
                }
            }

            // If we can't add any more sections without overlapping, return the current schedule
            if (currentSchedule.Any())
            {
                allSchedules.Add(new List<SectionViewModel>(currentSchedule));
            }

            return allSchedules;
        }


    }
}
