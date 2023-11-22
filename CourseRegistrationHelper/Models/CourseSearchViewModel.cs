using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseRegistrationHelper.Models
{
    //public class CourseSearchViewModel
    //{
    //    public List<University> Universities { get; set; } // For dropdown
    //    public List<College> Colleges { get; set; }       // For dropdown
    //    public List<Department> Departments { get; set; } // For dropdown
    //    public List<Course> Courses { get; set; }
    //    public List<SectionViewModel> Sections { get; set; } //List for sections to display

    //    // Add properties for selected values if you need to retain dropdown selections
    //    public int SelectedUniversityId { get; set; }
    //    public int SelectedCollegeId { get; set; }
    //    public int SelectedDepartmentId { get; set; }
    //    public int SelectedCourseId { get; set; }

    //}
    public class CourseSearchViewModel
    {
        // Change these properties to use SelectListItems for the dropdowns
        public List<SelectListItem> Universities { get; set; }
        public List<SelectListItem> Colleges { get; set; }
        public List<SelectListItem> Departments { get; set; }
        public List<SelectListItem> Courses { get; set; }
        public List<SectionViewModel> Sections { get; set; }

        // Properties to hold selected values for filtering
        public int? SelectedUniversityId { get; set; }
        public int? SelectedCollegeId { get; set; }
        public int? SelectedDepartmentId { get; set; }
        public int? SelectedCourseId { get; set; }
    }

}
