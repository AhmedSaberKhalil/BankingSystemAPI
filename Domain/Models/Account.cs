using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
	public class Account
	{
		[Key]
		public int AccountID { get; set; }
		[Required]
		public string Type { get; set; }
		[Required]
		public decimal Balance { get; set; }
		[ForeignKey("Customer")]
		[Required]
		public int CustomerID { get; set; }

		public Customer Customer { get; set; }
	}
}
