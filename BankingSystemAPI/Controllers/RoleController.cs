using Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystemAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RoleController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> roleManager;

		public RoleController(RoleManager<IdentityRole> roleManager)
		{
			this.roleManager = roleManager;
		}
		[HttpPost("CreateRole")]
		public async Task<IActionResult> CreateRole(DtoRole roleDto)
		{
			if (ModelState.IsValid)
			{
				IdentityRole role = new IdentityRole();
				role.Name = roleDto.RoleName;
				IdentityResult result = await roleManager.CreateAsync(role);
				if (result.Succeeded)
				{
					Console.WriteLine("role name added");
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
			}
			return BadRequest(ModelState);
		}
	}
}
