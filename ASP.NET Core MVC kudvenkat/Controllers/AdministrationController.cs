using ASP.NET_Core_MVC_kudvenkat.Models;
using ASP.NET_Core_MVC_kudvenkat.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_kudvenkat.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole(model.RoleName);
                IdentityResult result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded) return RedirectToAction("ListRoles", "administration");

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string Id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Rolewith Id = {Id} cannot be found";
                return View("NotFound");
            }

            EditRoleViewModel model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach (var user in await userManager.GetUsersInRoleAsync(role.Name))
            {
                model.Users.Add(user.UserName);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            IdentityRole role = await roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded) return RedirectToAction("ListRoles");

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }
    }
}
