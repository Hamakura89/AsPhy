using System.Collections.Generic;

namespace AsFi.Models
{
    public class StudentDashboardDto
    {
        public List<Lecture> AvailableLectures { get; set; } = new List<Lecture>();
        public List<Test> AvailableTests { get; set; } = new List<Test>();
    }
}