using MiniDBMS.Context;

namespace MiniDBMS.SqlCommands
{
    public class DropIndexCommand : SqlCommand{
        public string _indexName = string.Empty;
        public DropIndexCommand(params string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "DROP INDEX <index-name>";

        public override void Execute(SqlExecutionContext context)
        {

        }

        public override void Parse()
        {
            if (_command.Length != 3 && _command[1].ToUpperInvariant() != "INDEX")
                ThrowInvalidSyntaxError();
            _indexName = _command[2];
        }
    }

}
