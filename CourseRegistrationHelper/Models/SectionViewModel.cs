namespace CourseRegistrationHelper.Models
{
    public class SectionViewModel
    {
        // Add properties that match the section details you need to display
        public int SectionId { get; set; }
        public string CourseName { get; set; }
        public string CourseSymbol { get; set; }
        public int CreditHours { get; set; }
        public int CRN { get; set; }

        public string Room { get; set; }
        public string Days { get; set; }
        public string Time { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public int EnrolledStudents { get; set; }
        public int AvailableSeats => Capacity - EnrolledStudents;
        public string Location { get; set; }
        public string Instructor { get; set; }

        public bool IsSelected { get; set; } // To be used with the checkbox in the view

        // ... any other properties you need

    }
}
