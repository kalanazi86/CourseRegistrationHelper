using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CourseRegistrationHelper.Models
{
    public class Instructor
    {
        [Key]
        public int InstructorId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public string OfficeHours { get; set; }

        public string Email { get; set; }

        // Navigation property
        public virtual ICollection<Section> Sections { get; set; }
    }
}
