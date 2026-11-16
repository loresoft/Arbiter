namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Extension methods for calculating business days on <see cref="DateOnly"/>, <see cref="DateTime"/>, and <see cref="DateTimeOffset"/>.
/// </summary>
/// <remarks>
/// Business days are Monday through Friday. Holidays are not taken into account.
/// </remarks>
public static class BusinessDayExtensions
{
    /// <summary>
    /// Gets the next business day by adding the specified number of days to <paramref name="date"/>,
    /// then advancing past any Saturday or Sunday.
    /// </summary>
    /// <param name="date">The starting date.</param>
    /// <param name="processDays">The number of calendar days to add before skipping weekends. Defaults to 1.</param>
    /// <returns>The resulting <see cref="DateOnly"/> that falls on a weekday.</returns>
    /// <remarks>Holidays are not taken into account.</remarks>
    public static DateOnly NextBusinessDay(this DateOnly date, int processDays = 1)
    {
        var nextDay = date.ToDateTime(TimeOnly.MinValue).AddDays(processDays);
        while (nextDay.DayOfWeek == DayOfWeek.Saturday || nextDay.DayOfWeek == DayOfWeek.Sunday)
            nextDay = nextDay.AddDays(1);

        return DateOnly.FromDateTime(nextDay);
    }

    /// <summary>
    /// Gets the next business day by adding the specified number of days to <paramref name="date"/>,
    /// then advancing past any Saturday or Sunday.
    /// </summary>
    /// <param name="date">The starting date and time.</param>
    /// <param name="processDays">The number of calendar days to add before skipping weekends. Defaults to 1.</param>
    /// <returns>The resulting <see cref="DateTime"/> that falls on a weekday, preserving the time of day.</returns>
    /// <remarks>Holidays are not taken into account.</remarks>
    public static DateTime NextBusinessDay(this DateTime date, int processDays = 1)
    {
        var nextDay = date.AddDays(processDays);
        while (nextDay.DayOfWeek == DayOfWeek.Saturday || nextDay.DayOfWeek == DayOfWeek.Sunday)
            nextDay = nextDay.AddDays(1);

        return nextDay;
    }

    /// <summary>
    /// Gets the next business day by adding the specified number of days to <paramref name="date"/>,
    /// then advancing past any Saturday or Sunday.
    /// </summary>
    /// <param name="date">The starting date and time.</param>
    /// <param name="processDays">The number of calendar days to add before skipping weekends. Defaults to 1.</param>
    /// <returns>The resulting <see cref="DateTimeOffset"/> that falls on a weekday, preserving the time of day and offset.</returns>
    /// <remarks>Holidays are not taken into account. The day of week is evaluated using the offset of <paramref name="date"/>.</remarks>
    public static DateTimeOffset NextBusinessDay(this DateTimeOffset date, int processDays = 1)
    {
        var nextDay = date.AddDays(processDays);
        while (nextDay.DayOfWeek == DayOfWeek.Saturday || nextDay.DayOfWeek == DayOfWeek.Sunday)
            nextDay = nextDay.AddDays(1);

        return nextDay;
    }
}
