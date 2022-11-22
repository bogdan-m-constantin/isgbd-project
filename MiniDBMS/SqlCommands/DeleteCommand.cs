using MiniDBMS.Context;
using MiniDBMS.Domain;
using MiniDBMS.Utils;
using Newtonsoft.Json.Linq;
using Raven.Client.Documents.Operations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace MiniDBMS.SqlCommands
{
    public class DeleteCommand : SqlCommand

    {
        private string _table = "";
        private string[] _allowed = new string[] { "<", ">", "<=", ">=", "<>", "=" };
        private string? _col = null;
        private string? _val = null;
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
                    throw new Exception($"Column {_col} does not exists");
                var col = table.Attributes.First(c => c.Name == _col);
                if (_val?.ValidateAs(col.Type) != true)
                    throw new Exception($"Value {_val} should be of type {Enum.GetName(col.Type)}");
                _val = _val.CleanDataAs(col.Type);
            }
            using var session = context.Store.OpenSession();

            // check foreign keys referencing this
            var childTables = context.Database!.Tables.Where(e => e.Attributes.Any(e => e.ForeignKey?.ReferencedTable == _table));
            if (_col == null)
            {
                if(childTables.Count() > 0)
                {
                    throw new Exception($"Could not truncate table. {childTables.Select(e => e.Name).Join(",")} depend on it.");
                }
                var idsToDelete = session.Query<TableRow>().
                    Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{table.Name}:"))
                    .Select(e => e.Id);
                foreach (var id in idsToDelete)
                {
                    session.Delete(id);
                }
                table.ClearIndexes( context, session);
                session.SaveChanges();
            }
            else
            {
                var id = _val!;
                foreach(var fkTable in childTables)
                {
                    var fkColumns = fkTable.ForeignKeyAttributes.Where(e => e.ForeignKey!.ReferencedTable == _table);
                    foreach(var fkColumn in fkColumns)
                    {
                        if (context.GetIndex(fkTable.Name, fkColumn.ForeignKey!.Name).CheckIndex(context, id, session))
                            throw new Exception($"Foreign key violation. Id: {id}. FK {fkColumn.ForeignKey.Name}");
                    }
                }
                session.Delete($"{context.CurrentDatabase}:{table.Name}:{id}");
                _val?.RemoveFromIndexes(table, context, session);
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
        }
    }
}
