using MiniDBMS.Context;
using System.Security.Cryptography;

namespace MiniDBMS.SqlCommands
{
    public class UseDatabaseCommand : SqlCommand
    {
        private string DatabaseToUse = string.Empty;
        public UseDatabaseCommand(params string[] cmd) : base(cmd)
        {
        }

        public override void Execute(SqlExecutionContext context)
        {
            if (DatabaseExists(context))
            {
                context.CurrentDatabase = DatabaseToUse;
            }
            else
                throw new Exception($"Database '{DatabaseToUse}' does not exist");
        }

        private bool DatabaseExists(SqlExecutionContext context)
        {
            // should check the catalog if database exists.
            return false;
        }

        public override void Parse()
        {
            if (_command.Length != 3 && _command[1].ToUpperInvariant() != "DATABASE")
                throw new Exception("Invalid syntax. Correct syntax is USE DATABASE <dabatase-name>");
            DatabaseToUse = _command[2];
        }
    }

}
