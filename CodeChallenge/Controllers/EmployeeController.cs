using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CodeChallenge.Services;
using CodeChallenge.Models;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;
        private readonly ICompensationService _compensationService;
        private const int MinYear = 1950;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService, ICompensationService compensationService)
        {
            _logger = logger;
            _employeeService = employeeService;
            _compensationService = compensationService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody] Employee newEmployee)
        {
            _logger.LogDebug($"Recieved employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        /// <summary>
        /// Gets the number of reports under a given employee. This counting includes indirect reports,
        /// such as employees who report to a report of the indicated employee.
        /// </summary>
        /// <param name="id">GUID of the employee to query reports</param>
        /// <returns>ReportingStructure</returns>
        [HttpGet("numberOfReports/{id}", Name = "GetNumberOfReports")]
        public IActionResult GetNumberOfReports(string id)
        {
            _logger.LogDebug($"Recieved get number of reports request for employee '{id}'");

            var guidRef = new Guid();
            //check valid input string
            if (string.IsNullOrEmpty(id))
                return BadRequest("Employee id must not be null or empty");
            if (!Guid.TryParse(id, out guidRef))
                return BadRequest("Employee id must be in the form of a GUID");

            //make sure the employee exists
            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound("Employee with id" + id + " does not exist");

            var count = 0;
            //handle null direct reports, IE someone that does not have any direct reports.
            if (existingEmployee.DirectReports is not null)
            {
                foreach (Employee report in existingEmployee.DirectReports)
                {
                    count += GetIndirectReports(report);
                }
            }
            var result = new ReportingStructure(existingEmployee, count);
            return Ok(result);
        }

        private int GetIndirectReports(Employee report)
        {
            //start count at one for current employee
            var count = 1;
            //handle null direct reports, IE someone that does not have any direct reports.
            if (report.DirectReports is not null)
            {
                foreach (Employee indirectReport in report.DirectReports)
                {
                    count += GetIndirectReports(indirectReport);
                }
            }
            return count;
        }
        /// <summary>
        /// Creates a new compensation record.
        /// </summary>
        /// <returns>Compensation</returns>
        [HttpPost("compensation")]
        public IActionResult CreateCompensationRecord([FromBody] Compensation compensation)
        {
            _logger.LogDebug($"Received create compensation request for '{compensation.Employee.EmployeeId}' effective starting '{compensation.EffectiveDate}'");

            var id = compensation.Employee.EmployeeId;

            var guidRef = new Guid();
            //check valid input string
            if (string.IsNullOrEmpty(id))
                return BadRequest("Employee id must not be null or empty");
            if (!Guid.TryParse(id, out guidRef))
                return BadRequest("Employee id must be in the form of a GUID");

            //make sure the employee exists
            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound("Employee with id" + id + " does not exist");

            //check valid date time
            //in a real life setting, this should probably have a date bound
            //by company-based constraints. But for now we'll say anything after 1950.
            //All dates in the future are valid because we plan to be in-business forever.
            var date = compensation.EffectiveDate;
            if(date.Year <= MinYear)
            {
                return BadRequest("Effective date cannot be earlier than " + (MinYear+1) + ".");
            }

            //check valid Salary.
            //We're allowing for strange cases like 0 if the CEO is paid entirely in
            //equity, but it can't be negative.
            if (compensation.Salary < 0)
            {
                return BadRequest("Salary cannot be negative");
            }

            _compensationService.Create(compensation);

            return CreatedAtRoute("GetCompensationForEmployee", new { id = compensation.Employee.EmployeeId }, compensation);
        }

        /// <summary>
        /// Gets the current compensation level for an employee.
        /// </summary>
        /// <returns>Compensation</returns>
        [HttpGet("compensation/{id}", Name = "GetCompensationForEmployee")]
        public IActionResult GetCompensationForEmployee(string id)
        {
            _logger.LogDebug($"Received get current active compensation request for employee '{id}'");
            var compensation = _compensationService.GetByEmployeeId(id);
            if (compensation == null)
                return NotFound();

            return Ok(compensation);
        }
    }
}
