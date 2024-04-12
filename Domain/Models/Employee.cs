using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
	public class Employee
	{
		[Key]
		public int EmployeeID { get; set; }
		[Required]
		public string Name { get; set; }
		[Required]
		public string Position { get; set; }
		[ForeignKey("Branch")]
		[Required]
		public int BranchID { get; set; }
		public Branch Branch { get; set; }
	}
}
