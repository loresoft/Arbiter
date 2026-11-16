using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Arbiter.Services.Extensions;

/// <summary>
///   <see cref="TimeZoneInfo" /> constants and extension methods for converting <see cref="DateTimeOffset" />,
///   <see cref="DateTime" />, and <see cref="DateOnly" /> values (including nullable values) to US time zones.
/// </summary>
public static class TimeZoneExtensions
{
    private static readonly FrozenDictionary<string, (string Standard, string Daylight)> _zoneAbbreviations =
        new Dictionary<string, (string Standard, string Daylight)>(StringComparer.OrdinalIgnoreCase)
        {
            ["America/New_York"] = ("EST", "EDT"),
            ["America/Chicago"] = ("CST", "CDT"),
            ["America/Denver"] = ("MST", "MDT"),
            ["America/Los_Angeles"] = ("PST", "PDT"),
        }
        .ToFrozenDictionary();

    /// <summary>
    /// Eastern Time (America/New_York) — covers both EST (UTC−5) and EDT (UTC−4).
    /// </summary>
    public static TimeZoneInfo Eastern { get; } =
        TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    /// <summary>
    /// Central Time (America/Chicago) — covers both CST (UTC−6) and CDT (UTC−5).
    /// </summary>
    public static TimeZoneInfo Central { get; } =
        TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

    /// <summary>
    /// Mountain Time (America/Denver) — covers both MST (UTC−7) and MDT (UTC−6).
    /// </summary>
    public static TimeZoneInfo Mountain { get; } =
        TimeZoneInfo.FindSystemTimeZoneById("America/Denver");

    /// <summary>
    /// Pacific Time (America/Los_Angeles) — covers both PST (UTC−8) and PDT (UTC−7).
    /// </summary>
    public static TimeZoneInfo Pacific { get; } =
        TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");


    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to Eastern Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Eastern Time.</returns>
    public static DateTimeOffset ToEastern(this DateTimeOffset dateTime)
        => TimeZoneInfo.ConvertTime(dateTime, Eastern);

    /// <summary>
    /// Converts the nullable <see cref="DateTimeOffset"/> to Eastern Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Eastern Time, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateTimeOffset? ToEastern(this DateTimeOffset? dateTime)
        => dateTime?.ToEastern();

    /// <summary>
    /// Converts the <see cref="DateTime"/> to Eastern Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert. Must have <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Eastern Time.</returns>
    /// <exception cref="ArgumentException">Thrown when <see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Unspecified"/>.</exception>
    public static DateTimeOffset ToEastern(this DateTime dateTime)
        => dateTime.Kind != DateTimeKind.Unspecified
            ? TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), Eastern)
            : throw new ArgumentException("DateTime.Kind must not be Unspecified.", nameof(dateTime));

    /// <summary>
    /// Converts the nullable <see cref="DateTime"/> to Eastern Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert. When not <see langword="null"/>, must have <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Eastern Time, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Unspecified"/>.</exception>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateTimeOffset? ToEastern(this DateTime? dateTime)
        => dateTime?.ToEastern();

    /// <summary>
    /// Converts the <see cref="DateOnly"/> to Eastern Time.
    /// </summary>
    /// <param name="dateOnly">The <see cref="DateOnly"/> to convert.</param>
    /// <param name="time">The time to use for the conversion. If null, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Eastern Time.</returns>
    public static DateTimeOffset ToEastern(this DateOnly dateOnly, TimeOnly? time = null)
    {
        time ??= TimeOnly.MinValue;

        var dt = dateOnly.ToDateTime(time.Value);
        return new DateTimeOffset(dt, Eastern.GetUtcOffset(dt));
    }

    /// <summary>
    /// Converts the nullable <see cref="DateOnly"/> to Eastern Time.
    /// </summary>
    /// <param name="dateOnly">The <see cref="DateOnly"/> to convert.</param>
    /// <param name="time">The time to use for the conversion. If null, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Eastern Time, or <see langword="null"/> if <paramref name="dateOnly"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateOnly))]
    public static DateTimeOffset? ToEastern(this DateOnly? dateOnly, TimeOnly? time = null)
        => dateOnly?.ToEastern(time);


    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to Central Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Central Time.</returns>
    public static DateTimeOffset ToCentral(this DateTimeOffset dateTime)
        => TimeZoneInfo.ConvertTime(dateTime, Central);

    /// <summary>
    /// Converts the nullable <see cref="DateTimeOffset"/> to Central Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Central Time, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateTimeOffset? ToCentral(this DateTimeOffset? dateTime)
        => dateTime?.ToCentral();

    /// <summary>
    /// Converts the <see cref="DateTime"/> to Central Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert. Must have <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Central Time.</returns>
    /// <exception cref="ArgumentException">Thrown when <see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Unspecified"/>.</exception>
    public static DateTimeOffset ToCentral(this DateTime dateTime)
        => dateTime.Kind != DateTimeKind.Unspecified
            ? TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), Central)
            : throw new ArgumentException("DateTime.Kind must not be Unspecified.", nameof(dateTime));

    /// <summary>
    /// Converts the nullable <see cref="DateTime"/> to Central Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert. When not <see langword="null"/>, must have <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Central Time, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Unspecified"/>.</exception>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateTimeOffset? ToCentral(this DateTime? dateTime)
        => dateTime?.ToCentral();

    /// <summary>
    /// Converts the <see cref="DateOnly"/> to Central Time.
    /// </summary>
    /// <param name="dateOnly">The <see cref="DateOnly"/> to convert.</param>
    /// <param name="time">The time to use for the conversion. If null, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Central Time.</returns>
    public static DateTimeOffset ToCentral(this DateOnly dateOnly, TimeOnly? time = null)
    {
        time ??= TimeOnly.MinValue;

        var dt = dateOnly.ToDateTime(time.Value);
        return new DateTimeOffset(dt, Central.GetUtcOffset(dt));
    }

    /// <summary>
    /// Converts the nullable <see cref="DateOnly"/> to Central Time.
    /// </summary>
    /// <param name="dateOnly">The <see cref="DateOnly"/> to convert.</param>
    /// <param name="time">The time to use for the conversion. If null, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Central Time, or <see langword="null"/> if <paramref name="dateOnly"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateOnly))]
    public static DateTimeOffset? ToCentral(this DateOnly? dateOnly, TimeOnly? time = null)
        => dateOnly?.ToCentral(time);


    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to Mountain Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Mountain Time.</returns>
    public static DateTimeOffset ToMountain(this DateTimeOffset dateTime)
        => TimeZoneInfo.ConvertTime(dateTime, Mountain);

    /// <summary>
    /// Converts the nullable <see cref="DateTimeOffset"/> to Mountain Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Mountain Time, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateTimeOffset? ToMountain(this DateTimeOffset? dateTime)
        => dateTime?.ToMountain();

    /// <summary>
    /// Converts the <see cref="DateTime"/> to Mountain Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert. Must have <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Mountain Time.</returns>
    /// <exception cref="ArgumentException">Thrown when <see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Unspecified"/>.</exception>
    public static DateTimeOffset ToMountain(this DateTime dateTime)
        => dateTime.Kind != DateTimeKind.Unspecified
            ? TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), Mountain)
            : throw new ArgumentException("DateTime.Kind must not be Unspecified.", nameof(dateTime));

    /// <summary>
    /// Converts the nullable <see cref="DateTime"/> to Mountain Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert. When not <see langword="null"/>, must have <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Mountain Time, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Unspecified"/>.</exception>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateTimeOffset? ToMountain(this DateTime? dateTime)
        => dateTime?.ToMountain();

    /// <summary>
    /// Converts the <see cref="DateOnly"/> to Mountain Time.
    /// </summary>
    /// <param name="dateOnly">The <see cref="DateOnly"/> to convert.</param>
    /// <param name="time">The time to use for the conversion. If null, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Mountain Time.</returns>
    public static DateTimeOffset ToMountain(this DateOnly dateOnly, TimeOnly? time = null)
    {
        time ??= TimeOnly.MinValue;

        var dt = dateOnly.ToDateTime(time.Value);
        return new DateTimeOffset(dt, Mountain.GetUtcOffset(dt));
    }

    /// <summary>
    /// Converts the nullable <see cref="DateOnly"/> to Mountain Time.
    /// </summary>
    /// <param name="dateOnly">The <see cref="DateOnly"/> to convert.</param>
    /// <param name="time">The time to use for the conversion. If null, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Mountain Time, or <see langword="null"/> if <paramref name="dateOnly"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateOnly))]
    public static DateTimeOffset? ToMountain(this DateOnly? dateOnly, TimeOnly? time = null)
        => dateOnly?.ToMountain(time);


    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to Pacific Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Pacific Time.</returns>
    public static DateTimeOffset ToPacific(this DateTimeOffset dateTime)
        => TimeZoneInfo.ConvertTime(dateTime, Pacific);

    /// <summary>
    /// Converts the nullable <see cref="DateTimeOffset"/> to Pacific Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Pacific Time, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateTimeOffset? ToPacific(this DateTimeOffset? dateTime)
        => dateTime?.ToPacific();

    /// <summary>
    /// Converts the <see cref="DateTime"/> to Pacific Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert. Must have <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Pacific Time.</returns>
    /// <exception cref="ArgumentException">Thrown when <see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Unspecified"/>.</exception>
    public static DateTimeOffset ToPacific(this DateTime dateTime)
        => dateTime.Kind != DateTimeKind.Unspecified
            ? TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), Pacific)
            : throw new ArgumentException("DateTime.Kind must not be Unspecified.", nameof(dateTime));

    /// <summary>
    /// Converts the nullable <see cref="DateTime"/> to Pacific Time.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert. When not <see langword="null"/>, must have <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Pacific Time, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Unspecified"/>.</exception>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateTimeOffset? ToPacific(this DateTime? dateTime)
        => dateTime?.ToPacific();

    /// <summary>
    /// Converts the <see cref="DateOnly"/> to Pacific Time.
    /// </summary>
    /// <param name="dateOnly">The <see cref="DateOnly"/> to convert.</param>
    /// <param name="time">The time to use for the conversion. If null, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Pacific Time.</returns>
    public static DateTimeOffset ToPacific(this DateOnly dateOnly, TimeOnly? time = null)
    {
        time ??= TimeOnly.MinValue;

        var dt = dateOnly.ToDateTime(time.Value);
        return new DateTimeOffset(dt, Pacific.GetUtcOffset(dt));
    }

    /// <summary>
    /// Converts the nullable <see cref="DateOnly"/> to Pacific Time.
    /// </summary>
    /// <param name="dateOnly">The <see cref="DateOnly"/> to convert.</param>
    /// <param name="time">The time to use for the conversion. If null, defaults to <see cref="TimeOnly.MinValue"/>.</param>
    /// <returns>The converted <see cref="DateTimeOffset"/> in Pacific Time, or <see langword="null"/> if <paramref name="dateOnly"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateOnly))]
    public static DateTimeOffset? ToPacific(this DateOnly? dateOnly, TimeOnly? time = null)
        => dateOnly?.ToPacific(time);


    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to Eastern Time and returns it as a formatted string
    /// with the timezone abbreviation appended (e.g., <c>01/15/2025 3:30 PM EST</c>).
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert and format.</param>
    /// <param name="format">The date and time format string. See <see cref="DateTimeOffset.ToString(string)"/> for valid formats.</param>
    /// <returns>The formatted Eastern Time string with the timezone abbreviation appended.</returns>
    public static string ToEasternString(
        this DateTimeOffset dateTime,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null) =>
        ToTimeZoneString(dateTime.ToEastern(), Eastern, format);

    /// <summary>
    /// Converts the nullable <see cref="DateTimeOffset"/> to Eastern Time and returns it as a formatted string
    /// with the timezone abbreviation appended (e.g., <c>01/15/2025 3:30 PM EST</c>).
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert and format.</param>
    /// <param name="format">The date and time format string. See <see cref="DateTimeOffset.ToString(string)"/> for valid formats.</param>
    /// <returns>The formatted Eastern Time string with the timezone abbreviation appended, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static string? ToEasternString(
        this DateTimeOffset? dateTime,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null) =>
        dateTime?.ToEasternString(format);

    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to Central Time and returns it as a formatted string
    /// with the timezone abbreviation appended (e.g., <c>01/15/2025 2:30 PM CST</c>).
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert and format.</param>
    /// <param name="format">The date and time format string. See <see cref="DateTimeOffset.ToString(string)"/> for valid formats.</param>
    /// <returns>The formatted Central Time string with the timezone abbreviation appended.</returns>
    public static string ToCentralString(
        this DateTimeOffset dateTime,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null) =>
        ToTimeZoneString(dateTime.ToCentral(), Central, format);

    /// <summary>
    /// Converts the nullable <see cref="DateTimeOffset"/> to Central Time and returns it as a formatted string
    /// with the timezone abbreviation appended (e.g., <c>01/15/2025 2:30 PM CST</c>).
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert and format.</param>
    /// <param name="format">The date and time format string. See <see cref="DateTimeOffset.ToString(string)"/> for valid formats.</param>
    /// <returns>The formatted Central Time string with the timezone abbreviation appended, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static string? ToCentralString(
        this DateTimeOffset? dateTime,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null) =>
        dateTime?.ToCentralString(format);

    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to Mountain Time and returns it as a formatted string
    /// with the timezone abbreviation appended (e.g., <c>01/15/2025 1:30 PM MST</c>).
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert and format.</param>
    /// <param name="format">The date and time format string. See <see cref="DateTimeOffset.ToString(string)"/> for valid formats.</param>
    /// <returns>The formatted Mountain Time string with the timezone abbreviation appended.</returns>
    public static string ToMountainString(
        this DateTimeOffset dateTime,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null) =>
        ToTimeZoneString(dateTime.ToMountain(), Mountain, format);

    /// <summary>
    /// Converts the nullable <see cref="DateTimeOffset"/> to Mountain Time and returns it as a formatted string
    /// with the timezone abbreviation appended (e.g., <c>01/15/2025 1:30 PM MST</c>).
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert and format.</param>
    /// <param name="format">The date and time format string. See <see cref="DateTimeOffset.ToString(string)"/> for valid formats.</param>
    /// <returns>The formatted Mountain Time string with the timezone abbreviation appended, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static string? ToMountainString(
        this DateTimeOffset? dateTime,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null) =>
        dateTime?.ToMountainString(format);

    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to Pacific Time and returns it as a formatted string
    /// with the timezone abbreviation appended (e.g., <c>01/15/2025 12:30 PM PST</c>).
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert and format.</param>
    /// <param name="format">The date and time format string. See <see cref="DateTimeOffset.ToString(string)"/> for valid formats.</param>
    /// <returns>The formatted Pacific Time string with the timezone abbreviation appended.</returns>
    public static string ToPacificString(
        this DateTimeOffset dateTime,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null) =>
        ToTimeZoneString(dateTime.ToPacific(), Pacific, format);

    /// <summary>
    /// Converts the nullable <see cref="DateTimeOffset"/> to Pacific Time and returns it as a formatted string
    /// with the timezone abbreviation appended (e.g., <c>01/15/2025 12:30 PM PST</c>).
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert and format.</param>
    /// <param name="format">The date and time format string. See <see cref="DateTimeOffset.ToString(string)"/> for valid formats.</param>
    /// <returns>The formatted Pacific Time string with the timezone abbreviation appended, or <see langword="null"/> if <paramref name="dateTime"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static string? ToPacificString(
        this DateTimeOffset? dateTime,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null) =>
        dateTime?.ToPacificString(format);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToTimeZoneString(DateTimeOffset converted, TimeZoneInfo zone, string? format)
    {
        var abbr = _zoneAbbreviations.TryGetValue(zone.Id, out var a)
            ? zone.IsDaylightSavingTime(converted) ? a.Daylight : a.Standard
            : zone.IsDaylightSavingTime(converted) ? zone.DaylightName : zone.StandardName;

        return $"{converted.ToString(format, System.Globalization.CultureInfo.CurrentCulture)} {abbr}";
    }
}
