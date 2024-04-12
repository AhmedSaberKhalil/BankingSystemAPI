using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
	public class Customer
	{
		[Key]
		public int CustomerID { get; set; }
		[Required]
		public string Name { get; set; }

		[Required]
		public string Address { get; set; }
		[Required]
		public string Phone { get; set; }
		[Required]
		public string Email { get; set; }
	}
}
