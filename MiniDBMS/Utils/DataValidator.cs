using MiniDBMS.Domain;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Index = MiniDBMS.Domain.Index;

namespace MiniDBMS.Utils
{
    public static class DataUtils
    {
        public const string IntFormat = "0000000000";
        public const string DecimalFormat = "0000000000.0000000000";
        public static bool ValidateAs(this string data, DataType type)
        {
            if (data.ToUpperInvariant() == "NULL")
                return true;
            return type switch
            {
                DataType.Int => data.ValidateAsInt(),
                DataType.String => data.ValidateAsString(),
                DataType.Decimal => data.ValidateAsDecimal(),
                DataType.Boolean => data.ValidateAsBoolean(),
                _ => false,
            };
        }
        private static bool ValidateAsInt(this string data)
        {
            return int.TryParse(data, out var _);

        }
        private static bool ValidateAsString(this string data)
        {
            if(data.StartsWith("\"") && data.EndsWith("\""))
            {
                return Regex.Matches(data.Substring(1,data.Length - 2), @"^(?:[^""\\]|\\.|""(?:\\.|[^""\\])*"")*$").Count == 1;
            }
            return false;
        }
        private static bool ValidateAsDecimal(this string data)
        {
            return decimal.TryParse(data, out var _);
        }
        private static bool ValidateAsBoolean(this string data)
        {
            return bool.TryParse(data,out var _);
        }

        public static string CleanDataAs(this string data, DataType type)
        {
            if (data.ToUpperInvariant() == "NULL")
                return data.ToUpperInvariant();
            return type switch
            {
                DataType.String => data.CleanDataAsString(),
                DataType.Boolean => data.CleanDataAsBoolean(),
                DataType.Int => data.CleanDataAsInt(),
                DataType.Decimal => data.CleanDataAsDecimal(),
                _ => data,
            };
        }
        private static string CleanDataAsString(this string data)
        {
            return data.Substring(1, data.Length - 2).Replace(";", "#~#");
        }
        private static string CleanDataAsBoolean(this string data)
        {
            return bool.Parse(data).ToString();
        }
        private static string CleanDataAsInt(this string data)
        {
            return int.Parse(data).ToString(IntFormat);
        }
        private static string CleanDataAsDecimal(this string data)
        {
            return decimal.Parse(data).ToString(DecimalFormat);
        }

        public static (string id, string values) PackData(this Table t, Dictionary<string, string> values)
        {
            List<string> valueParts = new();
            List<string> idParts = new();
            foreach (var c in t.Attributes)
            {
                var value = values.ContainsKey(c.Name) ? values[c.Name] : "NULL";
                if (c.PrimaryKey)
                    idParts.Add(value);
                else
                    valueParts.Add(value);
            }
            return (idParts.Join(";"), valueParts.Join(";"));

        }
        public static (string id, string values) PackData(this Table t, Index index, Dictionary<string, string> values)
        {
            List<string> valueParts = new();
            List<string> idParts = new();
            foreach (var c in t.Attributes)
            {
                var value = values.ContainsKey(c.Name) ? values[c.Name] : "NULL";
                if (c.PrimaryKey)
                    valueParts.Add(value);
                if(index.Column.Contains(c.Name))
                    idParts.Add(value);
            }
            return (idParts.Join(";"), valueParts.Join(";"));

        }
        public static Dictionary<string,object> UnpackData(this TableRow row, Table table)
        {
            var idParts = row.Id.Split(":")[2].Split(";");
            int idIndex = 0;
            var valueParts = row.Values.Split(";");
            int valuesIndex = 0;
            Dictionary<string, object> values = new();
            foreach (var c in table.Attributes)
            {
                
                if (c.PrimaryKey)
                {
                    values[c.Name] = FormatForDisplay(idParts[idIndex],c.Type);
                    idIndex++;
                }
                else
                {
                    values[c.Name] = FormatForDisplay(valueParts[valuesIndex],c.Type);
                    valuesIndex++;
                }
            }
            return values;
        }

        private static object FormatForDisplay(string value, DataType type)
        {
            try
            {
                return type switch
                {
                    DataType.Int => int.Parse(value),
                    DataType.Decimal => decimal.Parse(value),
                    _ => value
                };
            }
            catch (Exception e) {
                throw e;
            }
        }
    }
}
