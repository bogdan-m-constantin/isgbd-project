namespace MiniDBMS.Domain
{
    public class ForeignKey {
        public string ReferencedTable { get; set; } = string.Empty;
        public string ReferencedColumn { get; set; } = string.Empty;
        public string SourceTable { get; set; } = string.Empty;
        public string SourceColumn { get; set; } = string.Empty;

        public string Name => $"FK_{SourceTable}_{ReferencedTable}_{SourceColumn}";
    }

}
