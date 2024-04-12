using DataAccessEF.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessEF.CustomValidation
{
	public class UniqueEmailAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			var dbContext = validationContext.GetService<ApplicationDbContext>();
			var email = (string)value;

			if (dbContext.Users.Any(u => u.Email == email))
			{
				return new ValidationResult("Email is already in use.");
			}

			return ValidationResult.Success;

		}

	}
}
