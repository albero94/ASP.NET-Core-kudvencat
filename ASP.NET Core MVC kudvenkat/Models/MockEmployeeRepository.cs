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
                new Employee(){Id = 1, Name = "Mary", Email = "mary@mail.com", Department = Dept.HR},
                new Employee(){Id = 2, Name = "Steven", Email = "steven@mail.com", Department = Dept.IT},
                new Employee(){Id = 3, Name = "Sam", Email = "sam@mail.com", Department = Dept.IT}
            };
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeList;
        }

        public Employee GetEmployee(int Id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == Id);
        }
    }
}
