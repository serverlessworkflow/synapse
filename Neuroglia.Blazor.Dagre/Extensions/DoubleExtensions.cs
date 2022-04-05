using System.Globalization;

namespace Neuroglia.Blazor.Dagre
{
    public static class DoubleExtensions
    {
        public static string ToInvariantString(this double number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }
        public static string? ToInvariantString(this double? number)
        {
            return number?.ToString(CultureInfo.InvariantCulture);
        }
    }
}
