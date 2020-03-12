using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_kudvenkat.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>()
            {
                new Employee(){Id = 1, Name = "Mary", Email = "mary@mail.com", Department = "HR"},
                new Employee(){Id = 1, Name = "Steven", Email = "steven@mail.com", Department = "IT"},
                new Employee(){Id = 1, Name = "Sam", Email = "sam@mail.com", Department = "IT"}
            };
        }
        public Employee GetEmployee(int Id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == Id);
        }
    }
}
