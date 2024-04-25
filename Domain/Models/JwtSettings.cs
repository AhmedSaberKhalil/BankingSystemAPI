using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class JwtSettings
    {
        public string ValidIssuer { get; set; }
        public string ValiedAudiance { get; set; }
        public string Secret { get; set; }

    }
}
