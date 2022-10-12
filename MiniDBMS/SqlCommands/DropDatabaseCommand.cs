using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class DropDatabaseCommand : SqlCommand
    {
        public DropDatabaseCommand(params string[] cmd) : base(cmd)
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
