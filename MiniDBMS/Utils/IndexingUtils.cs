using MiniDBMS.Context;
using MiniDBMS.Domain;
using Newtonsoft.Json.Linq;
using Raven.Client.Documents.Session;
using Index = MiniDBMS.Domain.Index;

namespace MiniDBMS.Utils
{
    public static class IndexingUtils
    {
        public static void AddToIndexes(this Dictionary<string, object> values, Table table, SqlExecutionContext context, IDocumentSession session)
        {
            values.ToDictionary(e => e.Key, e => e.Value.ToString()!)!.AddToIndexes(table, context, session);
        }
        public static void AddToIndexes(this Dictionary<string, string> values, Table table, SqlExecutionContext context, IDocumentSession session)
        {
            foreach (var index in table.Indexes)
            {
                values.AddToIndex( table, context, session, index);
            }

        }

        private static void AddToIndex(this Dictionary<string, object> values, Table table, SqlExecutionContext context, IDocumentSession session, Index index)
        {
            values.ToDictionary(e => e.Key, e => e.Value.ToString()!)!.AddToIndex(table, context, session,index);

        }
        private static void AddToIndex(this Dictionary<string, string> values, Table table, SqlExecutionContext context, IDocumentSession session, Index index)
        {
            var data = table.PackData(index, values);
            var remoteItem = session.Load<IndexItem>($"{context.CurrentDatabase}:{index.Name}:{data.id}");
            if (remoteItem == null)
            {
                remoteItem = new IndexItem($"{context.CurrentDatabase}:{index.Name}:{data.id}", "");
            }
            remoteItem.Values += data.values + "|";
            session.Store(remoteItem);
        }

        public static void RemoveFromIndexes(this string id, Table table, SqlExecutionContext context, IDocumentSession session)
        {
            foreach (var index in table.Indexes)
            {
                id.RemoveFromIndex(context, session, index);
            }
        }

        private static void RemoveFromIndex(this string id, SqlExecutionContext context, IDocumentSession session, Index index)
        {
            var remoteItems = session.Query<IndexItem>().Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{index.Name}")).ToList().Where(e=> e.Values.Split("|", StringSplitOptions.RemoveEmptyEntries).Any(e => e == id));
            foreach (var remoteItem in remoteItems)
            {
                remoteItem.Values = remoteItem.Values.Split("|").Where(e => e != id).Join("|");
                if (remoteItem.Values.Length == 0)
                {
                    session.Delete(remoteItem.Id);
                }
                else
                { 
                    session.Store(remoteItem, remoteItem.Id);

                }
            }
        }

        public static void ClearIndexes(this Table table, SqlExecutionContext context, IDocumentSession session)
        {
            foreach (var index in table.Indexes)
            {
                index.ClearIndex(context, session);
            }
        }

        public static void ClearIndex(this Index index,SqlExecutionContext context, IDocumentSession session)
        {
            var remoteItems = session.Query<IndexItem>().Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{index.Name}"));
            foreach (var remoteItem in remoteItems)
            {
                session.Delete(remoteItem);
            }
        }

        public static void CreateIndexFile(this Index index, Table table, SqlExecutionContext context)
        {
            using var session = context.Store.OpenSession();
            var remoteItems = session.Query<TableRow>().Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{table.Name}")).ToList()
                .Select(e => e.UnpackData(table));
            foreach (var remoteItem in remoteItems)
            {
                remoteItem.AddToIndex(table, context, session, index);

            }
            session.SaveChanges();
        }
        public static bool CheckIndex(this Index index, SqlExecutionContext context, string value, IDocumentSession session)
        {
             return session.Load<IndexItem>($"{context.CurrentDatabase}:{index.Name}:{value}")?.Values?.Split("|",StringSplitOptions.RemoveEmptyEntries)?.Any() ?? false;
            
        }
    }
}
