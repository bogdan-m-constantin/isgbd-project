using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class CreateDatabaseCommand : SqlCommand
    {
        public CreateDatabaseCommand(params string[] cmd) : base(cmd)
        {
        }

        public override void Execute(SqlExecutionContext context)
        {

        }

        public override void Parse()
        {

        }
    }

}
