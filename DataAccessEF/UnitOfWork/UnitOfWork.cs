using DataAccessEF.Data;
using DataAccessEF.Repository;
using Domain.Models;
using Domain.Repository;
using Domain.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace DataAccessEF.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _context;
		public IRepository<Account> Accounts { get; private set; }
		//	public IRepository<Branch> Branchs { get; private set; }
		public IBranchRepository Branchs { get; private set; }
		public IRepository<Card> Cards { get; private set; }
		public IRepository<Customer> Customers { get; private set; }
		public IRepository<Employee> Employees { get; private set; }
		public IRepository<Loan> Loans { get; }
		public IRepository<Transaction> Transactions { get; private set; }
		public IRepository<Transfer> Transfers { get; private set; }


		public UnitOfWork(ApplicationDbContext context)
		{
			this._context = context;
			Accounts = new Repository<Account>(_context);
			//Branchs = new Repository<Branch>(_context);
			Branchs = new BranchRepository(_context);
			Cards = new Repository<Card>(_context);
			Customers = new Repository<Customer>(_context);
			Employees = new Repository<Employee>(_context);
			Loans = new Repository<Loan>(_context);
			Transactions = new Repository<Transaction>(_context);
			Transfers = new Repository<Transfer>(_context);
		}

		public int Complete()
		{
			return _context.SaveChanges();
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
