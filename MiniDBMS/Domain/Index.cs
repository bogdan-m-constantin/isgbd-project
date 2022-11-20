using MiniDBMS.Utils;

namespace MiniDBMS.Domain
{
    public class Index
    {
        public List<string> Columns { get; set; } = new();
        public string Name { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"          {Name} - Columns: {Columns.Join(",")}";
        }

    }
    
}
