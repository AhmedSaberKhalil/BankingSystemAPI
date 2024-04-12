using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
	public class Branch
	{
		[Key]
		public int BranchID { get; set; }
		[Required]
		public string BranchName { get; set; }
		[Required]
		public string Location { get; set; }
		public List<Employee> Employees { get; set; }
		public List<Customer> Customers { get; set; }
		public List<Account> Accounts { get; set; }
	}
}
