using MiniDBMS.Context;
using MiniDBMS.Domain;
using MiniDBMS.Utils;

using System.Security.Cryptography.X509Certificates;
using Attribute = MiniDBMS.Domain.Attribute;
using Index = MiniDBMS.Domain.Index;

namespace MiniDBMS.SqlCommands
{

    public class CreateTableCommand : SqlCommand
    {
        private string _tableName = string.Empty;
        private List<Attribute> _columns = new();
        public CreateTableCommand(params string[] cmd) : base(cmd)
        {
        }
        public override string CorrectSyntax => "CREATE TABLE <tableName> (col1 type [PRIMARY KEY] [REFERENCES Table.Column], col2 type [PRIMARY KEY], [...] , coln type [PRIMARY KEY])";
        // CREATE TABLE <tableName> (col1 type, col2 type, col3 type)
        public override void Execute(SqlExecutionContext context)
        {
            if (context.CurrentDatabase == null)
                throw new Exception($"Database not selected. Select detabase with USE DATABASE <database-name> command");
            if (context.TableExists(_tableName))
                throw new Exception($"The table {_tableName} allready exists.");

            foreach(var c in _columns.Where(c=> c.ForeignKey != null))
            {
                if (!context.TableExists(c.ForeignKey!.ReferencedTable))
                    throw new Exception($"Referenced table {c.ForeignKey.ReferencedTable} does not exist");

                if (!context.ColumnExists(c.ForeignKey!.ReferencedTable, c.ForeignKey!.ReferencedColumn))
                    throw new Exception($"Referenced column {c.ForeignKey.ReferencedColumn} does not exist on table {c.ForeignKey.ReferencedTable}");

                if (context.GetColumn(c.ForeignKey!.ReferencedTable, c.ForeignKey!.ReferencedColumn).Type != c.Type)
                    throw new Exception($"Referenced column {c.Name} has diferent type than {c.ForeignKey.ReferencedTable}.{c.ForeignKey.ReferencedColumn}");
                
            }

            Table t = new()
            {
                Name = _tableName,
                Attributes = _columns,

            };

            context.AddTable(t);
            foreach(var fk in t.ForeignKeyAttributes)
            {

                context.CreateIndex(_tableName, new Index() { Column = fk.Name, Name = fk.ForeignKey.Name });
            }

            
        }

        public override void Parse()
        {
            _tableName = _command[2];

            if (!_command[3].StartsWith("("))
                throw new Exception("Invalid syntax. Correct syntax is: ");
            string attributesStr = _command.Skip(3).Join(" ").Trim();
            if (!attributesStr.EndsWith(")"))
                ThrowInvalidSyntaxError();

            attributesStr = attributesStr.Substring(1, attributesStr.Length - 2);

            if (attributesStr.Contains(")") || attributesStr.Contains("("))
                ThrowInvalidSyntaxError();

            var attrs = attributesStr.Split(",");



            foreach (var attr in attrs)
            {
                var parts = attr.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length != 2 && parts.Length != 4 && parts.Length != 6)
                    ThrowInvalidSyntaxError();
                string attrName = parts[0];
                DataType attrType = Enum.Parse<DataType>(parts[1], true);
                if (attrType == DataType.Invalid)
                    ThrowInvalidSyntaxError();
                bool primaryKey = false;
                bool foreignKey = false;
                string? referencedTable = null;
                string? referencedColumn = null;
                if (parts.Length == 4 || (parts.Contains("PRIMARY") && parts.Contains("REFERENCES") && parts.Length == 6))
                {
                    if (parts[2] == "PRIMARY" && parts[3] == "KEY")
                    {
                        primaryKey = true;
                    }
                    if (parts[2] == "REFERENCES" || (parts[2] == "PRIMARY" && parts.Length >= 5 && parts[4] == "REFERENCES"))
                    {
                        var tableParts = parts[parts[2] == "PRIMARY" ? 5 : 3].Split(".");

                        if (tableParts.Length != 2)
                            ThrowInvalidSyntaxError();
                        foreignKey = true;
                        referencedTable = tableParts[0];
                        referencedColumn = tableParts[1];
                    }
                }
                else if (parts.Length != 2)
                    ThrowInvalidSyntaxError();
                    
                _columns.Add(new()
                {
                    Type = attrType,
                    Name = attrName,
                    PrimaryKey = primaryKey,
                    ForeignKey = foreignKey ? new()
                    {
                        ReferencedTable = referencedTable!,
                        ReferencedColumn = referencedColumn!,
                        SourceColumn = attrName,
                        SourceTable = _tableName
                    } : null
                });

            }
        }
    }

}
