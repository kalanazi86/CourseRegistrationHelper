using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseRegistrationHelper.Models
{
    public class University
    {
        [Key]
        public int UniversityId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
        public string URL { get; set; }

        // Navigation property
        public virtual ICollection<College> Colleges { get; set; }
    }
}