namespace CourseRegistrationHelper.Models
{
    public class SectionViewModel
    {
        // Add properties that match the section details you need to display
        public string CourseName { get; set; }
        public string CourseSymbol { get; set; }
        public int CreditHours { get; set; }
        public int CRN { get; set; }

        public string Room { get; set; }
        public string Days { get; set; }
        public string Time { get; set; }
        public int Capacity { get; set; }
        public int EnrolledStudents { get; set; }
        public int AvailableSeats => Capacity - EnrolledStudents;
        public string Location { get; set; }
        public string Instructor { get; set; }
        // ... any other properties you need
    }
}
