
using System;
using System.Net;
using System.Net.Http;
using System.Text;

using CodeChallenge.Models;

using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        // Attribute ClassInitialize requires this signature
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer();
            _httpClient = _testServer.NewClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        [DataRow("16a596ae-edd3-4847-99fe-c4518e82c86f", "John", "Lennon", 4)]
        [DataRow("03aa1462-ffa9-4978-901b-7c001562cf6f", "Ringo", "Starr", 2)]
        [DataRow("c0c2293d-16bd-4603-8e08-638a9d18b22c", "George", "Harrison", 0)]
        public void GetNumberOfReports_Returns_Ok(string employeeId, string expectedFirstName, string expectedLastName, int expectedReports)
        {

            // Execute
            var postRequestTask = _httpClient.GetAsync($"api/employee/numberOfReports/{employeeId}");
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var resultStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(expectedFirstName, resultStructure.Employee.FirstName);
            Assert.AreEqual(expectedLastName, resultStructure.Employee.LastName);
            Assert.AreEqual(expectedReports, resultStructure.NumberOfReports);
        }

        [TestMethod]
        public void GetNumberOfReports_InvalidId_Returns_BadRequest()
        {
            // Arrange
            string employeeId = "5";

            // Execute
            var postRequestTask = _httpClient.GetAsync($"api/employee/numberOfReports/{employeeId}");
            var response = postRequestTask.Result;
            var message = response.Content.ReadAsStringAsync();

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("Employee id must be in the form of a GUID", message.Result);
        }

        [TestMethod]
        public void GetNumberOfReports_EmployeeDoesNotExist_Returns_NotFound()
        {
            // Arrange
            string employeeId = Guid.NewGuid().ToString();

            // Execute
            var postRequestTask = _httpClient.GetAsync($"api/employee/numberOfReports/{employeeId}");
            var response = postRequestTask.Result;
            var message = response.Content.ReadAsStringAsync();

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual($"Employee with id {employeeId} does not exist", message.Result);
        }

        [TestMethod]
        public void CreateCompensation_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f",
                FirstName = "John",
                LastName = "Lennon"
            };
            var compensation = new Compensation()
            {
                Salary = 5,
                EffectiveDate = new DateTime(2001, 7, 19),
                Employee = employee
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newCompensation = response.DeserializeContent<Compensation>();
            Assert.IsNotNull(newCompensation.CompensationId);
            Assert.AreEqual(compensation.Salary, newCompensation.Salary);
            Assert.AreEqual(compensation.EffectiveDate, newCompensation.EffectiveDate);
            Assert.AreEqual(employee.FirstName, newCompensation.Employee.FirstName);
        }

        [TestMethod]
        public void CreateCompensation_EmployeeIdNullEmpty_Returns_BadRequest()
        {
            // Arrange
            var employee = new Employee()
            {
                FirstName = "John",
                LastName = "Lennon"
            };
            var compensation = new Compensation()
            {
                Salary = 5,
                EffectiveDate = new DateTime(2001, 7, 19),
                Employee = employee
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var message = response.Content.ReadAsStringAsync();

            Assert.AreEqual("Employee id must not be null or empty", message.Result);
        }

        [TestMethod]
        public void CreateCompensation_EmployeeIdNonGUID_Returns_BadRequest()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "NotAGUID",
                FirstName = "John",
                LastName = "Lennon"
            };
            var compensation = new Compensation()
            {
                Salary = 5,
                EffectiveDate = new DateTime(2001, 7, 19),
                Employee = employee
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var message = response.Content.ReadAsStringAsync();

            Assert.AreEqual("Employee id must be in the form of a GUID", message.Result);
        }

        [TestMethod]
        public void CreateCompensation_EmployeeIdDoesNotExist_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = Guid.NewGuid().ToString(),
                FirstName = "John",
                LastName = "Lennon"
            };
            var compensation = new Compensation()
            {
                Salary = 5,
                EffectiveDate = new DateTime(2001, 7, 19),
                Employee = employee
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            var message = response.Content.ReadAsStringAsync();

            Assert.AreEqual($"Employee with id {employee.EmployeeId} does not exist", message.Result);
        }

        [TestMethod]
        public void CreateCompensation_EmployeeNull_Returns_BadRequest()
        {
            // Arrange
            var compensation = new Compensation()
            {
                Salary = 5,
                EffectiveDate = new DateTime(2001, 7, 19)
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var message = response.Content.ReadAsStringAsync();
            Assert.AreEqual($"Employee must not be null", message.Result);
        }

        [TestMethod]
        public void CreateCompensation_DateTooEarly_Returns_BadRequest()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f",
                FirstName = "John",
                LastName = "Lennon"
            };
            var compensation = new Compensation()
            {
                Salary = 5,
                EffectiveDate = new DateTime(1949, 1, 1),
                Employee = employee
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var message = response.Content.ReadAsStringAsync();

            Assert.AreEqual("Effective date cannot be earlier than 1951.", message.Result);
        }

        [TestMethod]
        public void CreateCompensation_DateNull_Returns_BadRequest()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f",
                FirstName = "John",
                LastName = "Lennon"
            };
            var compensation = new Compensation()
            {
                Salary = 5,
                Employee = employee
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var message = response.Content.ReadAsStringAsync();

            //Null DateTime autopopulates to earliest possible DateTime. Way too early for our records.
            Assert.AreEqual("Effective date cannot be earlier than 1951.", message.Result);
        }

        [TestMethod]
        public void CreateCompensation_SalaryNegative_Returns_BadRequest()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f",
                FirstName = "John",
                LastName = "Lennon"
            };
            var compensation = new Compensation()
            {
                Salary = -1,
                EffectiveDate = new DateTime(2001, 1, 1),
                Employee = employee
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var message = response.Content.ReadAsStringAsync();

            Assert.AreEqual("Salary cannot be negative", message.Result);
        }

        [TestMethod]
        public void CreateCompensation_SalaryNull_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "c0c2293d-16bd-4603-8e08-638a9d18b22c",
                FirstName = "George",
                LastName = "Harrison"
            };
            var compensation = new Compensation()
            {
                EffectiveDate = new DateTime(2001, 1, 1),
                Employee = employee
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newCompensation = response.DeserializeContent<Compensation>();
            Assert.IsNotNull(newCompensation.CompensationId);
            //null salary autopopulates to 0
            Assert.AreEqual(compensation.Salary, 0);
            Assert.AreEqual(compensation.EffectiveDate, newCompensation.EffectiveDate);
            Assert.AreEqual(employee.FirstName, newCompensation.Employee.FirstName);
        }

        [TestMethod]
        public void GetCompensationByEmployeeId_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            //These were created in a previous test.
            var expectedSalary = 5;
            var expectedDate = new DateTime(2001, 7, 19);

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/compensation/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var compensation = response.DeserializeContent<Compensation>();
            Assert.AreEqual(expectedFirstName, compensation.Employee.FirstName);
            Assert.AreEqual(expectedSalary, compensation.Salary);
            Assert.AreEqual(expectedDate, compensation.EffectiveDate);
        }

        [TestMethod]
        public void GetCompensationByEmployeeId_DoesNotExist_Returns_NotFound()
        {
            // Arrange
            var employeeId = Guid.NewGuid().ToString();

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/compensation/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
