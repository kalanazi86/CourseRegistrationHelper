using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CourseRegistrationHelper.Models
{
    public class College
    {
        [Key]
        public int CollegeId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [ForeignKey("University")]
        public int UniversityId { get; set; }

        // Navigation property
        public University University { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
    }
}