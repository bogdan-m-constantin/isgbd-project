namespace MiniDBMS.Domain
{
    public class TableHeader
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public TableHeader(string name, int width)
        {
            Name = name;
            Width = width;
        }
    }
}
