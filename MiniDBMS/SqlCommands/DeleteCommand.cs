using MiniDBMS.Context;
using MiniDBMS.Domain;
using MiniDBMS.Utils;
using Raven.Client.Documents.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDBMS.SqlCommands
{
    public class DeleteCommand : SqlCommand

    {
        private string _table = "";
        private string[] _allowed = new string[] { "<", ">", "<=", ">=", "<>", "=" };
        private string? _col = "";
        private string? _val = "";
        public DeleteCommand(string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "DELETE FROM TableName [WHERE col = val ]";

        public override void Execute(SqlExecutionContext context)
        {
            if (!context.TableExists(_table))
                throw new Exception($"Table {_table} does not exists");
            var table = context.GetTable(_table);
            if (_col != null)
            {
                if (!context.ColumnExists(table.Name, _col))
                    throw new Exception($"..");
                var col = table.Attributes.First(c => c.Name == _col);
                if (_val?.ValidateAs(col.Type) != true)
                    throw new Exception();
                _val = _val.CleanDataAs(col.Type);
            }
            if (_col == null)
            {
                using var session = context.Store.OpenSession();
                var idsToDelete = session.Query<TableRow>().
                    Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{table.Name}:"))
                    .Select(e => e.Id);
                foreach (var id in idsToDelete)
                {
                    session.Delete(id);
                }
                session.SaveChanges();
            }
            else
            {
                using var session = context.Store.OpenSession();
                session.Delete($"{context.CurrentDatabase}:{table.Name}:{_val}");
                session.SaveChanges();
            }
        }

        public override void Parse()
        {
            if (_command[1] != "FROM")
                ThrowInvalidSyntaxError();
            _table = _command[2];
            if (_command.Length > 3)
            {
                if (_command.Length != 7)
                    ThrowInvalidSyntaxError();
                if (_command[3] != "WHERE")
                    ThrowInvalidSyntaxError();
                if (!_allowed.Contains(_command[5]))
                    ThrowInvalidSyntaxError();
                _col = _command[4];
                _val = _command[6];

            }


            throw new NotImplementedException();
        }
    }
}
