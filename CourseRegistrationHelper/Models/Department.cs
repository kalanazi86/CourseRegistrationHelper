using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CourseRegistrationHelper.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [ForeignKey("College")]
        public int CollegeId { get; set; }

        // Navigation property
        public College College { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
    }
}
