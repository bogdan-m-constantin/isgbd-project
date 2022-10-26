namespace MiniDBMS.Domain
{
    public class ForeignKey {
        public string ReferencedTable { get; set; } = string.Empty;
        public string ReferencedColumn { get; set; } = string.Empty;
    }
    
}
