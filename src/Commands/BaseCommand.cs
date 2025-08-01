using Spectre.Console;
using Spectre.Console.Cli;
using SolarScope.Models;

namespace SolarScope.Commands;

/// <summary>
/// Base command class with common functionality for all solar commands
/// </summary>
public abstract class BaseCommand<TSettings> : AsyncCommand<TSettings> where TSettings : BaseCommandSettings
{
    /// <summary>
    /// Provides default validation - can be overridden by derived commands
    /// </summary>
    public override ValidationResult Validate(CommandContext context, TSettings settings)
    {
        return ValidationResult.Success();
    }

    /// <summary>
    /// Converts a day-of-year number to a user-friendly date string
    /// </summary>
    /// <param name="dayOfYear">Day of year (1-365/366)</param>
    /// <param name="year">The year</param>
    /// <returns>Formatted date string (e.g., "May 03, 2023") or "Day X" if conversion fails</returns>
    protected static string GetDateFromDayOfYear(int dayOfYear, int year)
    {
        try
        {
            var date = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
            return date.ToString("MMM dd, yyyy", System.Globalization.CultureInfo.CurrentCulture);
        }
        catch
        {
            return $"Day {dayOfYear}";
        }
    }

    /// <summary>
    /// Gets the appropriate year to use for date formatting from the data
    /// </summary>
    /// <param name="data">Solar data</param>
    /// <param name="preferredYear">Preferred year (nullable)</param>
    /// <returns>Year to use for date conversion</returns>
    protected static int GetYearForDisplay(SolarData data, int? preferredYear = null)
    {
        return preferredYear ?? data.LatestYear;
    }

    /// <summary>
    /// Formats a day entry with a user-friendly date
    /// </summary>
    /// <param name="day">Day data</param>
    /// <param name="year">Year for date conversion</param>
    /// <returns>Formatted date string</returns>
    protected static string FormatDayAsDate(BarChartData day, int year)
    {
        return GetDateFromDayOfYear(day.D, year);
    }
    
    /// <summary>
    /// Formats a BarChartDataWithYear entry with a user-friendly date (using the included year)
    /// </summary>
    /// <param name="dayWithYear">Day data with year information</param>
    /// <returns>Formatted date string</returns>
    protected static string FormatDayAsDate(BarChartDataWithYear dayWithYear)
    {
        return dayWithYear.FormattedDate;
    }
}
