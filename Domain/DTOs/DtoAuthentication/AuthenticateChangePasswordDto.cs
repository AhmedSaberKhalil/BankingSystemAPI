using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.DtoAuthentication
{
	public class AuthenticateChangePasswordDto
	{
		[Required]
		public string OldPassword { get; set; }
		[Required]
		public string NewPassword { get; set; }
		[Required]
		[Compare("NewPassword")]
		public string ConfirmPassword { get; set; }
	}
}
