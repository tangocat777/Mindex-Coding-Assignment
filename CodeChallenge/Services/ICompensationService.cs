using CodeChallenge.Models;
using System.Collections.Generic;
using System;

namespace CodeChallenge.Services
{
    public interface ICompensationService
    {
        Compensation Create(Compensation employee);

        Compensation GetByEmployeeId(string id);
    }
}
