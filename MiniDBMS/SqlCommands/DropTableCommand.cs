using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class DropTableCommand : SqlCommand
    {
        private string _tableName = string.Empty;
        public DropTableCommand(params string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "DROP TABLE <table-name>";


        public override void Execute(SqlExecutionContext context)
        {
            if (context.CurrentDatabase == null)
                throw new Exception($"Database not selected. Select detabase with USE DATABASE <database-name> command");
            if (!context.TableExists(_tableName))
                throw new Exception($"The table {_tableName} does not exists.");
            context.DropTable(_tableName);
        }

        public override void Parse()
        {
            if (_command.Length != 3 && _command[1].ToUpperInvariant() != "TABLE")
                ThrowInvalidSyntaxError();
            _tableName = _command[2];
        }
    }

}
