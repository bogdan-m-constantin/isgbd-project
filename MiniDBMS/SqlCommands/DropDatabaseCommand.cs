using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class DropDatabaseCommand : SqlCommand
    {
        private string _databaseToDrop = string.Empty;
        public DropDatabaseCommand(params string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "DROP DATABASE <dabatase-name>";

        public override void Execute(SqlExecutionContext context)
        {
            if (!context.DatabaseExists(_databaseToDrop))
                throw new Exception($"Database '{_databaseToDrop}' does not exist");
            context.DropDatabase(_databaseToDrop);

        }

        public override void Parse()
        {
            if (_command.Length != 3 && _command[1].ToUpperInvariant() != "DATABASE")
                ThrowInvalidSyntaxError();
            _databaseToDrop = _command[2];
        }
    }

}
