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
        public bool Distinct{ get; set; }
        public string[] Attributes { get; set; }
        public IEnumerable<TableRow> Rows { get; set; }

        public SelectResponse(Table table, IEnumerable<TableRow> rows, string[] attributes, bool distinct)
        {
            Table = table;
            Rows = rows;
            Attributes = attributes;
            Distinct = distinct;
        }
        public override string ToString()
        {   
            StringBuilder builder = new StringBuilder();
            var tempValues = Rows.Select(e => e.UnpackData(Table).ToDictionary(k => k.Key, v => v.Value?.ToString() ?? "NULL"));
                var values = (Distinct ? tempValues.DistinctBy(e => Attributes.Select(a => e[a]).Join("#")) : tempValues).ToList();
            int count = values.Count;
            if(values.Count > 20)
            {
                values = values.Take(20).ToList();
                builder.AppendLine($"Only showing 20 results out of {count}");
            }

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
