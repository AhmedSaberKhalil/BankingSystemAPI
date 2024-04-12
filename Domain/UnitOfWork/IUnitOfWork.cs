using Domain.Models;
using Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.UnitOfWork
{
	public interface IUnitOfWork : IDisposable
	{
		IRepository<Account> Accounts { get; }
		//IRepository<Branch> Branchs { get; }
		IBranchRepository Branchs { get; }
		IRepository<Card> Cards { get; }
		IRepository<Customer> Customers { get; }
		IRepository<Employee> Employees { get; }
		IRepository<Loan> Loans { get; }
		IRepository<Transaction> Transactions { get; }
		IRepository<Transfer> Transfers { get; }
		int Complete();
	}
}
