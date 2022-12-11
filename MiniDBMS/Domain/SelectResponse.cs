using MiniDBMS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniDBMS.Domain
{
    public class SelectResponse
    {
        public Table Table { get; set; }
        public string[] Attributes { get; set; }
        public IEnumerable<TableRow> Rows { get; set; }

        public SelectResponse(Table table, IEnumerable<TableRow> rows, string[] attributes)
        {
            Table = table;
            Rows = rows;
            Attributes = attributes;
        }
        public override string ToString()
        {   
            StringBuilder builder = new StringBuilder();
            var values = Rows.Select(e => e.UnpackData(Table).ToDictionary(k => k.Key, v => v.Value?.ToString() ?? "NULL")).ToList();
            var headers = Attributes
                .Select(
                    a => new TableHeader(a, Math.Max(values.Select(r => r[a].Length).Max(), a.Length))
                );
            builder.AppendRowDelimiter(headers)
                .AppendRowContent(headers, headers.ToDictionary(e => e.Name, e => e.Name))
                .AppendRowDelimiter(headers);
            values.ForEach(e =>
                builder.AppendRowContent(headers, e).AppendRowDelimiter(headers)
            ) ;
            return builder.ToString();
        }
    }
}
