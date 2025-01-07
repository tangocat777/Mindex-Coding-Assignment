using CodeChallenge.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace CodeChallenge.Repositories
{
    public interface ICompensationRepository
    {
        Compensation GetByEmployeeId(string id);
        Compensation Add(Compensation employee);
        Task SaveAsync();
    }
}
