using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class CreateDatabaseCommand : SqlCommand
    {
        private string _databaseToCreate = string.Empty;
        public CreateDatabaseCommand(params string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "CREATE DATABASE <dabatase-name>";

        public override void Execute(SqlExecutionContext context)
        {
            if (context.DatabaseExists(_databaseToCreate))
            {
                throw new Exception($"Database {_databaseToCreate} allready exists");
            }
            else
            {
                context.AddDatabase(_databaseToCreate);
            }
        }
        // CREATE DATABASE DBNAME
        public override void Parse()
        {
            if (_command.Length != 3 && _command[1].ToUpperInvariant() != "DATABASE")
                ThrowInvalidSyntaxError();
            _databaseToCreate = _command[2];
        }
    }

}
