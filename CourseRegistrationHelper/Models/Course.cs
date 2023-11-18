using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Collections.Specialized.BitVector32;


namespace CourseRegistrationHelper.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required, StringLength(10)]
        public string Code { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        // Navigation property
        public Department Department { get; set; }
        public virtual ICollection<Section> Sections { get; set; }
    }
}

