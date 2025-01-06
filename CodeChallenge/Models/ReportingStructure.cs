namespace CodeChallenge.Models
{
    public class ReportingStructure
    {
        //The readme mentions naming the properties employee and numberOfReports,
        //but I'm choosing to use PascalCase as it is C# naming convention.
        public Employee Employee { get; set; }
        public int NumberOfReports { get; set; }

        public ReportingStructure(Employee employee, int numberOfReports)
        {
            this.Employee = employee;
            this.NumberOfReports = numberOfReports;
        }
    }
}
