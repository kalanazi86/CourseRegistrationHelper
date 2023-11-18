using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace CourseRegistrationHelper.Models
{
    public class Instructor
    {
        [Key]
        public int InstructorId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        // Navigation property
        public virtual ICollection<Section> Sections { get; set; }
    }
}
