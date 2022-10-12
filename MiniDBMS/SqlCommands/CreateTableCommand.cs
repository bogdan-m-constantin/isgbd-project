using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class CreateTableCommand : SqlCommand
    {
        public CreateTableCommand(params string[] cmd) : base(cmd)
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
