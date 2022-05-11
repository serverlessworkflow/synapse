using System.Runtime.Serialization;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Enumerates all supported chart types
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum ChartType
    {
        /// <summary>
        /// Indicates a pie chart
        /// </summary>
        [EnumMember(Value = "pie")]
        Pie,
        /// <summary>
        /// Indicates a doughnut chart
        /// </summary>
        [EnumMember(Value = "doughnut")]
        Doughnut,
        /// <summary>
        /// Indicates a bar chart
        /// </summary>
        [EnumMember(Value = "bar")]
        Bar,
        /// <summary>
        /// Indicates a line chart
        /// </summary>
        [EnumMember(Value = "line")]
        Line,
        /// <summary>
        /// Indicates a polar area chart
        /// </summary>
        [EnumMember(Value = "polarArea")]
        PolarArea,
        /// <summary>
        /// Indicates a bubble chart
        /// </summary>
        [EnumMember(Value = "bubble")]
        Bubble,
        /// <summary>
        /// Indicates a radar chart
        /// </summary>
        [EnumMember(Value = "radar")]
        Radar,
        /// <summary>
        /// Indicates a scatter chart
        /// </summary>
        [EnumMember(Value = "scatter")]
        Scatter
    }

}
