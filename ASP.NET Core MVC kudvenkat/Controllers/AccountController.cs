using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ASP.NET_Core_MVC_kudvenkat.Models;
using ASP.NET_Core_MVC_kudvenkat.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ASP.NET_Core_MVC_kudvenkat.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            if (await userManager.HasPasswordAsync(
                await userManager.GetUserAsync(User)))
                return RedirectToAction("ChangePassword");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (await userManager.HasPasswordAsync(user)) return RedirectToAction("ChangePassword");

            var result = await userManager.AddPasswordAsync(user, model.NewPassword);
            if (!result.Succeeded)
            {
                PrintModelErrors(result.Errors);
                return View();
            }

            await signInManager.RefreshSignInAsync(user);
            return View("AddPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                PrintModelErrors(result.Errors);
                return View();
            }

            await signInManager.RefreshSignInAsync(user);
            return View("ChangePasswordConfirmation");
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {userId} is invalid";
                return View("NotFound");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) return View();

            ViewBag.ErrorTitle = "Email cannot be confirmed";
            return View("Error");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                new { ReturnUrl = returnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null,
                                                                string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError("", "Error loading external login information.");
                return View("Login", loginViewModel);
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError("", "Error loading external login information");
            }

            ApplicationUser user = null;
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email != null)
            {
                user = await userManager.FindByEmailAsync(email);
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Email not confirmed yet");
                    return View("Login", loginViewModel);
                }
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                if (email != null)
                {
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await userManager.CreateAsync(user);
                        await GenerateAndLogConfirmationLink(user);
                        return RedirectToRegistrationMessageView();
                    }

                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }
            }

            ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
            ViewBag.ErrorMessage = $"Please contact support on ealbero94@gmail.com";
            return View("Error");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null && await userManager.IsEmailConfirmedAsync(user))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetLink = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token }, Request.Scheme);
                logger.LogWarning(passwordResetLink);
            }
            return View("ForgotPasswordConfirmation");
        }

        [HttpPost]
        [HttpGet]
        //[AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null) return Json(true);
            else return Json($"Email {email} is alrady in use.");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.EmailConfirmed &&
                    await userManager.CheckPasswordAsync(user, model.Password))
                {
                    ModelState.AddModelError("", "Email not confirmed yet");
                    return View(model);
                }
                var result = await signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction("index", "home");
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, City = model.City };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await GenerateAndLogConfirmationLink(user);

                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }

                    return RedirectToRegistrationMessageView();
                }

                PrintModelErrors(result.Errors);

            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (email == null || token == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) return View("ResetPasswordConfirmation");

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }
            else
            {
                PrintModelErrors(result.Errors);

                return View(model);
            }
        }

        // PRIVATE METHODS
        private async Task GenerateAndLogConfirmationLink(ApplicationUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
            logger.LogWarning(confirmationLink);
        }
        private void PrintModelErrors(IEnumerable<IdentityError> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        private IActionResult RedirectToRegistrationMessageView()
        {
            ViewBag.ErrorTitle = "Registration successful";
            ViewBag.ErrorMessage = "Beofre you can login, please confirm your email by clicking on the confirmatoin link we have emailed you";
            return View("Error");
        }

    }
}
