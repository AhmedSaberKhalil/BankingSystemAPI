using DataAccessEF.Data;
using Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessEF.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _context;

		public Repository(ApplicationDbContext context)
		{
			this._context = context;
		}
		T IRepository<T>.Find(Expression<Func<T, bool>> criteria)
		{
			return _context.Set<T>().SingleOrDefault(criteria);
		}
			public T Add(T entity)
			{
				var t = _context.Set<T>().Add(entity);
				if (t != null)
				{
					return (T)entity;  // Return the added entity
				}
				else
				{
					throw new ArgumentException("can not save Data");  // Throw exception
				}
			}

        public void Delete(T entity)
		{
			_context.Set<T>().Remove(entity);
		}

		public IEnumerable<T> GetAll()
		{
			return _context.Set<T>().ToList();
		}

		public T GetById(int id)
		{
			var entity = _context.Set<T>().Find(id);
			if (entity == null)
				 throw new ArgumentNullException($"Id Number Is Not Found {id}"); 
			return entity;
		}

		public T Update(int id, T entity)
		{
			
            var t = _context.Set<T>().Update(entity);
            if (t != null)
            {
                return (T)entity; 
            }
            else
            {
                throw new ArgumentException("can not save Data");  // Throw exception
            }
        }

		
	}
}
