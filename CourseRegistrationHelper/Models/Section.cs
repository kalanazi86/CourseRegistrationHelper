using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CourseRegistrationHelper.Models
{
    public class Section
    {
        [Key]
        public int SectionId { get; set; }

        [Required]
        public int CRN { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required, StringLength(100)]
        public string Location { get; set; } // e.g., 'SEU Riyadh-males', 'SEU Jeddah-females', 'Online'

        [StringLength(50)]
        public string Room { get; set; } // e.g., 'Room 101', 'Lab A', etc.

        // Assume the class schedule is represented by a single string, e.g., "MoWeFr 10:00-11:15"
        [Required, StringLength(50)]
        public string Schedule { get; set; }

        // Start and end times are stored as TimeSpan, which EF Core maps to time in SQL Server
        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [ForeignKey("Instructor")]
        public int InstructorId { get; set; }

        // Navigation properties
        public Course Course { get; set; }
        public Instructor Instructor { get; set; }
    }
}
