using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
	public class Loan
	{
		[Key]
		public int LoanID { get; set; }
		[Required]
		public decimal Amount { get; set; }
		[Required]
		public decimal InterestRate { get; set; }
		[ForeignKey("Customer")]
		[Required]
		public int CustomerID { get; set; }
		public Customer Customer { get; set; }
	}
}
