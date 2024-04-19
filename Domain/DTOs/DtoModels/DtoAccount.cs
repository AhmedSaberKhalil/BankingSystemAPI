using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.DtoModels
{
    public class DtoAccount
    {
        [Key]
        public int AccountID { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public decimal Balance { get; set; }
        [Required]
        public int CustomerID { get; set; }
    }
}
