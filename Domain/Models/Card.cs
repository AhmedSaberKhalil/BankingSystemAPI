using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
	public class Card
	{
		[Key]
		public int CardNumber { get; set; }
		[Required]
		public string Type { get; set; }
		[Required]
		public DateTime ExpiryDate { get; set; }
		[ForeignKey("Customer")]
		[Required]
		public int CustomerID { get; set; }
		public Customer Customer { get; set; }
	}
}
