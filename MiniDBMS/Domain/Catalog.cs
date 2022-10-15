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
    }
    public class Index
    {
        public List<string> Columns { get; set; } = new();
        public string Name { get; set; } = string.Empty;
    }
    
}
