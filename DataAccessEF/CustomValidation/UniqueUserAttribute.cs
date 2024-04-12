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
	public class UniqueUserAttribute :ValidationAttribute
	{
		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
	{
		var dbContext = validationContext.GetService<ApplicationDbContext>();
		var userName = (string)value;

		if (dbContext.Users.Any(u => u.UserName == userName))
		{
			return new ValidationResult("User Name Is Already Found.");
		}

		return ValidationResult.Success;

	}
}
}
