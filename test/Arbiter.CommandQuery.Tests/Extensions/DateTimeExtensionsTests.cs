using System.Globalization;

using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class DateTimeExtensionsTests
{
    private static readonly TimeZoneInfo CentralZone = TimeZoneInfo.CreateCustomTimeZone(
        id: "Test Central",
        baseUtcOffset: TimeSpan.FromHours(-6),
        displayName: "Test Central",
        standardDisplayName: "Test Central");

    private static readonly TimeZoneInfo EasternAustraliaZone = TimeZoneInfo.CreateCustomTimeZone(
        id: "Test Eastern Australia",
        baseUtcOffset: TimeSpan.FromHours(10),
        displayName: "Test Eastern Australia",
        standardDisplayName: "Test Eastern Australia");

    private static readonly TimeZoneInfo CentralDaylightZone = CreateCentralDaylightZone();

    private static TimeZoneInfo CreateCentralDaylightZone()
    {
        // DST from the second Sunday in March to the first Sunday in November, matching US rules
        var transitionTime = new DateTime(1, 1, 1, 2, 0, 0);
        var daylightStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(transitionTime, 3, 2, DayOfWeek.Sunday);
        var daylightEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(transitionTime, 11, 1, DayOfWeek.Sunday);

        var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            dateStart: DateTime.MinValue.Date,
            dateEnd: DateTime.MaxValue.Date,
            daylightDelta: TimeSpan.FromHours(1),
            daylightTransitionStart: daylightStart,
            daylightTransitionEnd: daylightEnd);

        TimeZoneInfo.AdjustmentRule[] rules = [rule];

        return TimeZoneInfo.CreateCustomTimeZone(
            id: "Test Central Daylight",
            baseUtcOffset: TimeSpan.FromHours(-6),
            displayName: "Test Central Daylight",
            standardDisplayName: "Test Central Standard",
            daylightDisplayName: "Test Central Daylight",
            adjustmentRules: rules);
    }

    private static DateOnly ParseDate(string value) => DateOnly.Parse(value, CultureInfo.InvariantCulture);

    [Test]
    public void ToDateTimeOffset_WithTimeAndZone_ReturnsConverted()
    {
        var date = new DateOnly(2024, 3, 15);
        var time = new TimeOnly(8, 30);

        var result = date.ToDateTimeOffset(time, CentralZone);

        var expected = new DateTimeOffset(2024, 3, 15, 8, 30, 0, TimeSpan.FromHours(-6));
        result.Should().Be(expected);
        result.Offset.Should().Be(TimeSpan.FromHours(-6));
    }

    [Test]
    public void ToDateTimeOffset_WithoutTime_UsesMidnight()
    {
        var date = new DateOnly(2024, 3, 15);

        var result = date.ToDateTimeOffset(zone: CentralZone);

        var expected = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.FromHours(-6));
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateTimeOffset_WithoutZone_UsesLocalOffset()
    {
        var date = new DateOnly(2024, 3, 15);
        var time = new TimeOnly(12, 0);

        var result = date.ToDateTimeOffset(time);

        var dateTime = new DateTime(2024, 3, 15, 12, 0, 0);
        var expectedOffset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
        result.DateTime.Should().Be(dateTime);
        result.Offset.Should().Be(expectedOffset);
    }

    [Test]
    [Arguments("2024-01-15", -6)]
    [Arguments("2024-07-15", -5)]
    [Arguments("2024-12-15", -6)]
    public void ToDateTimeOffset_WithDaylightZone_UsesOffsetForDate(string value, int expectedHours)
    {
        var date = ParseDate(value);

        var result = date.ToDateTimeOffset(zone: CentralDaylightZone);

        result.Offset.Should().Be(TimeSpan.FromHours(expectedHours));
        result.Date.Should().Be(date.ToDateTime(TimeOnly.MinValue));
    }

    [Test]
    public void ToDateTimeOffset_Nullable_WithoutTime_UsesMidnight()
    {
        DateOnly? date = new DateOnly(2024, 3, 15);

        var result = date.ToDateTimeOffset(zone: CentralZone);

        var expected = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.FromHours(-6));
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateTime_WithTime_ReturnsUnspecifiedKind()
    {
        var date = new DateOnly(2024, 3, 15);
        var time = new TimeOnly(8, 30, 15);

        var result = date.ToDateTime((TimeOnly?)time);

        var expected = new DateTime(2024, 3, 15, 8, 30, 15);
        result.Should().Be(expected);
        result.Kind.Should().Be(DateTimeKind.Unspecified);
    }

    [Test]
    public void ToDateTime_WithoutTime_UsesMidnight()
    {
        var date = new DateOnly(2024, 3, 15);

        var result = date.ToDateTime();

        var expected = new DateTime(2024, 3, 15);
        result.Should().Be(expected);
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void ToDateOnly_DateTimeOffset_SameDayInZone_ReturnsDate()
    {
        var dateTime = new DateTimeOffset(2024, 3, 15, 12, 0, 0, TimeSpan.Zero);

        var result = dateTime.ToDateOnly(CentralZone);

        var expected = new DateOnly(2024, 3, 15);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateOnly_DateTimeOffset_BehindUtc_ReturnsPreviousDay()
    {
        // 2024-03-15 03:00 UTC is 2024-03-14 21:00 at UTC-6
        var dateTime = new DateTimeOffset(2024, 3, 15, 3, 0, 0, TimeSpan.Zero);

        var result = dateTime.ToDateOnly(CentralZone);

        var expected = new DateOnly(2024, 3, 14);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateOnly_DateTimeOffset_AheadOfUtc_ReturnsNextDay()
    {
        // 2024-03-15 20:00 UTC is 2024-03-16 06:00 at UTC+10
        var dateTime = new DateTimeOffset(2024, 3, 15, 20, 0, 0, TimeSpan.Zero);

        var result = dateTime.ToDateOnly(EasternAustraliaZone);

        var expected = new DateOnly(2024, 3, 16);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateOnly_DateTimeOffset_UsesInstantNotSourceOffset()
    {
        // 2024-03-15 01:00 at UTC+10 is 2024-03-14 15:00 UTC, which is 2024-03-14 09:00 at UTC-6
        var dateTime = new DateTimeOffset(2024, 3, 15, 1, 0, 0, TimeSpan.FromHours(10));

        var result = dateTime.ToDateOnly(CentralZone);

        var expected = new DateOnly(2024, 3, 14);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateOnly_DateTimeOffset_WithoutZone_UsesLocal()
    {
        var dateTime = new DateTimeOffset(2024, 3, 15, 12, 0, 0, TimeSpan.Zero);

        var result = dateTime.ToDateOnly();

        var localDateTime = TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Local);
        var expected = DateOnly.FromDateTime(localDateTime.DateTime);
        result.Should().Be(expected);
    }

    [Test]
    [Arguments(DateTimeKind.Unspecified)]
    [Arguments(DateTimeKind.Utc)]
    [Arguments(DateTimeKind.Local)]
    public void ToDateOnly_DateTime_DropsTimeWithoutConversion(DateTimeKind kind)
    {
        var dateTime = new DateTime(2024, 3, 15, 23, 59, 59, kind);

        var result = dateTime.ToDateOnly();

        var expected = new DateOnly(2024, 3, 15);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateOnly_NullableDateTimeOffset_WithoutZone_UsesLocal()
    {
        DateTimeOffset? dateTime = new DateTimeOffset(2024, 3, 15, 12, 0, 0, TimeSpan.Zero);

        var result = dateTime.ToDateOnly();

        var expected = dateTime.Value.ToDateOnly(TimeZoneInfo.Local);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateTimeOffset_Nullable_WithValue_ReturnsConverted()
    {
        DateOnly? date = new DateOnly(2024, 3, 15);
        var time = new TimeOnly(8, 30);

        var result = date.ToDateTimeOffset(time, CentralZone);

        var expected = new DateTimeOffset(2024, 3, 15, 8, 30, 0, TimeSpan.FromHours(-6));
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateTimeOffset_Nullable_WithNull_ReturnsNull()
    {
        DateOnly? date = null;

        var result = date.ToDateTimeOffset(zone: CentralZone);

        result.Should().BeNull();
    }

    [Test]
    public void ToDateTime_Nullable_WithValue_ReturnsConverted()
    {
        DateOnly? date = new DateOnly(2024, 3, 15);
        var time = new TimeOnly(8, 30);

        var result = date.ToDateTime(time);

        var expected = new DateTime(2024, 3, 15, 8, 30, 0);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateTime_Nullable_WithoutTime_UsesMidnight()
    {
        DateOnly? date = new DateOnly(2024, 3, 15);

        var result = date.ToDateTime();

        var expected = new DateTime(2024, 3, 15);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateTime_Nullable_WithNull_ReturnsNull()
    {
        DateOnly? date = null;

        var result = date.ToDateTime();

        result.Should().BeNull();
    }

    [Test]
    public void ToDateOnly_NullableDateTimeOffset_WithValue_ConvertsToZone()
    {
        // 2024-03-15 03:00 UTC is 2024-03-14 21:00 at UTC-6
        DateTimeOffset? dateTime = new DateTimeOffset(2024, 3, 15, 3, 0, 0, TimeSpan.Zero);

        var result = dateTime.ToDateOnly(CentralZone);

        var expected = new DateOnly(2024, 3, 14);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateOnly_NullableDateTimeOffset_WithNull_ReturnsNull()
    {
        DateTimeOffset? dateTime = null;

        var result = dateTime.ToDateOnly(CentralZone);

        result.Should().BeNull();
    }

    [Test]
    public void ToDateOnly_NullableDateTime_WithValue_ReturnsDate()
    {
        DateTime? dateTime = new DateTime(2024, 3, 15, 23, 59, 59);

        var result = dateTime.ToDateOnly();

        var expected = new DateOnly(2024, 3, 15);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDateOnly_NullableDateTime_WithNull_ReturnsNull()
    {
        DateTime? dateTime = null;

        var result = dateTime.ToDateOnly();

        result.Should().BeNull();
    }
}
