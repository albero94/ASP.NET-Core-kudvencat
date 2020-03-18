using ASP.NET_Core_MVC_kudvenkat.Models;
using ASP.NET_Core_MVC_kudvenkat.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_kudvenkat.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public HomeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees();
            return View(model);
        }

        public ViewResult Details(int? id)
        {
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                PageTitle = "Employee Details",
                Employee = _employeeRepository.GetEmployee(id??1)
            };
            return View(homeDetailsViewModel);
        }

        public ViewResult Create()
        {

            return View();
        }
    }
}
