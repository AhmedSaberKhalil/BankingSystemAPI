using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace DataAccessEF.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{

		}
		public ApplicationDbContext()
		{

		}
		public DbSet<Account> Account { get; set; }
		public DbSet<Branch> Branch { get; set; }
		public DbSet<Card> Card { get; set; }
		public DbSet<Customer> Customer { get; set; }
		public DbSet<Employee> Employee { get; set; }
		public DbSet<Loan> Loan { get; set; }
		public DbSet<Transaction> Transaction { get; set; }
		public DbSet<Transfer> Transfer { get; set; }
	}
}
