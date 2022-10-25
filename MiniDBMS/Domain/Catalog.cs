using MiniDBMS.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDBMS.Domain
{
    public class Catalog 
    {
        public List<Database> Databases { get; set; } = new();
        public override string ToString()
        {
            return $"Catalog State: \r\n{Databases.Select(d=>d.ToString()).Join(Environment.NewLine )}";
        }
    }
    
}
