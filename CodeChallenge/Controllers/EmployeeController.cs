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

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
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
        /// <returns>A ReportingStructure, which includes the employee data and the total number of direct and indirect reports.</returns>
        [HttpGet("numberOfReports/{id}", Name = "GetNumberOfReports")]
        public IActionResult GetNumberOfReports(string id)
        {
            _logger.LogDebug($"Recieved employee update request for '{id}'");

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
    }
}
