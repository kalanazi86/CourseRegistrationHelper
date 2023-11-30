using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseRegistrationHelper.Models
{
    public class AddSectionViewModel
    {
        public int? UniversityId { get; set; }
        public int? CollegeId { get; set; }
        public int? DepartmentId { get; set; }
        public List<SelectListItem> StartTimes { get; set; }
        public List<SelectListItem> EndTimes { get; set; }
        //public int? CourseId { get; set; }


        public List<SelectListItem> Universities { get; set; }
        public List<SelectListItem> Colleges { get; set; }
        public List<SelectListItem> Departments { get; set; }
        public List<SelectListItem> Courses { get; set; }
        public List<SelectListItem> Instructors { get; set; }

        // Section Details
        public int CRN { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string Room { get; set; }
        public string Days { get; set; } // To store as a string like "135"
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int CourseId { get; set; } // To be set automatically when a course is selected
        public int InstructorId { get; set; }
    }

}
