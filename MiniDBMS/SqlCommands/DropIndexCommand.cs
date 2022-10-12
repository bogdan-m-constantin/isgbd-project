using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class DropIndexCommand : SqlCommand{ 
        public DropIndexCommand(params string[] cmd) : base(cmd)
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
