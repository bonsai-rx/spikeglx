using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

/// <summary>
/// Provides a method for parsing a string representing a range of channels into an array of integers.
/// </summary>
public static class ChannelRangeParser
{
    /// <summary>
    /// Parses a string representing a range of channels into an array of integers.
    /// </summary>
    /// <param name="range">The string to parse, in the format "start:step:end, start:step:end".</param>
    /// <returns>An array of integers representing the parsed channel range.</returns>
    public static int[] Parse(string range)
    {
        // "0:5:10,1:3" -> ["0:5:10", "0:3"]
        return range.Split(',')
        // ["0:5:10", "1:3"] -> [["0", "5", "10"], ["0", "3"]]
       .Select(x => x.Split(':'))
       // [["0", "5", "10"], ["1", "3"]] -> [[0, 10, 5], [0, 3, 1]]
       .Select(p => new {
           /// <summary>
           /// The first value in the range.
           /// </summary>
           First = int.Parse(p.First()),
           /// <summary>
           /// The last value in the range.
           /// </summary>
           Last = int.Parse(p.Last()),
           /// <summary>
           /// The step value in the range, or 1 if not specified.
           /// </summary>
           Step = (p.Length == 3) ? int.Parse(p.Skip(1).First()) : 1
       })
       // [[0, 10, 5], [0, 3, 1]] -> [0, 5, 10, 0, 1, 2, 3]
       .SelectMany(x => Enumerable.Range(x.First, x.Last - x.First + 1)
         .Where((y, i) => i % x.Step == 0))
       // [0, 5, 10, 0, 1, 2, 3] -> [0, 0, 1, 2, 3, 5, 10]
       .OrderBy(z => z)
       // [0, 1, 2, 3, 5, 10] -> [0, 1, 2, 3, 5, 10]
       .Distinct()
       .ToArray();
    }
}

/// <summary>
/// Converts a string representation of a channel range to an array of integers.
/// </summary>
public class ChannelRangeTypeConverter : TypeConverter
{
    /// <summary>
    /// Determines if the type converter can convert from the specified source type.
    /// </summary>
    /// <param name="context">The type descriptor context.</param>
    /// <param name="sourceType">The source type to convert from.</param>
    /// <returns>True if the type converter can convert from the source type, false otherwise.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        // Check if the source type is a string, which can be converted to a channel range.
        if (sourceType == typeof(string))
        {
            return true;
        }
        // If not, delegate to the base class for further checking.
        return base.CanConvertFrom(context, sourceType);
    }

    /// <summary>
    /// Converts the specified object to an array of integers representing a channel range.
    /// </summary>
    /// <param name="context">The type descriptor context.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <param name="value">The object to convert.</param>
    /// <returns>An array of integers representing the channel range.</returns>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        // Check if the value is a string, which can be parsed to a channel range.
        if (value is string stringValue)
        {
            // Use the ChannelRangeParser to parse the string to an array of integers.
            return ChannelRangeParser.Parse(stringValue);
        }
        // If not, delegate to the base class for further conversion.
        return base.ConvertFrom(context, culture, value);
    }
}