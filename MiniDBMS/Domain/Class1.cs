using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDBMS.Domain
{
    internal class TableRow
    {
        public string Id { get; set; } = string.Empty;
        public string Values { get; set; } = string.Empty;
        
        public TableRow()
        {

        }
        public TableRow( string id, string values)
        {
            Id = id;
            Values = values;
        }
    }
}
