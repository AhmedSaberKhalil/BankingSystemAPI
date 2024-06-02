using DataAccessEF.CustomValidation;
using DataAccessEF.Data;
using Domain.DTOs.DtoAuthentication;
using Domain.EmailService;
using Domain.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Domain.DTOs;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BankingSystemAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly IConfiguration configuration;
        private readonly IOptions<JwtSettings> _options;
        private readonly IEmailService emailService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly RoleManager<IdentityRole> roleManager;

		public AuthenticationController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,IOptions<JwtSettings> options ,
            RoleManager<IdentityRole> roleManager, IEmailService emailService, ILogger<AuthenticationController> logger)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.configuration = configuration;
            this._options = options;
            this.emailService = emailService;
            this._logger = logger;
            this.roleManager = roleManager;
		}

        [HttpPost("Register")]
        public async Task<IActionResult> Register(AuthenticateRegisterDto registrationDto)
        {
            Log.Information("User registration attempt for username: {Username}, email: {Email}", registrationDto.Username, registrationDto.Email);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for user registration: {@Errors}", errors);
                return BadRequest(ModelState);
            }

            ApplicationUser user = new ApplicationUser
            {
                UserName = registrationDto.Username,
                Email = registrationDto.Email
            };

            try
            {
                IdentityResult result = await userManager.CreateAsync(user, registrationDto.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmEmailUrl = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token = token }, Request.Scheme);

                    await emailService.SendEmailAsync(user.Email, "Confirm your email",
                        $"Please confirm your account by clicking <a href=\"{confirmEmailUrl}\">here</a>.");

                    await signInManager.SignInAsync(user, false);

                    _logger.LogInformation("Email confirmation sent to user ID: {UserId}", user.Id);
                    return Ok(true);
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        _logger.LogWarning("Error occurred during user creation for username: {Username}, email: {Email}. Error: {Error}", registrationDto.Username, registrationDto.Email, item.Description);
                    }
                    return BadRequest(result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during user registration for username: {Username}, email: {Email}", registrationDto.Username, registrationDto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error. Please try again later.");
            }
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("User Id and Token are required");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                return Ok("Email confirmed successfully");

            return BadRequest("Error confirming email");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(AuthenticateLoginDto loginDto)
        {
            _logger.LogInformation("User login attempt for username: {Username}", loginDto.Username);

            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByNameAsync(loginDto.Username);

                if (user != null)
                {
                    bool found = await userManager.CheckPasswordAsync(user, loginDto.Password);

                    if (found)
                    {
                        try
                        {
                            if (loginDto.Username == "ahmedkhalil")
                            {
                                await userManager.AddToRoleAsync(user, "Administrator");
                                _logger.LogInformation("User {Username} added to role Administrator", loginDto.Username);
                            }
                            else
                            {
                                await userManager.AddToRoleAsync(user, "User");
                                _logger.LogInformation("User {Username} added to role User", loginDto.Username);
                            }

                            var accessToken = await GenerateJwtToken(user);
                            _logger.LogInformation("User {Username} login successful", loginDto.Username);

                            return Ok(accessToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An unexpected error occurred during the login process for username: {Username}", loginDto.Username);
                            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error. Please try again later.");
                        }
                    }
                }

                _logger.LogWarning("Unauthorized login attempt for username: {Username}", loginDto.Username);
                return Unauthorized();
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Invalid model state for login attempt: {@Errors}", errors);

            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            _logger.LogInformation("Generating JWT token for user ID: {UserId}, username: {Username}", user.Id, user.UserName);

            try
            {
                // Define token lifetime
                var tokenLifetime = TimeSpan.FromMinutes(30);

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = _options.Value.ValidIssuer,
                    Audience = _options.Value.ValiedAudiance,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Secret)), SecurityAlgorithms.HmacSha256),
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserType", "Employee"),
                new Claim("UserGender", "Male")
                    }),
                    Expires = DateTime.UtcNow.Add(tokenLifetime) // Set token expiration time
                };

                // Add user roles as claims
                var userRoles = await userManager.GetRolesAsync(user);
                foreach (var role in userRoles)
                {
                    tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var accessToken = tokenHandler.WriteToken(securityToken);

                _logger.LogInformation("JWT token generated successfully for user ID: {UserId}, username: {Username}", user.Id, user.UserName);

                // Generate and set password reset token
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.SetAuthenticationTokenAsync(user, "ResetPassword", "ResetPasswordToken", resetToken);

                _logger.LogInformation("Password reset token generated and set for user ID: {UserId}, username: {Username}", user.Id, user.UserName);

                // Save user info to AspNetUserLogins
                await userManager.AddLoginAsync(user, new UserLoginInfo("Identity", user.Id, user.UserName));

                _logger.LogInformation("User login info saved for user ID: {UserId}, username: {Username}", user.Id, user.UserName);

                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating JWT token for user ID: {UserId}, username: {Username}", user.Id, user.UserName);
                throw; // Rethrow the exception to be handled by the calling code
            }
        }
 
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            _logger.LogInformation("Refresh token attempt.");

            var principal = GetPrincipalFromExpiredToken(model.Token);
            if (principal == null)
            {
                _logger.LogWarning("Invalid token provided for refresh.");
                return BadRequest("Invalid token");
            }

            var user = await userManager.FindByNameAsync(principal.Identity.Name);
            if (user == null)
            {
                _logger.LogWarning("User not found for the given token.");
                return BadRequest("Invalid token");
            }

            try
            {
                var newToken = await GenerateJwtToken(user);
                _logger.LogInformation("Token refreshed successfully for user ID: {UserId}, username: {Username}", user.Id, user.UserName);
                return Ok(new { token = newToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing the token for user ID: {UserId}, username: {Username}", user.Id, user.UserName);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error. Please try again later.");
            }
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            _logger.LogInformation("Getting principal from expired token.");

            try
            {
                var jwtSettings = configuration.GetSection("JWT");
                var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = false // We don't care about the token's expiration date
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token provided.");
                    throw new SecurityTokenException("Invalid token");
                }

                _logger.LogInformation("Principal successfully retrieved from expired token.");
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving principal from expired token.");
                throw;
            }
        }
 
        [HttpPost("update-userName")]
        public async Task<ActionResult> UpdateUser(UpdateUserDto userDto)
        {
            _logger.LogInformation("UpdateUser request received for old username: {OldUserName}", userDto.oldUserName);

            if (ModelState.IsValid)
            {
                try
                {
                    ApplicationUser user = await userManager.FindByNameAsync(userDto.oldUserName);
                    if (user == null)
                    {
                        _logger.LogWarning("User not found for old username: {OldUserName}", userDto.oldUserName);
                        return Unauthorized();
                    }
                    else
                    {
                        if (user.UserName != userDto.NewUserName && user.UserName == userDto.oldUserName)
                        {
                            user.UserName = userDto.NewUserName;
                        }
                        else
                        {
                            _logger.LogWarning("Invalid user name change attempt from {OldUserName} to {NewUserName}", userDto.oldUserName, userDto.NewUserName);
                            return BadRequest("Invalid user name change attempt.");
                        }

                        var result = await userManager.UpdateAsync(user);
                        if (!result.Succeeded)
                        {
                            foreach (var item in result.Errors)
                            {
                                _logger.LogWarning("Error updating user: {ErrorDescription}", item.Description);
                                return BadRequest(item.Description);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("User name updated successfully from {OldUserName} to {NewUserName}", userDto.oldUserName, userDto.NewUserName);
                            return Ok("Update User Name Successfully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating user name for {OldUserName}", userDto.oldUserName);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error. Please try again later.");
                }
            }
            return Unauthorized();
        }

        [HttpGet("userinfo")]
        public IActionResult GetUserInfo()
        {
            _logger.LogInformation("GetUserInfo request received.");

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                try
                {
                    var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    var username = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                    var email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

                    _logger.LogInformation("User info retrieved for user ID: {UserId}, username: {Username}", userId, username);
                    return Ok(new
                    {
                        UserId = userId,
                        Username = username,
                        Email = email
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving user info.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error. Please try again later.");
                }
            }

            _logger.LogWarning("Unauthorized user info request.");
            return Unauthorized();
        }


        [HttpPost("Change-Password")]
        public async Task<IActionResult> ChangePassword(AuthenticateChangePasswordDto changePasswordDto)
        {
            _logger.LogInformation("ChangePassword request received.");

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        _logger.LogWarning("User not found.");
                        return Unauthorized();
                    }

                    var result = await userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
                    if (result.Succeeded)
                    {
                        await signInManager.RefreshSignInAsync(user);
                        _logger.LogInformation("Password changed successfully for user ID: {UserId}", user.Id);
                        return Ok("Password changed successfully.");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogWarning("Error changing password: {ErrorDescription}", error.Description);
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return BadRequest(ModelState);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while changing password.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error. Please try again later.");
                }
            }

            _logger.LogWarning("Invalid model state for ChangePassword request.");
            return BadRequest("Invalid data");
        }
  
        [HttpPost("Forgot-password")]
        public async Task<bool> ForgotPasswordAsync(AuthenticateForgotPasswordDto forgotPasswordDto)
        {
            _logger.LogInformation("ForgotPassword request received for email: {Email}", forgotPasswordDto.Email);

            try
            {
                ApplicationUser user = await userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", forgotPasswordDto.Email);
                    return false; // User not found
                }
                else
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var resetLink = $"{configuration["AppBaseUrl"]}/reset-password?token={WebUtility.UrlEncode(token)}";

                    var emailBody = $"Click the link to reset your password: {resetLink}";

                    await emailService.SendEmailAsync(forgotPasswordDto.Email, "Reset Password", emailBody);
                    _logger.LogInformation("Password reset email sent for email: {Email}", forgotPasswordDto.Email);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing ForgotPassword request for email: {Email}", forgotPasswordDto.Email);
                return false;
            }
        }
   
      
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] AuthenticateResetPasswordDto resetPasswordDto)
        {
            _logger.LogInformation("ResetPassword request received for email: {Email}", resetPasswordDto.Email);

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await userManager.FindByEmailAsync(resetPasswordDto.Email);
                    if (user == null)
                    {
                        _logger.LogWarning("User not found for email: {Email}", resetPasswordDto.Email);
                        return Ok("Email not found");
                    }

                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Password reset successfully for email: {Email}", resetPasswordDto.Email);
                        return Ok();
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogWarning("Error resetting password: {ErrorDescription}", error.Description);
                            ModelState.TryAddModelError(error.Code, error.Description);
                        }
                        return BadRequest(ModelState);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while resetting password for email: {Email}", resetPasswordDto.Email);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error. Please try again later.");
                }
            }

            _logger.LogWarning("Invalid model state for ResetPassword request.");
            return BadRequest(ModelState);
        }
        [HttpGet("Log-Out")]
        public IActionResult Logout()
        {
            _logger.LogInformation("Logout request received.");

            try
            {
                signInManager.SignOutAsync();
                _logger.LogInformation("User logged out successfully.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging out.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error. Please try again later.");
            }
        }
    }
}
