using MiniDBMS.Context;
using MiniDBMS.Domain;
using MiniDBMS.Domain.Exceptions;
using MiniDBMS.Utils;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace MiniDBMS.SqlCommands
{
    public class Join
    {
        public string Table { get; set; }
        public string Column { get; set; }
        public string ParentTable { get; set; }
        public string ParentColumn { get; set; }
    }
    public class SelectSqlCommand : SqlCommand
    {
        private string _tableName = string.Empty;
        private bool _distinct = false;
        private List<SelectAttribute> _attributes = new();
        private List<string> _attributeNames = new();

        private string[] _allowedOperators = new string[] { "<", ">", "<=", ">=", "<>", "=", "BETWEEN" };
        public List<SelectCondition> _selectConditions = new();
        public List<Join> _joins = new List<Join>();
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
            var allTables = _joins.ToDictionary(e => e.Table, e => context.GetTable(e.Table));
            allTables[table.Name] = table;
            if (_attributeNames.Count == 1 && _attributeNames[0] == "*")
                _attributes = allTables.SelectMany(t => t.Value.Attributes.Select(e => new SelectAttribute { Table = t.Value.Name, Attribute = e.Name })).ToList();
            else
                foreach (var attr in _attributeNames)
                {
                    string[] parts = attr.Split(".");
                    if (parts.Length == 1)
                    {
                        var attrs = allTables.SelectMany(e => e.Value.Attributes.Where(a => a.Name == parts[0]).Select(a => new SelectAttribute() { Table = e.Key, Attribute = a.Name }));
                        var count = attrs.Count();
                        if (count == 0)
                            throw new Exception($"Attribute {parts[0]} does not exist");
                        else if (count == 1)
                            _attributes.Add(attrs.First());
                        else
                            throw new Exception($"Definition of attr {parts[0]} is ambiguous");
                    }
                    else if (parts.Length == 2)
                    {
                        if (allTables.ContainsKey(parts[0]))
                            if (allTables[parts[0]].Attributes.Any(e => e.Name == parts[1]))
                                _attributes.Add(new() { Attribute = parts[1], Table = parts[0] });
                            else
                                throw new Exception($"Invalid argument {parts[1]} on table {parts[0]}");
                        else
                            throw new Exception($"Invalid table {parts[0]}");


                    }
                    else
                        throw new Exception($"Invalid argument {attr}");
                }
            using var session = context.Store.OpenSession();
            try
            {
                var joinResult = PerformJoins(context, session, allTables);
                foreach (var condition in _selectConditions)
                {
                    joinResult = joinResult.ApplyCondition(condition, table, context, session);
                }

                var result = new SelectResponse(allTables, joinResult, _attributes, _distinct);

                Console.WriteLine(result);
                Console.WriteLine($"Total of {result.Result.Rows.Count()}");
            }
            catch (InvalidOperationException e)
            {
                if (e.Message == "Sequence contains no matching elements")
                    Console.WriteLine("Total of 0 rows");
            }

        }

        private JoinResult PerformJoins(SqlExecutionContext context, IDocumentSession session, Dictionary<string, Table> allTables)
        {
            var rows = session.Advanced.DocumentQuery<TableRow>().WhereStartsWith(e => e.Id, $"{context.CurrentDatabase}:{_tableName}:").ToList().Cast<TableRow>();
            List<string> presentTables = new List<string>
            {
                _tableName
            };
            var result = new JoinResult
            {
                Rows = rows.Select(e => new Dictionary<string, TableRow> { { _tableName, e } }).ToList(),
            };
            foreach(var join in _joins) { 
                result = PerformJoin(context,session,allTables,join, result, presentTables);
            }             
            return result;

        }

        private JoinResult PerformJoin(SqlExecutionContext context, IDocumentSession session, Dictionary<string, Table> allTables, Join join, JoinResult result, List<string> presentTables)
        {
            if (presentTables.Contains(join.ParentTable))
            {                
                presentTables.Add(join.Table);
                //return PerformJoinNestedLoops(context, session, allTables, join,result);
                return PerformJoinHash(context, session, allTables, join, result);


            }
            throw new Exception($"{join.ParentTable} does not exist in current dataset");
            
        }

        private JoinResult PerformJoinHash(SqlExecutionContext context, IDocumentSession session, Dictionary<string, Table> allTables, Join join, JoinResult result)
        {
            var hashMap = GenerateHashMap(context,session,allTables,join);
            var res = new JoinResult
            {
                Rows = new()
            };
            foreach(var row in result.Rows)
            {
                var valueToCheck = row[join.ParentTable].UnpackData(allTables[join.ParentTable])[join.ParentColumn].ToString() ?? "NULL" ;
                if (hashMap.ContainsKey(valueToCheck)){
                    foreach(var r in hashMap[valueToCheck])
                    {
                        var dict = row.ToDictionary(e => e.Key, e => e.Value);
                        dict[join.Table] = r;
                        res.Rows.Add(dict);
                    }
                }
            }
            return res;
        }

        private Dictionary<string, List<TableRow>> GenerateHashMap(SqlExecutionContext context, IDocumentSession session, Dictionary<string, Table> allTables, Join join)
        {
            var table = allTables[join.Table];
            var index = table.GetIndexForColumn(join.Column);
            if(index == null)
            {
                return GenerateHashMapWithoutIndex(context, session, allTables, join);
            }
            Dictionary<string, List<TableRow>> map = new();
            var items = session.Advanced.DocumentQuery<IndexItem>().WhereStartsWith(e => e.Id, $"{context.CurrentDatabase}:{index.Name}:").ToList().Cast<IndexItem>();
            foreach(var item in items)
            {
                var ids = item.Values.Split("|", StringSplitOptions.RemoveEmptyEntries).Select(e => $"{context.CurrentDatabase}:{join.Table}:{e}");
                var tableRows = session.Advanced.DocumentQuery<TableRow>().WhereIn(e=>e.Id, ids).ToList();
                map[tableRows[0].UnpackData(table)[join.Column].ToString() ?? "NULL"] = tableRows;
            }

            return map;

        }

        private Dictionary<string, List<TableRow>> GenerateHashMapWithoutIndex(SqlExecutionContext context, IDocumentSession session, Dictionary<string, Table> allTables, Join join)
        {
            var rows = session.Advanced.DocumentQuery<TableRow>().WhereStartsWith(e => e.Id, $"{context.CurrentDatabase}:{join.Table}:").ToList().Cast<TableRow>();
            Dictionary<string, List<TableRow>> map = new();
            foreach(var row in rows)
            {
                var val = row.UnpackData(allTables[join.Table])[join.Column].ToString() ?? "NULL";
                if (!map.ContainsKey(val))
                    map[val] = new();
                map[val].Add(row);
            }
            return map;
        }

        private JoinResult PerformJoinNestedLoops(SqlExecutionContext context, IDocumentSession session, Dictionary<string, Table> allTables, Join join, JoinResult result)
        {
            var res = new JoinResult
            {
                Rows = new()
            };
            var rowsToJoin = session.Advanced.DocumentQuery<TableRow>().WhereStartsWith(e => e.Id, $"{context.CurrentDatabase}:{join.Table}:").ToList().Cast<TableRow>();
            foreach(var a in result.Rows)
            {
                var valueToCheck = a[join.ParentTable].UnpackData(allTables[join.ParentTable])[join.ParentColumn];
                foreach(var b in rowsToJoin)
                {
                    var val = b.UnpackData(allTables[join.Table])[join.Column];
                    if (SelectUtils.CompareAs(context.GetColumn(join.Table,join.Column).Type, val.ToString(),valueToCheck.ToString()) == 0)
                    {
                        var dict = a.ToDictionary(e => e.Key, e => e.Value);
                        dict[join.Table] = b;
                        res.Rows.Add(dict);
                    } 
                }
            }
            return res;
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
            _attributeNames = _command.Skip(startIndex).Take(fromIndex - startIndex).Join(" ").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            if (!_attributeNames.Any())
                ThrowInvalidSyntaxError();
            _tableName = _command[fromIndex + 1];
            int whereIndex = _command.ToList().IndexOf("WHERE");
            int fromEnd = fromIndex + 1;
            ParseJoins(fromEnd, whereIndex);
            if (whereIndex < _command.Length)
            {
                if (whereIndex == -1)
                    return;
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

        private void ParseJoins(int fromEnd, int whereIndex)
        {
            if(whereIndex == -1 )
                whereIndex = _command.Length;
            if (fromEnd >= whereIndex)
                return;
            var joinPart = _command.Skip(fromEnd + 1).Take(whereIndex - fromEnd).ToList();
            // JOIN TABLE ON TABLE1.COLUMN1 = TABLE2.COLUMN2
            for(int i = 0; i < joinPart.Count; i+= 6)
            {
                if (joinPart[i] == "JOIN")
                {
                    var table = joinPart[i + 1];
                    if (joinPart[i+2] == "ON")
                    {
                        var firstParts = joinPart[i + 3].Split(".");
                        var secondParts = joinPart[i + 5].Split(".");
                        if (firstParts.Length == 2 && secondParts.Length == 2)
                        {
                            _joins.Add(new Join
                            {
                                Table = table,
                                Column = firstParts[0] == table ? firstParts[1] : secondParts[1],
                                ParentTable = firstParts[0] != table ? firstParts[0] : secondParts[0],
                                ParentColumn = firstParts[0] != table ? firstParts[1] : secondParts[1],

                            });
                        }
                    }
                    else ThrowInvalidSyntaxError();
                }
                else ThrowInvalidSyntaxError();
            }
            
        }
    }
}
