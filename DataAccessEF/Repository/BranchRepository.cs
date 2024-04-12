using DataAccessEF.Data;
using Domain.Models;
using Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessEF.Repository
{
	public class BranchRepository : Repository<Branch>, IBranchRepository
	{
		private readonly ApplicationDbContext _context;

		public BranchRepository(ApplicationDbContext context) : base(context) { }
        public Branch GetBranchNameWithEmployee(int branchId)
		{
			return _context.Branch.Include(e => e.Employees).FirstOrDefault(b => b.BranchID == branchId);
		}
	}
}
