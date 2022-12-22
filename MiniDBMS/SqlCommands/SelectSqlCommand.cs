using MiniDBMS.Context;
using MiniDBMS.Domain;
using MiniDBMS.Domain.Exceptions;
using MiniDBMS.Utils;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDBMS.SqlCommands
{
    public class SelectSqlCommand : SqlCommand
    {
        private string _tableName = string.Empty;
        private bool _distinct = false;
        private List<string> _attributes = new();

        private string[] _allowedOperators = new string[] { "<", ">", "<=", ">=", "<>", "=", "BETWEEN" };
        public List<SelectCondition> _selectConditions = new();

        private static readonly int[] _allowedConditionLengths = new int[] { 3, 4 };
        public SelectSqlCommand(string[] cmd) : base(cmd)
        {
        }

        public override string CorrectSyntax => "SELECT [DISTINCT] {[attr 1, attr 2 ] / [*]} FROM tableName [WHERE {condition}]";

        public override void Execute(SqlExecutionContext context)
        {
            if (!context.TableExists(_tableName))
                throw new Exception($"Table {_tableName} does not exist");
            var table = context.GetTable(_tableName);
            if (_attributes.Count == 1 && _attributes[0] == "*")
                _attributes = table.Attributes.Select(e => e.Name).ToList();
            var invalidAttributes = _attributes.Where(e => !context.ColumnExists(_tableName, e));
            if (invalidAttributes.Any())
                throw new Exception($"Table {_tableName} does not contain attributes {invalidAttributes.Join(",")}");

            using var session = context.Store.OpenSession();
            try
            {
                var rows = session.Advanced.DocumentQuery<TableRow>().WhereStartsWith(e => e.Id, $"{context.CurrentDatabase}:{_tableName}:");
                foreach (var condition in _selectConditions)
                {

                    rows = rows.ApplyCondition(condition, table, context, session);
                }
                var resultRows = rows.ToList();
               
                var result = new SelectResponse(table, resultRows, _attributes.ToArray(),_distinct);
                
                Console.WriteLine(result);
                Console.WriteLine($"Total of {result.Rows.Count()}");
            }catch(InvalidOperationException e)
            {
                if (e.Message == "Sequence contains no elements")
                    Console.WriteLine("Total of 0 rows");
            }

        }
        public override void Parse()
        {
            int fromIndex = _command.ToList().IndexOf("FROM");
            if (fromIndex < 0)
                ThrowInvalidSyntaxError();
            if (_command.Length < fromIndex + 2)
                ThrowInvalidSyntaxError();
            int startIndex = 1;
            if (_command[1] == "DISTINCT")
            {
                _distinct = true;
                startIndex = 2;
            }
            _attributes = _command.Skip(startIndex).Take(fromIndex - startIndex).Join(" ").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            if (!_attributes.Any())
                ThrowInvalidSyntaxError();
            _tableName = _command[fromIndex + 1];

            int whereIndex = fromIndex + 2;
            if (whereIndex < _command.Length)
            {
                if (_command[whereIndex] != "WHERE")
                    ThrowInvalidSyntaxError();
                var conditions = _command.Skip(whereIndex + 1).Join(" ").Split("AND", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                // column [<=, <, = , <>, >, >= ] value
                // column between value1 value2 
                foreach (var condition in conditions)
                {
                    var parts = condition.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (!_allowedConditionLengths.Contains(parts.Length))
                        ThrowInvalidSyntaxError();
                    var col = parts[0];

                    if (!_allowedOperators.Contains(parts[1]))
                        throw new Exception($"Invalid operator used in {condition}");
                    var op = parts[1];
                    string value1 = "";
                    string? value2 = null;
                    value1 = parts[2];
                    if (op == "BETWEEN")
                    {
                        if (parts.Length != 4)
                            ThrowInvalidSyntaxError();
                        value2 = parts[3];
                    }
                    else if (parts.Length != 3)
                        ThrowInvalidSyntaxError();

                    _selectConditions.Add(new(col, op, value1, value2));


                }

            }
        }
    }
}
