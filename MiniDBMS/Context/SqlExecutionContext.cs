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

        // sterge comentariile dupa ce implementezi
        // la sfarsit la fiecare comanda trebuie apelat UpdateCatalog();

        public bool DatabaseExists(string database)
        {
            // verifici daca exista baza de date in catalog;
            throw new NotImplementedException();
        }

        public void AddDatabase(string databaseToCreate)
        {
            // adaugi baza de date in catalog
            throw new NotImplementedException();
        }

        public bool TableExists(string tableName)
        {
            // verifici daca exista tabelul tableName in baza de date CurrentDatabase
            throw new NotImplementedException();
        }

        public void AddTable(Table t)
        {
            // adaugi tabelul t in baza de date CurrentDatabase
            throw new NotImplementedException();
        }

        public void DropTable(string tableName)
        {
            // stergi tabelul tableName din baza de date CurrentDatabase
            throw new NotImplementedException();
        }

        public void DropDatabase(string databaseToDrop)
        {
            if (CurrentDatabase == databaseToDrop)
                CurrentDatabase = null;
            // stergi baza de date databaseToDrop din catalog
            throw new NotImplementedException();
        }

        public bool IndexExists(string indexName)
        {
            // verifici daca exista deja un index cu numele indexName pe oricare din tabelel din baza de date curenta
            throw new NotImplementedException();
        }

        public void CreateIndex(string tableName, Domain.Index index)
        {
            // adaugi index-ul la tabelul tableName din baza de date curenta;
            throw new NotImplementedException();
        }

        internal void DropIndex(string indexName)
        {
            // stergi index-ul baza de date curenta;
            throw new NotImplementedException();
        }
    }
}
