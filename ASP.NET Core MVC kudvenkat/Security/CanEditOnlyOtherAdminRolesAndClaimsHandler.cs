using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_kudvenkat.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequirements>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ManageAdminRolesAndClaimsRequirements requirement)
        {
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            if (authFilterContext == null) return Task.CompletedTask;

            string loggedInAdminId =
                context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string adminIdBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];

            if (context.User.IsInRole("Admin") &&
                context.User.HasClaim(c => c.Type == "Edit Role" && c.Value == "true") &&
                adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
