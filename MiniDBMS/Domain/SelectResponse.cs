using MiniDBMS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniDBMS.Domain
{
    public class JoinResult
    {
        public List<Dictionary<string, TableRow>> Rows { get; set; }
    }
    public class SelectAttribute
    {
        public string Attribute = string.Empty;
        public string Table = string.Empty;

        public override string? ToString()
        {
            return $"{Table}.{Attribute}";
        }
    }
    public class SelectResponse
    {
        public Dictionary<string,Table> Tables { get; set; }
        public bool Distinct{ get; set; }
        public List<SelectAttribute> Attributes { get; set; }
        public JoinResult Result { get; set; }

        public SelectResponse(Dictionary<string, Table> tables, JoinResult rows, List<SelectAttribute> attributes, bool distinct)
        {
            Tables = tables;
            Result = rows;
            Attributes = attributes;
            Distinct = distinct;
        }
        public override string ToString()
        {   
            StringBuilder builder = new StringBuilder();
            var tempValues = Result.Rows.Select( e=> e.ToDictionary(c => c.Key, v => v.Value.UnpackData(Tables[v.Key]).ToDictionary(k => k.Key, f => f.Value?.ToString() ?? "NULL")));
                var values = (Distinct ? tempValues.DistinctBy(e => Attributes.Select(a => e[a.Table][a.Attribute]).Join("#")) : tempValues).ToList();
            int count = values.Count;
            if(values.Count > 20)
            {
                values = values.Take(20).ToList();
                builder.AppendLine($"Only showing 20 results out of {count}");
            }

            var headers = Attributes
                .Select(
                    a => new TableHeader(a.Attribute, Math.Max(values.Select(r => r[a.Table][a.Attribute].Length).Max(), a.Attribute.Length),a.Table)
                ).ToList();
            builder.AppendRowDelimiter(headers)
                .AppendRowContent(headers, headers.DistinctBy(e=>e.Table).ToDictionary(e => e.Table, e=> headers.Where(t=>t.Table == e.Table).ToDictionary(t=>t.Name, t=>t.Name)))
                .AppendRowDelimiter(headers);
            values.ForEach(e =>
                builder.AppendRowContent(headers, e).AppendRowDelimiter(headers)
            ) ;
            return builder.ToString();
        }
    }
}
