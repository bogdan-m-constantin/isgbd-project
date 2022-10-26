namespace MiniDBMS.Domain
{
    public class Attribute
    {
        public string Name { get; set; } = string.Empty;
        public DataType Type { get; set; }
        public bool PrimaryKey { get; set; }
        public ForeignKey? ForeignKey { get; set; }
        public override string ToString()
        { 
            return $"          {Name} {Enum.GetName(Type)} {(PrimaryKey ? "PK " : "")} {(ForeignKey != null ? $"REFERENCES {ForeignKey.ReferencedTable}.{ForeignKey.ReferencedColumn}" : "")}";
        }
    }
    
}
