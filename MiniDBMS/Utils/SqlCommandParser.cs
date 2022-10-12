using MiniDBMS.SqlCommands;
using Raven.Client.Documents.Queries.Suggestions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MiniDBMS.Utils
{
    public static class SqlCommandParser
    {


        public static SqlCommand ParseAsSqlCommand(this string cmd)
        {
            cmd = cmd.Cleanup();
            var tokens = cmd.Split(" ");
            if (tokens.Length == 0)
                throw new Exception("Invalid command given. Input was empty");
            var firstToken = tokens.First().ToUpperInvariant();
            var secondToken = tokens.Skip(1).FirstOrDefault()?.ToUpperInvariant();

            SqlCommand sqlCmd = tokens.First() switch
            {
                "USE" => new UseDatabaseCommand(tokens),
                "CREATE" => secondToken switch
                {
                    "DATABASE" => new CreateDatabaseCommand(tokens),
                    "TABLE" => new CreateTableCommand(tokens),
                    "INDEX" => new CreateIndexCommand(tokens),
                    _ => throw new Exception($"Invalid syntax near {secondToken}")
                }, // can be create db, table or index
                "DROP" => secondToken switch
                {
                    "DATABASE" => new DropDatabaseCommand(tokens),
                    "TABLE" => new DropTableCommand(tokens),
                    "INDEX" => new DropIndexCommand(tokens),
                    _ => throw new Exception($"Invalid syntax near {secondToken}")
                }, // can be create db,
                _ => throw new Exception($"Invalid syntax near {firstToken}")
            };


            sqlCmd.Parse();

            return sqlCmd;
        }
        public static string Cleanup(this string cmd)
        {
            return cmd.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Join(" ");
        }
        public static string Join(this IEnumerable<string> str, string separator)
        {
            return string.Join(separator, str);
        }
    }

}
