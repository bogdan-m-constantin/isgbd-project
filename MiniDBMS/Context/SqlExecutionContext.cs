using MiniDBMS.Domain;
using MiniDBMS.Utils;
using Newtonsoft.Json;
using Nito.Disposables;
using Raven.Client.Documents;
using System;
using Attribute = MiniDBMS.Domain.Attribute;
using Index = MiniDBMS.Domain.Index;

namespace MiniDBMS.Context
{
    public class SqlExecutionContext{
        private static string CatalogPath = Path.Combine(Directory.GetCurrentDirectory(), "catalog" );
        private static string CatalogFileName = Path.Combine(CatalogPath, "catalog.json");
        public string? CurrentDatabase{ get; set; }
        public Catalog? Catalog { get; set; }
        public IDocumentStore Store { get; set; }
        public Database? Database => Catalog.Databases.FirstOrDefault(e => CurrentDatabase != null && e.Name == CurrentDatabase);

        public SqlExecutionContext(IDocumentStore store)
        {
            Store = store;
        }

        public void UpdateCatalog()
        {
            if (Catalog == null)
                return;

            var json = JsonConvert.SerializeObject(Catalog);
            Directory.CreateDirectory(CatalogPath);
            File.WriteAllText(CatalogFileName, json);
        }
        public void LoadCatalog()
        {
            Directory.CreateDirectory(CatalogPath);
            if (File.Exists(CatalogFileName))
            {
                var json = File.ReadAllText(CatalogFileName);
                Catalog = JsonConvert.DeserializeObject<Catalog>(json);
            }
            else
                Catalog = new Catalog();
        }

        public bool DatabaseExists(string database)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
            return Catalog.Databases.Any(e => e.Name == database);
        }

        public void AddDatabase(string databaseToCreate)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
            Catalog.Databases.Add(new Database() { Name = databaseToCreate });
            UpdateCatalog();
        }

        public bool TableExists(string tableName)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
           
            return Database!.Tables.Any(t => t.Name == tableName);
        }

        public void AddTable(Table t)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
           
            Database!.Tables.Add(t);
            UpdateCatalog();
        }

        public void DropTable(string tableName)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
           
            var table = GetTable(tableName);
            using var session = Store.OpenSession();
            table.ClearIndexes(this,session);
            session.SaveChanges();
            Database!.Tables.RemoveAt(Database!.Tables.FindIndex(t => t.Name == tableName));
            
            UpdateCatalog();
        }

        public void DropDatabase(string databaseToDrop)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
            if (CurrentDatabase == databaseToDrop)
                CurrentDatabase = null;
            using var session = Store.OpenSession();

            Catalog.Databases.First(e => e.Name == databaseToDrop).Tables.ForEach(t =>
                t.ClearIndexes(this, session));
            session.SaveChanges();
            Catalog.Databases.RemoveAt(Catalog.Databases.FindIndex(db => Database!.Name == databaseToDrop));
            UpdateCatalog();
        }

        public bool IndexExists(string indexName)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
           
            return Database!.Tables.Any(t => t.Indexes.Any(e => e.Name == indexName));
            
        }

        public void CreateIndex(string tableName, Domain.Index index)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
           
            var table = Database!.Tables.First(t => t.Name == tableName);
            table.Indexes.Add(index);
            index.CreateIndexFile(table,this);
            UpdateCatalog();
        }

        public void DropIndex(string indexName)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
           
            var table = Database!.Tables.First(t => t.Indexes.Any(e => e.Name == indexName));
            var index = table.Indexes.First(e => e.Name == indexName);
            using var session = Store.OpenSession();

            index.ClearIndex(this, session);
            session.SaveChanges();
            table.Indexes.RemoveAt(table.Indexes.FindIndex(e => e.Name == indexName));
            
            UpdateCatalog();
        }

        public bool ColumnExists(string referencedTable, string referencedColumn)
        {
            return Catalog!.Databases.First(e => e.Name == CurrentDatabase)
                .Tables.First(t => t.Name == referencedTable)
                .Attributes.Any(a => a.Name == referencedColumn);
        }

        internal Attribute GetColumn(string referencedTable, string referencedColumn)
        {
            return Catalog!.Databases.First(e => e.Name == CurrentDatabase)
                .Tables.First(t => t.Name == referencedTable)
                .Attributes.First(a => a.Name == referencedColumn);
        }

        internal Table GetTable(string table)
        {
            return Catalog!.Databases.First(e => e.Name == CurrentDatabase)
                .Tables.First(t => t.Name == table);
        }

        internal Index GetIndex(string table, string name)
        {
            return GetTable(table).Indexes.First(f => f.Name == name);
        }
    }
}
