namespace MiniDBMS.Domain
{
    public class SelectCondition
    {
        public string Column { get; set; }
        public string Operand { get; set; }
        public string FirstValue { get; set; }
        public string Value => FirstValue;
        public string? SecondValue { get; set; }

        public SelectCondition(string column, string operand, string firstValue, string? secondValue)
        {
            Column = column;
            Operand = operand;
            FirstValue = firstValue;
            SecondValue = secondValue;
        }
    }
}
