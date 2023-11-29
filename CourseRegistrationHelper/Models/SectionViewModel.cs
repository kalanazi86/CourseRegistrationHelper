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
        private int _availableSeats;
        public int AvailableSeats
        {
            get { return Capacity - EnrolledStudents; }
            set { _availableSeats = value; } // This allows setting AvailableSeats directly if that's what you need
        }

        public List<DayOfWeek> ParsedDays
        {
            get { return ParseDays(this.Days); }
        }

        private List<DayOfWeek> ParseDays(string daysNumeric)
        {
            if (string.IsNullOrEmpty(daysNumeric))
            {
                return new List<DayOfWeek>(); // Return an empty list if daysNumeric is null or empty
            }

            var daysOfWeek = new Dictionary<char, DayOfWeek>
    {
        {'1', DayOfWeek.Sunday},
        {'2', DayOfWeek.Monday},
        {'3', DayOfWeek.Tuesday},
        {'4', DayOfWeek.Wednesday},
        {'5', DayOfWeek.Thursday},
        // ... other mappings ...
    };

            return daysNumeric.Select(n => daysOfWeek[n]).ToList();
        }

        public List<DayOfWeek> DaysOfWeek
        {
            get
            {
                return Days?.Select(c => ParseDay(c)).ToList() ?? new List<DayOfWeek>();
            }
        }

        private DayOfWeek ParseDay(char day)
        {
            return day switch
            {
                '1' => DayOfWeek.Sunday,
                '2' => DayOfWeek.Monday,
                '3' => DayOfWeek.Tuesday,
                '4' => DayOfWeek.Wednesday,
                '5' => DayOfWeek.Thursday,
                // Add cases for '6' and '7' if your system uses them for Friday and Saturday
                _ => throw new InvalidOperationException("Invalid day")
            };
        }

        public string Location { get; set; }
        public string Instructor { get; set; }

        public bool IsSelected { get; set; } // To be used with the checkbox in the view

        // ... any other properties you need

    }
}
