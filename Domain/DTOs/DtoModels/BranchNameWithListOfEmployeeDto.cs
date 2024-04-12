using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.DtoModels
{
    public class BranchNameWithListOfEmployeeDto
    {
        public int Id { get; set; }
        public string BranchName { get; set; }
        public List<string> Emp { get; set; } = new List<string>();
    }
}
