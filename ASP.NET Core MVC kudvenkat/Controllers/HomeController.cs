using ASP.NET_Core_MVC_kudvenkat.Models;
using ASP.NET_Core_MVC_kudvenkat.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_kudvenkat.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment hostingEnvironment;

        public HomeController(IEmployeeRepository employeeRepository,
            IWebHostEnvironment hostingEnvironment)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees();
            return View(model);
        }

        public ViewResult Details(int? id)
        {
            throw new Exception();
            Employee employee = _employeeRepository.GetEmployee(id ?? 1);

            if(employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNofFound", id.Value);
            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                PageTitle = "Employee Details",
                Employee = employee
            };
            return View(homeDetailsViewModel);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (!ModelState.IsValid) return View();
            string uniqueFileName = ProcessUploadedFile(model);

            Employee newEmployee = new Employee()
            {
                Name = model.Name,
                Email = model.Email,
                Department = model.Department,
                PhotoPath = uniqueFileName
            };
            _employeeRepository.Add(newEmployee);
            return RedirectToAction("details", new { id = newEmployee.Id });

        }

        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel viewModel = new EmployeeEditViewModel()
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(EmployeeEditViewModel model)
        {
            if (!ModelState.IsValid) return View();

            Employee employee = _employeeRepository.GetEmployee(model.Id);
            employee.Name = model.Name;
            employee.Email = model.Email;
            employee.Department = model.Department;

            if (model.Photo != null)
            {
                if (model.ExistingPhotoPath != null)
                {
                    string filePath = Path.Combine(hostingEnvironment.WebRootPath,
                        "images", model.ExistingPhotoPath);

                    System.IO.File.Delete(filePath);
                }

                employee.PhotoPath = ProcessUploadedFile(model);
            }

            _employeeRepository.Update(employee);
            return RedirectToAction("index");
        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photo.FileName != null)
            {
                string folderPath = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(folderPath, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
    }
}
