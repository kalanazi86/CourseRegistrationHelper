using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseRegistrationHelper.Models
{
    public class CourseSearchViewModel
    {
        public List<University> Universities { get; set; } // For dropdown
        public List<College> Colleges { get; set; }       // For dropdown
        public List<Department> Departments { get; set; } // For dropdown
        public List<Course> Courses { get; set; }         // For displaying search results

        // Add properties for selected values if you need to retain dropdown selections
        public int SelectedUniversityId { get; set; }
        public int SelectedCollegeId { get; set; }
        public int SelectedDepartmentId { get; set; }
        public int SelectedCourseId { get; set; }
    }
}
