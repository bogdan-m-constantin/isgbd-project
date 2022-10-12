using Raven.Client.Documents;

namespace MiniDBMS.Context
{
    public class SqlExecutionContext{
        public string? CurrentDatabase{ get; set; }
        public IDocumentStore Store { get; set; }

        public SqlExecutionContext(IDocumentStore store)
        {
            Store = store;
        }
    }
}
