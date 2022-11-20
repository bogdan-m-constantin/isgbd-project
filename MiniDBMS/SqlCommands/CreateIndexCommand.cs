using MiniDBMS.Context;
using MiniDBMS.Domain;
using MiniDBMS.Utils;
using System.Security.Cryptography;
using Index = MiniDBMS.Domain.Index;

namespace MiniDBMS.SqlCommands
{
    public class CreateIndexCommand : SqlCommand
    {
        private string _indexName = string.Empty;
        private string _tableName = string.Empty;
        private List<string> _columns = new();
        public CreateIndexCommand(params string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "CREATE INDEX index_name ON tableName.columnName";

        public override void Execute(SqlExecutionContext context)
        {
            if(context.CurrentDatabase == null)
                throw new Exception($"Database not selected. Select detabase with USE DATABASE <database-name> command");
            if (context.IndexExists(_indexName))
                throw new Exception($"There is allready an index with name {_indexName}");
            context.CreateIndex(_tableName, new Index
            {
                Columns = _columns,
                Name = _indexName
            });
        }
        
        public override void Parse()
        {
            _indexName = _command[2];
            var primaryParts = _command[4].Split(".");
            
            if (_command[3] != "ON" || primaryParts.Length != 2)
                ThrowInvalidSyntaxError();

            _tableName = primaryParts[0];
            _columns.Add(primaryParts[1]);

           

        }
    }

}
