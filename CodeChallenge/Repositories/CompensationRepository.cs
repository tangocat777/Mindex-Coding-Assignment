using CodeChallenge.Data;
using CodeChallenge.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeChallenge.Repositories
{
    public class CompensationRepository : ICompensationRepository
    {
        private readonly CompensationContext _compensationContext;

        public CompensationRepository(CompensationContext compensationContext)
        {
            _compensationContext = compensationContext;
        }

        public Compensation Add(Compensation compensation)
        {
            compensation.CompensationId = Guid.NewGuid().ToString();
            _compensationContext.CompensationSet.Add(compensation);
            return compensation;
        }

        public Compensation GetByEmployeeId(string id)
        {
            return _compensationContext.CompensationSet.Include(c => c.Employee).FirstOrDefault(c => c.Employee.EmployeeId == id);
        }

        public Task SaveAsync()
        {
            return _compensationContext.SaveChangesAsync();
        }
    }
}
