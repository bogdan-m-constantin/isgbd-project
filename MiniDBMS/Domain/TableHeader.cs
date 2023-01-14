namespace MiniDBMS.Domain
{
    public class TableHeader
    {
        public string Table { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public TableHeader(string name, int width, string table)
        {
            Name = name;
            Width = width;
            Table = table;
        }
    }
}
