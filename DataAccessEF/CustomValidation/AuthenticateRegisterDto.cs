using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessEF.CustomValidation
{
	public class AuthenticateRegisterDto
	{
		[Required]
		[UniqueUser]
		public string Username { get; set; }

		[Required]
		[EmailAddress]
		[UniqueEmail]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
