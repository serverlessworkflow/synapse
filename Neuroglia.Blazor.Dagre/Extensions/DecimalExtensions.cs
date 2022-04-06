using System.Globalization;

namespace Neuroglia.Blazor.Dagre
{
    public static class DecimalExtensions
    {
        public static string ToInvariantString(this decimal number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }
        public static string? ToInvariantString(this decimal? number)
        {
            return number?.ToString(CultureInfo.InvariantCulture);
        }
    }
}
