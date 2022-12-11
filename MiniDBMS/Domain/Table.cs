using MiniDBMS.SqlCommands;
using System.Text;

namespace MiniDBMS.Domain
{
    public class Table
    {
        public string Name { get; set; } = string.Empty;
        public List<Attribute> Attributes { get; set; } = new();
        public List<Index> Indexes { get; set; } = new();
        internal IEnumerable<Index> UniqueIndexes => Indexes.Where(e => e.Unique);

        internal IEnumerable<Attribute> ForeignKeyAttributes => Attributes.Where(e => e.ForeignKey != null);
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

        public Index? GetIndexForColumn(string column)
        {
            return Indexes.FirstOrDefault(e => e.Column == column);
        }
    }
    
}
