using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Domain.CustomPolicy
{
    public class EmployeeAuthorizationHandler : AuthorizationHandler<EmployeeMaleOnlyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployeeMaleOnlyRequirement requirement)
        {
            var gender = context.User.FindFirstValue("UserGender");
            if (gender == "Male")

                 context.Succeed(requirement);

            return Task.CompletedTask;
        }

         
    }
}
