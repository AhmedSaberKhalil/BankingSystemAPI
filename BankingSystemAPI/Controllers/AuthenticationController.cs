using DataAccessEF.CustomValidation;
using DataAccessEF.Data;
using Domain.DTOs.DtoAuthentication;
using Domain.EmailService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace BankingSystemAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly IConfiguration configuration;
		private readonly IEmailService emailService;
		private readonly RoleManager<IdentityRole> roleManager;

		public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.configuration = configuration;
			this.emailService = emailService;
			this.roleManager = roleManager;
		}
		[HttpPost("Register")]
		public async Task<IActionResult> Register(AuthenticateRegisterDto registerationDto)
		{
			if (ModelState.IsValid)
			{
				ApplicationUser user = new ApplicationUser();
				user.UserName = registerationDto.Username;
				user.Email = registerationDto.Email;
				IdentityResult result = await userManager.CreateAsync(user, registerationDto.Password);
				if (result.Succeeded)
				{
					
					await signInManager.SignInAsync(user, false);

					return Ok(true);
				}
				foreach (var item in result.Errors)
				{
					return BadRequest(item.Description);
				}

			}

			return BadRequest(ModelState);
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login(AuthenticateLoginDto loginDto)
		{
			if (ModelState.IsValid)
			{
				// check user name
				ApplicationUser user = await userManager.FindByNameAsync(loginDto.Username);
				if (user != null)
				{
					
					// check password
					bool found = await userManager.CheckPasswordAsync(user, loginDto.Password);
					if (found)
					{
						if (loginDto.Username == "ahmedkhalil")
						{
							await userManager.AddToRoleAsync(user, "Administrator");

						}
						else
						{
							await userManager.AddToRoleAsync(user, "User");

						}

						// Add Claims Token
						var claims = new List<Claim>();
						claims.Add(new Claim(ClaimTypes.Name, user.UserName));
						claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
						claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

						// Add Role
						// Role تبع انهي Token عشان اعرف ال

						var role = await userManager.GetRolesAsync(user);
						foreach (var itemRole in role)
						{
							claims.Add(new Claim(ClaimTypes.Role, itemRole));
						}

						// Key To Pass To signingCredentials
						SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
						// signingCredentials 
						SigningCredentials signincred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

						// create Token  in 2 steps

						// 1- represent Token
						// Json الداتا هنا بتتبعت
						JwtSecurityToken myToken = new JwtSecurityToken(
							issuer: configuration["JWT:ValidIssuer"],     // url web api (Provider)
							audience: configuration["JWT:ValiedAudiance"],  // url consumer angular
							claims: claims,
							expires: DateTime.UtcNow.AddHours(1),
							signingCredentials: signincred

							);

						// 2- Create Token
						return Ok(new
						{

							token = new JwtSecurityTokenHandler().WriteToken(myToken),
							expiration = myToken.ValidTo
						});


					}
				}
				return Unauthorized();

			}
			return Unauthorized();
		}

		[HttpPost("Change-Password")]
		public async Task<IActionResult> ChangePassword(AuthenticateChangePasswordDto changePasswordDto)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.GetUserAsync(User);
				if (user == null)
				{
					return Unauthorized();
				}
				// ChangePasswordAsync changes the user password
				var result = await userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
				if (result.Succeeded)
				{
					// Upon successfully changing the password refresh sign-in cookie
					await signInManager.RefreshSignInAsync(user);
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
				}
			}
			return BadRequest("Invalied data");
		}
		[HttpPost("Forgot-password")]
		public async Task<bool> ForgotPasswordAsync(AuthenticateForgotPasswordDto forgotPasswordDto)
		{
			ApplicationUser user = await userManager.FindByEmailAsync(forgotPasswordDto.Email);

			if (user == null)
			{
				return false; // User not found
			}
			else
			{
				var token = await userManager.GeneratePasswordResetTokenAsync(user);
				var resetLink = $"{configuration["AppBaseUrl"]}/reset-password?token={WebUtility.UrlEncode(token)}";

				var emailBody = $"Click the link to reset your password: {resetLink}";

				await emailService.SendEmailAsync(forgotPasswordDto.Email, "Reset Password", emailBody);
				return true;

			}

		}
		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] AuthenticateResetPasswordDto resetPasswordDto)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(resetPasswordDto.Email);
				if (user == null)
				{
					return Ok("Email not found");
				}
				var token = await userManager.GeneratePasswordResetTokenAsync(user);


				var result = await userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);
				if (result.Succeeded)
				{
					return Ok();
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.TryAddModelError(error.Code, error.Description);
					}
				}
			}

			return BadRequest(ModelState);
		}
		[HttpGet("Log-Out")]
		public IActionResult Logout()
		{
			return Ok(signInManager.SignOutAsync());
		}
	}
}
