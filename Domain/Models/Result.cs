using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }

        public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
    }

}
