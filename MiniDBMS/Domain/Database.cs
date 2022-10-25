using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace MiniDBMS.Domain
{
    public class Database
    {
        public List<Table> Tables { get; set; } = new();
        public string Name { get; set; } = string.Empty;
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"  {Name}");
            builder.AppendLine("    Tables:");
            foreach (var table in Tables)
            {
                builder.AppendLine(table.ToString());
            }
            return builder.ToString();
        }
    }
    
}
