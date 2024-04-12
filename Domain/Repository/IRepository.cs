using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repository
{
	public interface IRepository<T> where T : class
	{
		IEnumerable<T> GetAll();
		T GetById(int id);
		T Find(Expression<Func<T, bool>> criteria);
		void Add(T entity);
		void Update(int id, T entity);
		void Delete(T entity);



	}
}
