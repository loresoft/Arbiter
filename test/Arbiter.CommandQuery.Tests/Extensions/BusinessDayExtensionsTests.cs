using System.Globalization;

using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class BusinessDayExtensionsTests
{
    private static DateOnly ParseDate(string value) => DateOnly.Parse(value, CultureInfo.InvariantCulture);

    // 2024-03-11 is a Monday; 2024-03-16 and 2024-03-17 are a weekend
    [Test]
    [Arguments("2024-03-11", 1, "2024-03-12")]
    [Arguments("2024-03-14", 1, "2024-03-15")]
    [Arguments("2024-03-15", 1, "2024-03-18")]
    [Arguments("2024-03-16", 1, "2024-03-18")]
    [Arguments("2024-03-17", 1, "2024-03-18")]
    [Arguments("2024-03-15", 2, "2024-03-18")]
    [Arguments("2024-03-13", 3, "2024-03-18")]
    [Arguments("2024-03-11", 5, "2024-03-18")]
    [Arguments("2024-03-11", 7, "2024-03-18")]
    [Arguments("2024-03-13", 0, "2024-03-13")]
    [Arguments("2024-03-16", 0, "2024-03-18")]
    [Arguments("2024-02-28", 1, "2024-02-29")]
    [Arguments("2024-12-27", 1, "2024-12-30")]
    [Arguments("2025-01-31", 1, "2025-02-03")]
    public void NextBusinessDay_DateOnly_ReturnsWeekday(string start, int processDays, string expected)
    {
        var date = ParseDate(start);

        var result = date.NextBusinessDay(processDays);

        var expectedDate = ParseDate(expected);
        result.Should().Be(expectedDate);
        result.DayOfWeek.Should().NotBe(DayOfWeek.Saturday).And.NotBe(DayOfWeek.Sunday);
    }

    [Test]
    public void NextBusinessDay_DateOnly_DefaultsToOneDay()
    {
        var date = new DateOnly(2024, 3, 12);

        var result = date.NextBusinessDay();

        var expected = new DateOnly(2024, 3, 13);
        result.Should().Be(expected);
    }

    [Test]
    [Arguments("2024-03-11", 1, "2024-03-12")]
    [Arguments("2024-03-15", 1, "2024-03-18")]
    [Arguments("2024-03-16", 1, "2024-03-18")]
    [Arguments("2024-03-17", 1, "2024-03-18")]
    [Arguments("2024-03-13", 3, "2024-03-18")]
    [Arguments("2024-03-16", 0, "2024-03-18")]
    [Arguments("2024-12-27", 1, "2024-12-30")]
    public void NextBusinessDay_DateTime_ReturnsWeekday(string start, int processDays, string expected)
    {
        var time = new TimeOnly(14, 30);
        var date = ParseDate(start).ToDateTime(time);

        var result = date.NextBusinessDay(processDays);

        var expectedDate = ParseDate(expected).ToDateTime(time);
        result.Should().Be(expectedDate);
    }

    [Test]
    [Arguments(DateTimeKind.Unspecified)]
    [Arguments(DateTimeKind.Utc)]
    [Arguments(DateTimeKind.Local)]
    public void NextBusinessDay_DateTime_PreservesTimeAndKind(DateTimeKind kind)
    {
        var date = new DateTime(2024, 3, 15, 14, 30, 45, kind);

        var result = date.NextBusinessDay();

        var expected = new DateTime(2024, 3, 18, 14, 30, 45, kind);
        result.Should().Be(expected);
        result.Kind.Should().Be(kind);
    }

    [Test]
    [Arguments("2024-03-11", 1, "2024-03-12")]
    [Arguments("2024-03-15", 1, "2024-03-18")]
    [Arguments("2024-03-16", 1, "2024-03-18")]
    [Arguments("2024-03-17", 1, "2024-03-18")]
    [Arguments("2024-03-13", 3, "2024-03-18")]
    [Arguments("2024-03-16", 0, "2024-03-18")]
    [Arguments("2024-12-27", 1, "2024-12-30")]
    public void NextBusinessDay_DateTimeOffset_ReturnsWeekday(string start, int processDays, string expected)
    {
        var time = new TimeOnly(14, 30);
        var offset = TimeSpan.FromHours(-6);
        var startDateTime = ParseDate(start).ToDateTime(time);
        var date = new DateTimeOffset(startDateTime, offset);

        var result = date.NextBusinessDay(processDays);

        var expectedDateTime = ParseDate(expected).ToDateTime(time);
        var expectedDate = new DateTimeOffset(expectedDateTime, offset);
        result.Should().Be(expectedDate);
        result.Offset.Should().Be(offset);
    }

    [Test]
    public void NextBusinessDay_DateTimeOffset_UsesLocalDayOfWeekForOffset()
    {
        // Saturday 05:00 at UTC+10 is still Friday 19:00 UTC; the weekend is evaluated at the value's own offset
        var offset = TimeSpan.FromHours(10);
        var date = new DateTimeOffset(2024, 3, 16, 5, 0, 0, offset);

        var result = date.NextBusinessDay(0);

        var expected = new DateTimeOffset(2024, 3, 18, 5, 0, 0, offset);
        result.Should().Be(expected);
        result.Offset.Should().Be(offset);
    }
}
