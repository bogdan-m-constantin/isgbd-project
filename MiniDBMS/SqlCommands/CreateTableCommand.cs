using MiniDBMS.Context;
using MiniDBMS.Domain;
using MiniDBMS.Utils;

using System.Security.Cryptography.X509Certificates;
using Attribute = MiniDBMS.Domain.Attribute;

namespace MiniDBMS.SqlCommands
{
    
    public class CreateTableCommand : SqlCommand
    {
        private string _tableName = string.Empty;
        private List<Attribute> _columns = new();
        public CreateTableCommand(params string[] cmd) : base(cmd)
        {
        }
        public override string CorrectSyntax => "CREATE TABLE <tableName> (col1 type [PRIMARY KEY], col2 type [PRIMARY KEY], [...] , coln type [PRIMARY KEY])";
        // CREATE TABLE <tableName> (col1 type, col2 type, col3 type)
        public override void Execute(SqlExecutionContext context)
        {
            if(context.CurrentDatabase == null)
                throw new Exception($"Database not selected. Select detabase with USE DATABASE <database-name> command");
            if (context.TableExists(_tableName))
                throw new Exception($"The table {_tableName} allready exists.");

            Table t = new Table()
            {
                Name = _tableName,
                Attributes = _columns,

            };
            context.AddTable(t);
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
            
            
            
            foreach(var attr in attrs)
            {
                var parts = attr.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if(parts.Length != 2 && parts.Length != 4)
                    ThrowInvalidSyntaxError();
                string attrName = parts[0];
                DataType attrType = Enum.Parse<DataType>(parts[1], true);
                if (attrType == DataType.Invalid)
                    ThrowInvalidSyntaxError();
                bool primaryKey = false;
                if(parts.Length == 4)
                {
                    if (parts[2] == "PRIMARY" && parts[3] == "KEY")
                    {
                        primaryKey = true;
                    }
                    else
                        ThrowInvalidSyntaxError();
                }
                _columns.Add(new() {Type = attrType,Name = attrName, PrimaryKey = primaryKey});

            }
        }
    }

}
