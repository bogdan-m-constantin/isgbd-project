namespace MiniDBMS.Domain
{
    public class Database
    {
        public List<Table> Tables { get; set; } = new();
        public string Name { get;  set; } = string.Empty
    }
    
}
