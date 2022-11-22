using MiniDBMS.Utils;

namespace MiniDBMS.Domain
{
    public class Index
    {
        public string Column { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Unique { get; set; }

        public override string ToString()
        {
            return $"          {Name} {(Unique ? "UNIQUE": "NON-UNIQUE")} - Column: {Column}";
        }

    }
    
}
