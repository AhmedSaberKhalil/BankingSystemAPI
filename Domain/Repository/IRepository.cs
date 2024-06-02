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
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> FindAsync(Expression<Func<T, bool>> criteria);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(int id, T entity);
        Task DeleteAsync(T entity);
        //IEnumerable<T> GetAll();
        //T GetById(int id);
        //T Find(Expression<Func<T, bool>> criteria);
        //T Add(T entity);
        //T Update(int id, T entity);
        //void Delete(T entity);



    }
}
