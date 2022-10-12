using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class DropTableCommand : SqlCommand
    {
        public DropTableCommand(params string[] cmd) : base(cmd)
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
