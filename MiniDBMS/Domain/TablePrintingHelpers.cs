using System.Text;

namespace MiniDBMS.Domain
{
    public static class TablePrintingHelpers
    {
        public static StringBuilder AppendRowDelimiter(this StringBuilder builder, IEnumerable<TableHeader> headers, char padChar = '-')
        {
            foreach (var header in headers)
            {
                builder.Append("+").Append("".PadRight(header.Width+2, padChar));
            }
            builder.Append('+').AppendLine();
            return builder;
        }
        public static StringBuilder AppendRowContent(this StringBuilder builder, IEnumerable<TableHeader> headers, Dictionary<string, string> values)
        {
            foreach (var header in headers)
                builder.Append("| ").Append(values[header.Name].PadRight(header.Width, ' ')).Append(' ');
            
            builder.Append('|').AppendLine();
            
            return builder;
        }
    }
}
