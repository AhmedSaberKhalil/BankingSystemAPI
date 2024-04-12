using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.DtoAuthentication
{
	public class AuthenticateForgotPasswordDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
