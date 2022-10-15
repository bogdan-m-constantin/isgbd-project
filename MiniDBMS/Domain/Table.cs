namespace MiniDBMS.Domain
{
    public class Table
    {
        public string Name { get; set; } = string.Empty;
        public List<Attribute> Attributes { get; set; } = new();
        public List<Index> Indexes { get; set; } = new();
    }
    
}
