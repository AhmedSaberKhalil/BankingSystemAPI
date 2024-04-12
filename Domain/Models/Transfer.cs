using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
	public class Transfer
	{
		[Key]
		public int TransferID { get; set; }
		[Required]
		public decimal Amount { get; set; }
		[Required]
		public DateTime Date { get; set; }
		[ForeignKey("Account")]
		[Required]
		public int AccountID { get; set; }
		public Account Account { get; set; }
	}
}
