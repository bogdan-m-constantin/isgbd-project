using MiniDBMS.Domain;
using Newtonsoft.Json;
using Raven.Client.Documents;

namespace MiniDBMS.Context
{
    public class SqlExecutionContext{
        private static string CatalogPath = Path.Combine(Environment.CurrentDirectory, "catalog" );
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
            return Catalog.Databases.Any(e => e.Name == database);
        }

        public void AddDatabase(string databaseToCreate)
        {
            Catalog.Databases.Add(new Database() { Name = databaseToCreate });
            UpdateCatalog();
        }

        public bool TableExists(string tableName)
        {
            var db = Catalog.Databases.FirstOrDefault(e => e.Name == CurrentDatabase);
            return db.Tables.Any(t => t.Name == tableName);
        }

        public void AddTable(Table t)
        {
            var db = Catalog.Databases.First(e => e.Name == CurrentDatabase);
            db.Tables.Add(t);
            UpdateCatalog();
        }

        public void DropTable(string tableName)
        {
            var db = Catalog.Databases.First(e => e.Name == CurrentDatabase);
            db.Tables.RemoveAt(t => t.Name == tableName);
            UpdateCatalog();
        }

        public void DropDatabase(string databaseToDrop)
        {
            if (CurrentDatabase == databaseToDrop)
                CurrentDatabase = null;
            Catalog.Databases.RemoveAt(db => db.Name == databaseToDrop);
            UpdateCatalog();
        }

        public bool IndexExists(string indexName)
        {
            var db = Catalog.Database.FirstOrDefault(e => e.name = CurrentDatabase);
            return db.Tables.Any(t => t.Indexes.Any(e => e.Name == indexName));
            
        }

        public void CreateIndex(string tableName, Domain.Index index)
        {
            var db = Catalog.Database.FirstOrDefault(e => e.name = CurrentDatabase);
            var table = db.Tables.FirstOrDefault(t => t.Name == tableName);
            table.Indexes.Add(index);
            UpdateCatalog();
        }

        internal void DropIndex(string indexName)
        {
            var db = Catalog.Database.First(e => e.name == CurrentDatabase);
            var table = db.Tables.First(t => t.Indexes.Any(e => e.Name == indexName));
            table.Indexes.RemoveAt(table.Indexes.FindIndex(e => e.Name == indexName));
            UpdateCatalog();
        }
    }
}
