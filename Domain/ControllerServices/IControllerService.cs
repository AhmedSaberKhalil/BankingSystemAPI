using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.DtoModels;
using Domain.Models;

namespace Domain.ControllerServices
{
    public interface IControllerService<T> where T : class
    {
      
        Task<Result<IEnumerable<T>>> GetAllAsync();
        Task<Result<T>> GetByIdAsync(int id);
        Task<Result<T>> AddAsync(T entity);
        Task<Result<bool>> UpdateAsync(int id, T entity);
        Task<Result<bool>> DeleteAsync(int id);

    }
}
