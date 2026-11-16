using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Arbiter.Services.Extensions;

/// <summary>
/// Extension methods for converting between <see cref="DateOnly"/>, <see cref="DateTime"/>, and <see cref="DateTimeOffset"/>.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts the <see cref="DateOnly"/> to a <see cref="DateTimeOffset"/> in the specified time zone.
    /// </summary>
    /// <param name="dateOnly">The date to convert.</param>
    /// <param name="time">The time of day to use for the conversion. If <see langword="null"/>, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <param name="zone">The time zone used to determine the UTC offset. If <see langword="null"/>, defaults to <see cref="TimeZoneInfo.Local"/>.</param>
    /// <returns>A <see cref="DateTimeOffset"/> representing the date and time with the offset of the specified time zone.</returns>
    public static DateTimeOffset ToDateTimeOffset(this DateOnly dateOnly, TimeOnly? time = null, TimeZoneInfo? zone = null)
    {
        zone ??= TimeZoneInfo.Local;
        time ??= TimeOnly.MinValue;

        var dateTime = dateOnly.ToDateTime(time.Value);
        var offset = zone.GetUtcOffset(dateTime);

        return new DateTimeOffset(dateTime, offset);
    }

    /// <summary>
    /// Converts the nullable <see cref="DateOnly"/> to a <see cref="DateTimeOffset"/> in the specified time zone.
    /// </summary>
    /// <param name="dateOnly">The date to convert.</param>
    /// <param name="time">The time of day to use for the conversion. If <see langword="null"/>, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <param name="zone">The time zone used to determine the UTC offset. If <see langword="null"/>, defaults to <see cref="TimeZoneInfo.Local"/>.</param>
    /// <returns>
    /// A <see cref="DateTimeOffset"/> representing the date and time with the offset of the specified time zone,
    /// or <see langword="null"/> if <paramref name="dateOnly"/> is <see langword="null"/>.
    /// </returns>
    [return: NotNullIfNotNull(nameof(dateOnly))]
    public static DateTimeOffset? ToDateTimeOffset(this DateOnly? dateOnly, TimeOnly? time = null, TimeZoneInfo? zone = null)
    {
        return dateOnly?.ToDateTimeOffset(time, zone);
    }


    /// <summary>
    /// Converts the <see cref="DateOnly"/> to a <see cref="DateTime"/> using the specified time of day.
    /// </summary>
    /// <param name="dateOnly">The date to convert.</param>
    /// <param name="time">The time of day to use for the conversion. If <see langword="null"/>, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>A <see cref="DateTime"/> with <see cref="DateTimeKind.Unspecified"/> kind representing the date and time.</returns>
    public static DateTime ToDateTime(this DateOnly dateOnly, TimeOnly? time = null)
    {
        time ??= TimeOnly.MinValue;
        return dateOnly.ToDateTime(time.Value);
    }

    /// <summary>
    /// Converts the nullable <see cref="DateOnly"/> to a <see cref="DateTime"/> using the specified time of day.
    /// </summary>
    /// <param name="dateOnly">The date to convert.</param>
    /// <param name="time">The time of day to use for the conversion. If <see langword="null"/>, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>
    /// A <see cref="DateTime"/> with <see cref="DateTimeKind.Unspecified"/> kind representing the date and time,
    /// or <see langword="null"/> if <paramref name="dateOnly"/> is <see langword="null"/>.
    /// </returns>
    [return: NotNullIfNotNull(nameof(dateOnly))]
    public static DateTime? ToDateTime(this DateOnly? dateOnly, TimeOnly? time = null)
    {
        return dateOnly?.ToDateTime(time);
    }


    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to a <see cref="DateOnly"/> in the specified timezone.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <param name="zone">The time zone to convert to before extracting the date. If <see langword="null"/>, defaults to <see cref="TimeZoneInfo.Local"/>.</param>
    /// <returns>The <see cref="DateOnly"/> for the date in the specified time zone.</returns>
    public static DateOnly ToDateOnly(this DateTimeOffset dateTime, TimeZoneInfo? zone = null)
    {
        zone ??= TimeZoneInfo.Local;

        var targetZone = TimeZoneInfo.ConvertTime(dateTime, zone);
        return DateOnly.FromDateTime(targetZone.Date);
    }

    /// <summary>
    /// Converts the nullable <see cref="DateTimeOffset"/> to a <see cref="DateOnly"/> in the specified timezone.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <param name="zone">The time zone to convert to before extracting the date. If <see langword="null"/>, defaults to <see cref="TimeZoneInfo.Local"/>.</param>
    /// <returns>
    /// The <see cref="DateOnly"/> for the date in the specified time zone,
    /// or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.
    /// </returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateOnly? ToDateOnly(this DateTimeOffset? dateTime, TimeZoneInfo? zone = null)
    {
        return dateTime?.ToDateOnly(zone);
    }

    /// <summary>
    /// Converts the <see cref="DateTime"/> to a <see cref="DateOnly"/>.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
    /// <returns>The <see cref="DateOnly"/> for the date component of <paramref name="dateTime"/>.</returns>
    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }

    /// <summary>
    /// Converts the nullable <see cref="DateTime"/> to a <see cref="DateOnly"/>.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
    /// <returns>
    /// The <see cref="DateOnly"/> for the date component of <paramref name="dateTime"/>,
    /// or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.
    /// </returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateOnly? ToDateOnly(this DateTime? dateTime)
    {
        return dateTime?.ToDateOnly();
    }
}
