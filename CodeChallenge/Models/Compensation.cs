using System;

namespace CodeChallenge.Models
{
    public class Compensation
    {
        public string CompensationId { get; set; }
        public Employee Employee { get; set; }

        public decimal Salary { get; set; }

        public DateTime EffectiveDate { get; set; }
    }
}
