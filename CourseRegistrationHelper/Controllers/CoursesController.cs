using CourseRegistrationHelper.Models;
using CourseRegistrationHelper.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
                Universities = _context.Universities.ToList(),
                //Colleges = _context.Colleges.ToList(),
                //Departments = _context.Departments.ToList(),
                //Courses = _context.Courses.ToList()
                // Initialize other properties if necessary
            };

            return View(model);
        }

        // Handle the search logic when user applies filters
        [HttpPost]
        public IActionResult Search(CourseSearchViewModel searchModel)
        {
            // Apply search filters and update the model accordingly
            var filteredCourses = _context.Courses
                // Filter by university, college, department, etc., based on searchModel
                .ToList();

            searchModel.Courses = filteredCourses;
            return View(searchModel);
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
    }
}
