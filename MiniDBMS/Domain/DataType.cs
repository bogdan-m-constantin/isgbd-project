using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace MiniDBMS.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataType
    {
        Invalid,
        String,
        Int,
        Decimal,
        Boolean,
    }
    
}
