using MiniDBMS.Context;
using MiniDBMS.Domain;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Attribute = MiniDBMS.Domain.Attribute;

namespace MiniDBMS.Utils
{
    public static class SelectUtils
    {
        public static IDocumentQuery<TableRow> ApplyCondition(this IDocumentQuery<TableRow> rows, SelectCondition condition, Table table, SqlExecutionContext context, IDocumentSession session) {
            var column = table.Attributes.First(e => e.Name == condition.Column);
            if (condition.Operand == "BETWEEN")
                return rows.ApplyRangeCondition(column, Convert.ToDecimal(condition.FirstValue), true, Convert.ToDecimal(condition.SecondValue), true, context, table, session);
            if (condition.Operand == "<")
                return rows.ApplyRangeCondition(column, Decimal.MinValue, true, Convert.ToDecimal(condition.Value), false, context, table, session);
            if (condition.Operand == "<=")
                return rows.ApplyRangeCondition(column, Decimal.MinValue, true, Convert.ToDecimal(condition.Value), true, context, table, session);
            if (condition.Operand == ">=")
                return rows.ApplyRangeCondition(column, Convert.ToDecimal(condition.Value), true, Decimal.MaxValue, false, context, table, session);
            if (condition.Operand == ">")
                return rows.ApplyRangeCondition(column, Convert.ToDecimal(condition.Value), true, Decimal.MaxValue, true, context, table, session);
            if (condition.Operand == "=")
                return rows.ApplyEqualCondition(column, condition.Value, context, table, session);
            if (condition.Operand == "<>")
                return rows.ApplyNotEqualCondition(column, condition.Value, context, table, session);

            return rows;
        }
        public static IDocumentQuery<TableRow> ApplyRangeCondition(this IDocumentQuery<TableRow> rows, Attribute column, decimal left, bool leftInclusive, decimal right, bool rightInclusive, SqlExecutionContext context, Table table, IDocumentSession session)
        {
            if (column.Type != DataType.Int && column.Type != DataType.Decimal)
                throw new Exception($"Can not have range query on {Enum.GetName<DataType>(column.Type)}");

            var index = table.GetIndexForColumn(column.Name);
            if (index == null)
                return rows.ApplyRangeConditionWithoutIndex(column, left, leftInclusive, right, rightInclusive, context, table,session);
            string startId = $"{context.CurrentDatabase}:{index.Name}:{left.ToString().CleanDataAs(column.Type)}";
            int leftCompare = leftInclusive ? 0 : 1;
            int rightCompare = rightInclusive ? 0 : -1;
            string endId = $"{context.CurrentDatabase}:{index.Name}:{right.ToString().CleanDataAs(column.Type)}";
            
            string[] ids = session.Query<IndexItem>().Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{index.Name}:")).ToList().Where(e => e.Id.CompareTo(startId) >= leftCompare).Where(e => e.Id.CompareTo(endId) <= rightCompare).ToList()
                .SelectMany(e => e.Values.Split("|").Select(e => $"{context.CurrentDatabase}:{table.Name}:{e}")).ToArray();
            return rows.WhereIn(e => e.Id, ids);


        }
        public static IDocumentQuery<TableRow> ApplyRangeConditionWithoutIndex(this IDocumentQuery<TableRow> rows, Attribute column, decimal left, bool leftInclusive, decimal right, bool rightInclusive, SqlExecutionContext context, Table table, IDocumentSession session)
        {
            int leftCompare = leftInclusive ? 0 : 1;
            int rightCompare = rightInclusive ? 0 : -1;

            var ids = session.Query<TableRow>().Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{table.Name}:")).ToList().ToDictionary(e => e.Id, e => (decimal)e.UnpackData(table)[column.Name]).
                Where(e => e.Value.CompareTo(left) >= leftCompare && e.Value.CompareTo(right) <= rightCompare).Select(e => e.Key).ToArray();
            return rows.WhereIn(e => e.Id, ids);

        }
        public static IDocumentQuery<TableRow> ApplyEqualCondition(this IDocumentQuery<TableRow> rows, Attribute column, string value, SqlExecutionContext context, Table table, IDocumentSession session)
        {

            // !!!!!!!!!!!!!!!!!!!!!!
            string[] ids = new string[] { };

            
            var index = table.GetIndexForColumn(column.Name);
            if (index == null)
                return rows.ApplyEqualConditionWithoutIndex(column,value, context, table, session);
            string id = $"{context.CurrentDatabase}:{index.Name}:{value.CleanDataAs(column.Type)}";
            ids = session.Load<IndexItem>(id)?.Values?.Split("|",StringSplitOptions.RemoveEmptyEntries).Select(e => $"{context.CurrentDatabase}:{table.Name}:{e}").ToArray()??new string[] {};
            return rows.WhereIn(e => e.Id, ids);

        }
        public static IDocumentQuery<TableRow> ApplyEqualConditionWithoutIndex(this IDocumentQuery<TableRow> rows, Attribute column, string value, SqlExecutionContext context, Table table, IDocumentSession session)
        {

            var ids = session.Query<TableRow>().Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{table.Name}:")).ToList().ToDictionary(e => e.Id, e => e.UnpackData(table)[column.Name]).
               Where(e => CompareAs(column.Type, value.CleanDataAs(column.Type), e.Value.ToString()) == 0).Select(e => e.Key).ToArray();
            return rows.WhereIn(e => e.Id, ids);
        }
       
        // !!!!!!!!!!!!!!!!!!!!!!
        public static IDocumentQuery<TableRow> ApplyNotEqualCondition(this IDocumentQuery<TableRow> rows, Attribute column, string value, SqlExecutionContext context, Table table, IDocumentSession session)
        {
            var index = table.GetIndexForColumn(column.Name);
            if (index == null)
                return rows.ApplyNotEqualConditionWithoutIndex(column,value , context, table, session);

            var ids = new string[] { };
            var lst = session.Query<IndexItem>().Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{index.Name}:")).ToList();
            ids = lst
                .Where(e => e.Id != $"{context.CurrentDatabase}:{index.Name}:{value.CleanDataAs(column.Type)}")
                .SelectMany(e => e.Values.Split("|").Select(e => $"{context.CurrentDatabase}:{table.Name}:{e}")).ToArray();
            return rows.WhereIn(e => e.Id, ids);

        }
        public static IDocumentQuery<TableRow> ApplyNotEqualConditionWithoutIndex(this IDocumentQuery<TableRow> rows, Attribute column, string value, SqlExecutionContext context, Table table, IDocumentSession session)
        {

            var ids = session.Query<TableRow>().Where(e => e.Id.StartsWith($"{context.CurrentDatabase}:{table.Name}:")).ToList().ToDictionary(e => e.Id, e => e.UnpackData(table)[column.Name]).
               Where(e => CompareAs(column.Type, value, e.Value.ToString()) != 0).Select(e => e.Key).ToArray();
            return rows.WhereIn(e => e.Id, ids);
        }

        private static int CompareAs(DataType type, string a, string? b)
        {
            if (a == "NULL")
                return b == null ? 0 : -1;
            
            return type switch
            {
                DataType.Invalid => -1,
                DataType.String => a.CompareTo(b),
                DataType.Int => Convert.ToInt32(a).CompareTo(Convert.ToInt32(b)),
                DataType.Decimal => Convert.ToDecimal(a).CompareTo(Convert.ToDecimal(b)),
                DataType.Boolean => Convert.ToBoolean(a).CompareTo(Convert.ToBoolean(b)),
                _ => -1,
            };
        }
    }
}
