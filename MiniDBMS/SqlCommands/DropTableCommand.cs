﻿using MiniDBMS.Context;
using MiniDBMS.Domain;
using MiniDBMS.Utils;

namespace MiniDBMS.SqlCommands
{
    public class DropTableCommand : SqlCommand
    {
        private string _tableName = string.Empty;
        public DropTableCommand(params string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "DROP TABLE <table-name>";


        public override void Execute(SqlExecutionContext context)
        {
            if (context.CurrentDatabase == null)
                throw new Exception($"Database not selected. Select detabase with USE DATABASE <database-name> command");
            if (!context.TableExists(_tableName))
                throw new Exception($"The table {_tableName} does not exists.");
            var childTables = context.Database!.Tables.Where(e => e.Attributes.Any(e => e.ForeignKey?.ReferencedTable == _tableName)).Select(e => e.Name);
            if (childTables.Any()){
                throw new Exception($"Tables {childTables.Join(",")} depend on table {_tableName}.");
            }
            using var session = context.Store.OpenSession();
            var idsToDelete = session.Query<TableRow>().
                   Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{_tableName}:"))
                   .Select(e => e.Id);
            foreach (var id in idsToDelete)
            {
                session.Delete(id);
            }
            context.GetTable(_tableName).ClearIndexes(context, session);
            session.SaveChanges();
            context.DropTable(_tableName);
        }

        public override void Parse()
        {
            if (_command.Length != 3 && _command[1].ToUpperInvariant() != "TABLE")
                ThrowInvalidSyntaxError();
            _tableName = _command[2];
        }
    }

}
