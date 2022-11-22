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
        private string _column = string.Empty;
        private bool _unique;

        public CreateIndexCommand(params string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "CREATE INDEX index_name ON tableName.columnName [UNIQUE]";

        public override void Execute(SqlExecutionContext context)
        {
            if(context.CurrentDatabase == null)
                throw new Exception($"Database not selected. Select detabase with USE DATABASE <database-name> command");
            if (context.IndexExists(_indexName))
                throw new Exception($"There is allready an index with name {_indexName}");
            var index = new Index
            {
                Column = _column,
                Name = _indexName,
                Unique = _unique
            };
            context.CreateIndex(_tableName, index);
            index.CreateIndexFile(context.GetTable(_tableName), context);
            
        }
        
        public override void Parse()
        {
            _indexName = _command[2];
            var primaryParts = _command[4].Split(".");
            
            if (_command[3] != "ON" || primaryParts.Length != 2)
                ThrowInvalidSyntaxError();

            _tableName = primaryParts[0];
            _column = primaryParts[1];
            if (_command.Length == 6)
            {
                if (_command[5].ToLowerInvariant() == "unique")
                    _unique = true;
            }
           

        }
    }

}
