using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class DropIndexCommand : SqlCommand{
        public string _indexName = string.Empty;
        public DropIndexCommand(params string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "DROP INDEX <index-name>";

        public override void Execute(SqlExecutionContext context)
        {
            if (context.CurrentDatabase == null)
                throw new Exception($"Database not selected. Select detabase with USE DATABASE <database-name> command");
            if (!context.IndexExists(_indexName))
                throw new Exception($"The index {_indexName} does not exists.");
            context.DropIndex(_indexName);
        }

        public override void Parse()
        {
            if (_command.Length != 3 && _command[1].ToUpperInvariant() != "INDEX")
                ThrowInvalidSyntaxError();
            _indexName = _command[2];
        }
    }

}
