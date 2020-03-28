using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_kudvenkat.Models
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                    new Employee
                    {
                        Id = 1,
                        Name = "Mark",
                        Department = Dept.IT,
                        Email = "mark@email.com"
                    },
                    new Employee
                    {
                        Id = 2,
                        Name = "Mary",
                        Department = Dept.IT,
                        Email = "mary@email.com"
                    },
                    new Employee
                    {
                        Id = 3,
                        Name = "John",
                        Department = Dept.HR,
                        Email = "john@email.com"
                    }
                );
        }
    }
}
