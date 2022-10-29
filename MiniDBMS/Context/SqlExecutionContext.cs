using MiniDBMS.Domain;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Attribute = MiniDBMS.Domain.Attribute;

namespace MiniDBMS.Context
{
    public class SqlExecutionContext{
        private static string CatalogPath = Path.Combine(Directory.GetCurrentDirectory(), "catalog" );
        private static string CatalogFileName = Path.Combine(CatalogPath, "catalog.json");
        public string? CurrentDatabase{ get; set; }
        public Catalog? Catalog { get; set; }
        public IDocumentStore Store { get; set; }

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
            var db = Catalog.Databases.First(e => e.Name == CurrentDatabase);
            return db.Tables.Any(t => t.Name == tableName);
        }

        public void AddTable(Table t)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
            var db = Catalog.Databases.First(e => e.Name == CurrentDatabase);
            db.Tables.Add(t);
            UpdateCatalog();
        }

        public void DropTable(string tableName)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
            var db = Catalog.Databases.First(e => e.Name == CurrentDatabase);
            db.Tables.RemoveAt(db.Tables.FindIndex(t => t.Name == tableName));
            UpdateCatalog();
        }

        public void DropDatabase(string databaseToDrop)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
            if (CurrentDatabase == databaseToDrop)
                CurrentDatabase = null;
            Catalog.Databases.RemoveAt(Catalog.Databases.FindIndex(db => db.Name == databaseToDrop));
            UpdateCatalog();
        }

        public bool IndexExists(string indexName)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
            var db = Catalog.Databases.First(e => e.Name == CurrentDatabase);
            return db.Tables.Any(t => t.Indexes.Any(e => e.Name == indexName));
            
        }

        public void CreateIndex(string tableName, Domain.Index index)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
            var db = Catalog.Databases.First(e => e.Name == CurrentDatabase);
            var table = db.Tables.First(t => t.Name == tableName);
            table.Indexes.Add(index);
            UpdateCatalog();
        }

        public void DropIndex(string indexName)
        {
            if (Catalog == null) throw new Exception("InvalidCatalog");
            var db = Catalog.Databases.First(e => e.Name == CurrentDatabase);
            var table = db.Tables.First(t => t.Indexes.Any(e => e.Name == indexName));
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
    }
}
