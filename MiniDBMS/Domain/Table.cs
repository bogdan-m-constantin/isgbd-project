using MiniDBMS.SqlCommands;
using System.Text;

namespace MiniDBMS.Domain
{
    public class Table
    {
        public string Name { get; set; } = string.Empty;
        public List<Attribute> Attributes { get; set; } = new();
        public List<Index> Indexes { get; set; } = new();

        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"      {Name}");
            sb.AppendLine($"        Columns:");
            foreach (var attr in Attributes)
            {
                sb.AppendLine(attr.ToString());
            }
            sb.AppendLine($"        Indexes:");
            foreach (var index in Indexes)
            {
                sb.AppendLine(index.ToString());
            }
            return sb.ToString();
        }
    }
    
}
