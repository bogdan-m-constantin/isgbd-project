using MiniDBMS.Context;
using System.Security.Cryptography;

namespace MiniDBMS.SqlCommands
{
    public class UseDatabaseCommand : SqlCommand
    {
        private string _databaseToUse = string.Empty;
        public UseDatabaseCommand(params string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "USE DATABASE <dabatase-name>";

        public override void Execute(SqlExecutionContext context)
        {
            if (context.DatabaseExists(_databaseToUse))
            {
                context.CurrentDatabase = _databaseToUse;
            }
            else
                throw new Exception($"Database '{_databaseToUse}' does not exist");
        }


        public override void Parse()
        {
            if (_command.Length != 3 && _command[1].ToUpperInvariant() != "DATABASE")
                throw new Exception("Invalid syntax. Correct syntax is USE DATABASE <dabatase-name>");
            _databaseToUse = _command[2];
        }
    }

}
