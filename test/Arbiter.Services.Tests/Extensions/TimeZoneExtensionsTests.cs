using System.Globalization;

using Arbiter.Services.Extensions;

namespace Arbiter.Services.Tests.Extensions;

public class TimeZoneExtensionsTests
{
    private static DateTimeOffset? ConvertNullable(string zone, DateTimeOffset? value) => zone switch
    {
        "Eastern" => value.ToEastern(),
        "Central" => value.ToCentral(),
        "Mountain" => value.ToMountain(),
        "Pacific" => value.ToPacific(),
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    private static DateTimeOffset? ConvertNullable(string zone, DateTime? value) => zone switch
    {
        "Eastern" => value.ToEastern(),
        "Central" => value.ToCentral(),
        "Mountain" => value.ToMountain(),
        "Pacific" => value.ToPacific(),
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    private static DateTimeOffset? ConvertNullable(string zone, DateOnly? value, TimeOnly? time = null) => zone switch
    {
        "Eastern" => value.ToEastern(time),
        "Central" => value.ToCentral(time),
        "Mountain" => value.ToMountain(time),
        "Pacific" => value.ToPacific(time),
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    private static string? FormatNullable(string zone, DateTimeOffset? value, string? format) => zone switch
    {
        "Eastern" => value.ToEasternString(format),
        "Central" => value.ToCentralString(format),
        "Mountain" => value.ToMountainString(format),
        "Pacific" => value.ToPacificString(format),
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    // 2025-01-15 20:30 UTC is standard time in all US zones
    [Test]
    [Arguments("Eastern", 15, -5)]
    [Arguments("Central", 14, -6)]
    [Arguments("Mountain", 13, -7)]
    [Arguments("Pacific", 12, -8)]
    public void NullableDateTimeOffset_WithValue_ConvertsToZone(string zone, int expectedHour, int expectedOffset)
    {
        DateTimeOffset? value = new DateTimeOffset(2025, 1, 15, 20, 30, 0, TimeSpan.Zero);

        var result = ConvertNullable(zone, value);

        var offset = TimeSpan.FromHours(expectedOffset);
        var expected = new DateTimeOffset(2025, 1, 15, expectedHour, 30, 0, offset);
        result.Should().Be(expected);
        result!.Value.Offset.Should().Be(offset);
    }

    [Test]
    [Arguments("Eastern")]
    [Arguments("Central")]
    [Arguments("Mountain")]
    [Arguments("Pacific")]
    public void NullableDateTimeOffset_WithNull_ReturnsNull(string zone)
    {
        DateTimeOffset? value = null;

        var result = ConvertNullable(zone, value);

        result.Should().BeNull();
    }

    [Test]
    [Arguments("Eastern", 15, -5)]
    [Arguments("Central", 14, -6)]
    [Arguments("Mountain", 13, -7)]
    [Arguments("Pacific", 12, -8)]
    public void NullableDateTime_WithUtcValue_ConvertsToZone(string zone, int expectedHour, int expectedOffset)
    {
        DateTime? value = new DateTime(2025, 1, 15, 20, 30, 0, DateTimeKind.Utc);

        var result = ConvertNullable(zone, value);

        var offset = TimeSpan.FromHours(expectedOffset);
        var expected = new DateTimeOffset(2025, 1, 15, expectedHour, 30, 0, offset);
        result.Should().Be(expected);
        result!.Value.Offset.Should().Be(offset);
    }

    [Test]
    [Arguments("Eastern")]
    [Arguments("Central")]
    [Arguments("Mountain")]
    [Arguments("Pacific")]
    public void NullableDateTime_WithNull_ReturnsNull(string zone)
    {
        DateTime? value = null;

        var result = ConvertNullable(zone, value);

        result.Should().BeNull();
    }

    [Test]
    [Arguments("Eastern")]
    [Arguments("Central")]
    [Arguments("Mountain")]
    [Arguments("Pacific")]
    public void NullableDateTime_WithUnspecifiedKind_Throws(string zone)
    {
        DateTime? value = new DateTime(2025, 1, 15, 20, 30, 0, DateTimeKind.Unspecified);

        var action = () => ConvertNullable(zone, value);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("dateTime");
    }

    // 2025-07-04 is daylight saving time in all US zones
    [Test]
    [Arguments("Eastern", -4)]
    [Arguments("Central", -5)]
    [Arguments("Mountain", -6)]
    [Arguments("Pacific", -7)]
    public void NullableDateOnly_WithValueAndTime_UsesZoneOffset(string zone, int expectedOffset)
    {
        DateOnly? value = new DateOnly(2025, 7, 4);
        var time = new TimeOnly(9, 15);

        var result = ConvertNullable(zone, value, time);

        var offset = TimeSpan.FromHours(expectedOffset);
        var expected = new DateTimeOffset(2025, 7, 4, 9, 15, 0, offset);
        result.Should().Be(expected);
        result!.Value.Offset.Should().Be(offset);
    }

    [Test]
    [Arguments("Eastern", -5)]
    [Arguments("Central", -6)]
    [Arguments("Mountain", -7)]
    [Arguments("Pacific", -8)]
    public void NullableDateOnly_WithoutTime_UsesMidnight(string zone, int expectedOffset)
    {
        DateOnly? value = new DateOnly(2025, 1, 15);

        var result = ConvertNullable(zone, value);

        var offset = TimeSpan.FromHours(expectedOffset);
        var expected = new DateTimeOffset(2025, 1, 15, 0, 0, 0, offset);
        result.Should().Be(expected);
        result!.Value.Offset.Should().Be(offset);
    }

    [Test]
    [Arguments("Eastern")]
    [Arguments("Central")]
    [Arguments("Mountain")]
    [Arguments("Pacific")]
    public void NullableDateOnly_WithNull_ReturnsNull(string zone)
    {
        DateOnly? value = null;
        var time = new TimeOnly(9, 15);

        var result = ConvertNullable(zone, value, time);

        result.Should().BeNull();
    }

    [Test]
    [Arguments("Eastern", "2025-01-15 15:30 EST")]
    [Arguments("Central", "2025-01-15 14:30 CST")]
    [Arguments("Mountain", "2025-01-15 13:30 MST")]
    [Arguments("Pacific", "2025-01-15 12:30 PST")]
    public void NullableString_WithStandardTimeValue_FormatsWithAbbreviation(string zone, string expected)
    {
        DateTimeOffset? value = new DateTimeOffset(2025, 1, 15, 20, 30, 0, TimeSpan.Zero);

        var result = FormatNullable(zone, value, "yyyy-MM-dd HH:mm");

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("Eastern", "2025-07-04 16:30 EDT")]
    [Arguments("Central", "2025-07-04 15:30 CDT")]
    [Arguments("Mountain", "2025-07-04 14:30 MDT")]
    [Arguments("Pacific", "2025-07-04 13:30 PDT")]
    public void NullableString_WithDaylightTimeValue_FormatsWithAbbreviation(string zone, string expected)
    {
        DateTimeOffset? value = new DateTimeOffset(2025, 7, 4, 20, 30, 0, TimeSpan.Zero);

        var result = FormatNullable(zone, value, "yyyy-MM-dd HH:mm");

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("Eastern")]
    [Arguments("Central")]
    [Arguments("Mountain")]
    [Arguments("Pacific")]
    public void NullableString_WithNull_ReturnsNull(string zone)
    {
        DateTimeOffset? value = null;

        var result = FormatNullable(zone, value, "yyyy-MM-dd HH:mm");

        result.Should().BeNull();
    }

    private static TimeZoneInfo GetZone(string zone) => zone switch
    {
        "Eastern" => TimeZoneExtensions.Eastern,
        "Central" => TimeZoneExtensions.Central,
        "Mountain" => TimeZoneExtensions.Mountain,
        "Pacific" => TimeZoneExtensions.Pacific,
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    private static DateTimeOffset ConvertValue(string zone, DateTimeOffset value) => zone switch
    {
        "Eastern" => value.ToEastern(),
        "Central" => value.ToCentral(),
        "Mountain" => value.ToMountain(),
        "Pacific" => value.ToPacific(),
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    private static DateTimeOffset ConvertValue(string zone, DateTime value) => zone switch
    {
        "Eastern" => value.ToEastern(),
        "Central" => value.ToCentral(),
        "Mountain" => value.ToMountain(),
        "Pacific" => value.ToPacific(),
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    private static DateTimeOffset ConvertValue(string zone, DateOnly value, TimeOnly? time = null) => zone switch
    {
        "Eastern" => value.ToEastern(time),
        "Central" => value.ToCentral(time),
        "Mountain" => value.ToMountain(time),
        "Pacific" => value.ToPacific(time),
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    private static string FormatValue(string zone, DateTimeOffset value, string? format) => zone switch
    {
        "Eastern" => value.ToEasternString(format),
        "Central" => value.ToCentralString(format),
        "Mountain" => value.ToMountainString(format),
        "Pacific" => value.ToPacificString(format),
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    private static string FormatWithCulture(string zone, DateTimeOffset value, string? format, CultureInfo culture)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            return FormatValue(zone, value, format);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Test]
    [Arguments("Eastern", "America/New_York", -5)]
    [Arguments("Central", "America/Chicago", -6)]
    [Arguments("Mountain", "America/Denver", -7)]
    [Arguments("Pacific", "America/Los_Angeles", -8)]
    public void Zone_HasExpectedIdAndBaseOffset(string zone, string expectedId, int expectedBaseOffset)
    {
        var timeZone = GetZone(zone);

        timeZone.Id.Should().Be(expectedId);
        timeZone.BaseUtcOffset.Should().Be(TimeSpan.FromHours(expectedBaseOffset));
        timeZone.SupportsDaylightSavingTime.Should().BeTrue();
    }

    [Test]
    [Arguments("Eastern", 1, 15, -5)]
    [Arguments("Central", 1, 14, -6)]
    [Arguments("Mountain", 1, 13, -7)]
    [Arguments("Pacific", 1, 12, -8)]
    [Arguments("Eastern", 7, 16, -4)]
    [Arguments("Central", 7, 15, -5)]
    [Arguments("Mountain", 7, 14, -6)]
    [Arguments("Pacific", 7, 13, -7)]
    public void DateTimeOffset_ConvertsToZone(string zone, int month, int expectedHour, int expectedOffset)
    {
        var value = new DateTimeOffset(2025, month, 15, 20, 30, 0, TimeSpan.Zero);

        var result = ConvertValue(zone, value);

        var offset = TimeSpan.FromHours(expectedOffset);
        var expected = new DateTimeOffset(2025, month, 15, expectedHour, 30, 0, offset);
        result.Should().Be(expected);
        result.Offset.Should().Be(offset);
    }

    [Test]
    [Arguments("Eastern")]
    [Arguments("Central")]
    [Arguments("Mountain")]
    [Arguments("Pacific")]
    public void DateTimeOffset_FromNonUtcOffset_PreservesInstant(string zone)
    {
        var sourceOffset = TimeSpan.FromHours(9);
        var value = new DateTimeOffset(2025, 1, 16, 5, 30, 0, sourceOffset);

        var result = ConvertValue(zone, value);

        var expectedOffset = GetZone(zone).GetUtcOffset(value);
        result.UtcDateTime.Should().Be(value.UtcDateTime);
        result.Offset.Should().Be(expectedOffset);
    }

    // Spring forward on 2025-03-09 at 02:00 local standard time
    [Test]
    [Arguments("Eastern", 7, -5)]
    [Arguments("Central", 8, -6)]
    [Arguments("Mountain", 9, -7)]
    [Arguments("Pacific", 10, -8)]
    public void DateTimeOffset_AtSpringForward_SwitchesToDaylightOffset(string zone, int transitionUtcHour, int standardOffset)
    {
        var transition = new DateTimeOffset(2025, 3, 9, transitionUtcHour, 0, 0, TimeSpan.Zero);
        var beforeTransition = transition.AddMinutes(-1);

        var before = ConvertValue(zone, beforeTransition);
        var after = ConvertValue(zone, transition);

        var standard = TimeSpan.FromHours(standardOffset);
        var daylight = standard + TimeSpan.FromHours(1);
        var expectedBefore = new DateTimeOffset(2025, 3, 9, 1, 59, 0, standard);
        var expectedAfter = new DateTimeOffset(2025, 3, 9, 3, 0, 0, daylight);
        before.Should().Be(expectedBefore);
        before.Offset.Should().Be(standard);
        after.Should().Be(expectedAfter);
        after.Offset.Should().Be(daylight);
    }

    // Fall back on 2025-11-02 at 02:00 local daylight time
    [Test]
    [Arguments("Eastern", 6, -5)]
    [Arguments("Central", 7, -6)]
    [Arguments("Mountain", 8, -7)]
    [Arguments("Pacific", 9, -8)]
    public void DateTimeOffset_AtFallBack_SwitchesToStandardOffset(string zone, int transitionUtcHour, int standardOffset)
    {
        var transition = new DateTimeOffset(2025, 11, 2, transitionUtcHour, 0, 0, TimeSpan.Zero);
        var beforeTransition = transition.AddMinutes(-1);

        var before = ConvertValue(zone, beforeTransition);
        var after = ConvertValue(zone, transition);

        var standard = TimeSpan.FromHours(standardOffset);
        var daylight = standard + TimeSpan.FromHours(1);
        var expectedBefore = new DateTime(2025, 11, 2, 1, 59, 0);
        var expectedAfter = new DateTime(2025, 11, 2, 1, 0, 0);
        before.DateTime.Should().Be(expectedBefore);
        before.Offset.Should().Be(daylight);
        after.DateTime.Should().Be(expectedAfter);
        after.Offset.Should().Be(standard);
    }

    [Test]
    [Arguments("Eastern", 1, 15, -5)]
    [Arguments("Central", 1, 14, -6)]
    [Arguments("Mountain", 1, 13, -7)]
    [Arguments("Pacific", 1, 12, -8)]
    [Arguments("Eastern", 7, 16, -4)]
    [Arguments("Central", 7, 15, -5)]
    [Arguments("Mountain", 7, 14, -6)]
    [Arguments("Pacific", 7, 13, -7)]
    public void DateTime_WithUtcKind_ConvertsToZone(string zone, int month, int expectedHour, int expectedOffset)
    {
        var value = new DateTime(2025, month, 15, 20, 30, 0, DateTimeKind.Utc);

        var result = ConvertValue(zone, value);

        var offset = TimeSpan.FromHours(expectedOffset);
        var expected = new DateTimeOffset(2025, month, 15, expectedHour, 30, 0, offset);
        result.Should().Be(expected);
        result.Offset.Should().Be(offset);
    }

    [Test]
    [Arguments("Eastern")]
    [Arguments("Central")]
    [Arguments("Mountain")]
    [Arguments("Pacific")]
    public void DateTime_WithLocalKind_ConvertsFromLocalInstant(string zone)
    {
        var value = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Local);

        var result = ConvertValue(zone, value);

        var expectedUtc = value.ToUniversalTime();
        var expectedOffset = GetZone(zone).GetUtcOffset(expectedUtc);
        result.UtcDateTime.Should().Be(expectedUtc);
        result.Offset.Should().Be(expectedOffset);
    }

    [Test]
    [Arguments("Eastern")]
    [Arguments("Central")]
    [Arguments("Mountain")]
    [Arguments("Pacific")]
    public void DateTime_WithUnspecifiedKind_Throws(string zone)
    {
        var value = new DateTime(2025, 1, 15, 20, 30, 0, DateTimeKind.Unspecified);

        var action = () => ConvertValue(zone, value);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("dateTime")
            .WithMessage("DateTime.Kind must not be Unspecified.*");
    }

    [Test]
    [Arguments("Eastern", 1, -5)]
    [Arguments("Central", 1, -6)]
    [Arguments("Mountain", 1, -7)]
    [Arguments("Pacific", 1, -8)]
    [Arguments("Eastern", 7, -4)]
    [Arguments("Central", 7, -5)]
    [Arguments("Mountain", 7, -6)]
    [Arguments("Pacific", 7, -7)]
    public void DateOnly_WithTime_KeepsLocalTimeAndUsesZoneOffset(string zone, int month, int expectedOffset)
    {
        var value = new DateOnly(2025, month, 15);
        var time = new TimeOnly(9, 15, 30);

        var result = ConvertValue(zone, value, time);

        var offset = TimeSpan.FromHours(expectedOffset);
        var expected = new DateTimeOffset(2025, month, 15, 9, 15, 30, offset);
        result.Should().Be(expected);
        result.Offset.Should().Be(offset);
    }

    [Test]
    [Arguments("Eastern", -4)]
    [Arguments("Central", -5)]
    [Arguments("Mountain", -6)]
    [Arguments("Pacific", -7)]
    public void DateOnly_WithoutTime_UsesMidnight(string zone, int expectedOffset)
    {
        var value = new DateOnly(2025, 7, 4);

        var result = ConvertValue(zone, value);

        var offset = TimeSpan.FromHours(expectedOffset);
        var expected = new DateTimeOffset(2025, 7, 4, 0, 0, 0, offset);
        result.Should().Be(expected);
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Test]
    [Arguments("Eastern", -5)]
    [Arguments("Central", -6)]
    [Arguments("Mountain", -7)]
    [Arguments("Pacific", -8)]
    public void DateOnly_WithTimeInSpringForwardGap_UsesStandardOffset(string zone, int standardOffset)
    {
        // 02:30 does not exist on 2025-03-09; TimeZoneInfo treats invalid times as standard time
        var value = new DateOnly(2025, 3, 9);
        var time = new TimeOnly(2, 30);

        var result = ConvertValue(zone, value, time);

        var offset = TimeSpan.FromHours(standardOffset);
        var expectedDateTime = new DateTime(2025, 3, 9, 2, 30, 0);
        result.DateTime.Should().Be(expectedDateTime);
        result.Offset.Should().Be(offset);
    }

    [Test]
    [Arguments("Eastern", -5)]
    [Arguments("Central", -6)]
    [Arguments("Mountain", -7)]
    [Arguments("Pacific", -8)]
    public void DateOnly_WithAmbiguousFallBackTime_UsesStandardOffset(string zone, int standardOffset)
    {
        // 01:30 occurs twice on 2025-11-02; TimeZoneInfo treats ambiguous times as standard time
        var value = new DateOnly(2025, 11, 2);
        var time = new TimeOnly(1, 30);

        var result = ConvertValue(zone, value, time);

        var offset = TimeSpan.FromHours(standardOffset);
        var expectedDateTime = new DateTime(2025, 11, 2, 1, 30, 0);
        result.DateTime.Should().Be(expectedDateTime);
        result.Offset.Should().Be(offset);
    }

    [Test]
    [Arguments("Eastern", "01/15/2025 3:30 PM EST")]
    [Arguments("Central", "01/15/2025 2:30 PM CST")]
    [Arguments("Mountain", "01/15/2025 1:30 PM MST")]
    [Arguments("Pacific", "01/15/2025 12:30 PM PST")]
    public void String_WithCustomFormat_FormatsWithAbbreviation(string zone, string expected)
    {
        var value = new DateTimeOffset(2025, 1, 15, 20, 30, 0, TimeSpan.Zero);

        var result = FormatWithCulture(zone, value, "MM/dd/yyyy h:mm tt", CultureInfo.InvariantCulture);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("Eastern", "01/15/2025 15:30:00 -05:00 EST")]
    [Arguments("Central", "01/15/2025 14:30:00 -06:00 CST")]
    [Arguments("Mountain", "01/15/2025 13:30:00 -07:00 MST")]
    [Arguments("Pacific", "01/15/2025 12:30:00 -08:00 PST")]
    public void String_WithDefaultFormat_UsesCurrentCulture(string zone, string expected)
    {
        var value = new DateTimeOffset(2025, 1, 15, 20, 30, 0, TimeSpan.Zero);

        var result = FormatWithCulture(zone, value, null, CultureInfo.InvariantCulture);

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("Eastern", "2025-01-15 15:30 EST")]
    [Arguments("Central", "2025-01-15 14:30 CST")]
    [Arguments("Mountain", "2025-01-15 13:30 MST")]
    [Arguments("Pacific", "2025-01-15 12:30 PST")]
    public void String_FromNonUtcOffset_ConvertsBeforeFormatting(string zone, string expected)
    {
        // 2025-01-16 05:30 at UTC+9 is 2025-01-15 20:30 UTC
        var sourceOffset = TimeSpan.FromHours(9);
        var value = new DateTimeOffset(2025, 1, 16, 5, 30, 0, sourceOffset);

        var result = FormatValue(zone, value, "yyyy-MM-dd HH:mm");

        result.Should().Be(expected);
    }

    [Test]
    [Arguments("Eastern", 7, "EST", "EDT")]
    [Arguments("Central", 8, "CST", "CDT")]
    [Arguments("Mountain", 9, "MST", "MDT")]
    [Arguments("Pacific", 10, "PST", "PDT")]
    public void String_AtSpringForward_SwitchesAbbreviation(string zone, int transitionUtcHour, string standard, string daylight)
    {
        var transition = new DateTimeOffset(2025, 3, 9, transitionUtcHour, 0, 0, TimeSpan.Zero);
        var beforeTransition = transition.AddMinutes(-1);

        var before = FormatValue(zone, beforeTransition, "yyyy-MM-dd HH:mm");
        var after = FormatValue(zone, transition, "yyyy-MM-dd HH:mm");

        before.Should().Be($"2025-03-09 01:59 {standard}");
        after.Should().Be($"2025-03-09 03:00 {daylight}");
    }

    [Test]
    [Arguments("Eastern", 6, "EST", "EDT")]
    [Arguments("Central", 7, "CST", "CDT")]
    [Arguments("Mountain", 8, "MST", "MDT")]
    [Arguments("Pacific", 9, "PST", "PDT")]
    public void String_AtFallBack_SwitchesAbbreviation(string zone, int transitionUtcHour, string standard, string daylight)
    {
        var transition = new DateTimeOffset(2025, 11, 2, transitionUtcHour, 0, 0, TimeSpan.Zero);
        var beforeTransition = transition.AddMinutes(-1);

        var before = FormatValue(zone, beforeTransition, "yyyy-MM-dd HH:mm");
        var after = FormatValue(zone, transition, "yyyy-MM-dd HH:mm");

        before.Should().Be($"2025-11-02 01:59 {daylight}");
        after.Should().Be($"2025-11-02 01:00 {standard}");
    }

    [Test]
    [Arguments("Eastern")]
    [Arguments("Central")]
    [Arguments("Mountain")]
    [Arguments("Pacific")]
    public void NullableString_WithValue_MatchesNonNullable(string zone)
    {
        var value = new DateTimeOffset(2025, 7, 4, 20, 30, 0, TimeSpan.Zero);
        DateTimeOffset? nullableValue = value;

        var result = FormatNullable(zone, nullableValue, "O");

        var expected = FormatValue(zone, value, "O");
        result.Should().Be(expected);
    }
}
