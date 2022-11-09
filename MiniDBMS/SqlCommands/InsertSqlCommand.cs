using MiniDBMS.Context;
using MiniDBMS.Domain;
using MiniDBMS.Utils;
using Raven.Client;
using Raven.Client.Documents.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace MiniDBMS.SqlCommands
{
    public class InsertSqlCommand : SqlCommand
    {
        private Dictionary<string, string> _values = new();
        private string _table = "";

        public InsertSqlCommand(string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "INSERT INTO TableName (col1,col2 .. coln) VALUES (val1, val2 .. valn) ";

        public override void Execute(SqlExecutionContext context)
        {
            if (!context.TableExists(_table))
                throw new Exception($"Table {_table} does not exist");

            var table = context.GetTable(_table);
            var invalidColumns = _values.Keys.Where(e => !table.Attributes.Any(c => c.Name == e));
            if (invalidColumns.Any())
                throw new Exception($"Invalid Column names : {invalidColumns.Join(", ")}");
            if (table.Attributes.Any(e => e.PrimaryKey && !_values.ContainsKey(e.Name)))
                throw new Exception($"INSERT statement should insert values in primary key columns");
            foreach(var column in _values.Keys)
            {
                var type = table.Attributes.First(f => f.Name == column).Type;
                if (!_values[column].ValidateAs(type))
                    throw new Exception($"Column {column} should be of type {Enum.GetName(type)}");

                _values[column] = _values[column].CleanDataAs(type);
            }
            
            using var session = context.Store.OpenSession();
            (var id, var data) = table.PackData(_values);

            TableRow row = new()
            {
                Id = $"{context.CurrentDatabase}:{table.Name}:{id}",
                Values = data
            };
            if (session.Query<TableRow>().Any(r => r.Id == row.Id))
                throw new Exception("Primary Key constraint violation");
                
            session.Store(row);
            session.SaveChanges();


        }

        public override void Parse()
        {
            if (_command[1] != "INTO")
                ThrowInvalidSyntaxError();
            _table = _command[2];
            if (!_command[3].StartsWith("("))
                ThrowInvalidSyntaxError();
            var valuesIndex = _command.ToList().FindIndex(e => e == "VALUES");
            if (valuesIndex == -1)
                ThrowInvalidSyntaxError();
            var columns = _command.Skip(3).Take(valuesIndex - 3).Join(" ").Replace("(", "").Replace(")", "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var columnCount = columns.Count();

            var values = _command.Skip(valuesIndex + 1).Join(" ").Replace("(", "").Replace(")", "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var valuesCount = columns.Count();
            if(columnCount != valuesCount)
                throw new Exception("INSERT statements should have the same number of values as columns");
            for (var i = 0; i < columnCount; i++)
            {
                if (_values.ContainsKey(columns[i]))
                    throw new Exception("Cannot insert into the same column more than once");
                _values[columns[i]] = values[i];
            }
            



        }
    }
}
