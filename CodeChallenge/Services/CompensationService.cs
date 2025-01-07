using CodeChallenge.Models;
using CodeChallenge.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CodeChallenge.Services
{
    public class CompensationService : ICompensationService
    {
        private readonly ICompensationRepository _compensationRepository;

        public CompensationService(ICompensationRepository compensationRepository)
        {
            _compensationRepository = compensationRepository;
        }

        public Compensation Create(Compensation compensation)
        {
            //set to empty list when creating compensation record.
            //This prevents an issue where the Employee fails to persist.
            compensation.Employee.DirectReports = new List<Employee>();
            if (compensation != null)
            {
                _compensationRepository.Add(compensation);
                _compensationRepository.SaveAsync().Wait();
            }

            return compensation;
        }

        public Compensation GetByEmployeeId(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return _compensationRepository.GetByEmployeeId(id);
            }
            return null;
        }
    }
}
