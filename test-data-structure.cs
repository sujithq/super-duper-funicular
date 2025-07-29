using System.Text.Json;
using SolarScope.Models;
using SolarScope.Services;

// Simple test to verify the data structure works
var dataService = new SolarDataService("data/sample.json");

try
{
    var data = await dataService.LoadDataAsync();
    
    if (data == null)
    {
        Console.WriteLine("❌ Failed to load data");
        return;
    }
    
    Console.WriteLine("✅ Data loaded successfully!");
    Console.WriteLine($"📊 Available years: {string.Join(", ", data.AvailableYears)}");
    Console.WriteLine($"📈 Total days across all years: {data.TotalDays}");
    
    foreach (var year in data.AvailableYears)
    {
        var (production, consumption, injection) = data.GetYearlyTotals(year);
        Console.WriteLine($"📅 Year {year}: {data[year].Count} days, {production:F2} kWh production");
    }
    
    // Test that Year2023 property works
    Console.WriteLine($"🏠 2023 data: {data.Year2023.Count} days");
    
    // Test first few days
    if (data.ContainsKey(2023) && data[2023].Any())
    {
        var firstDay = data[2023].First();
        Console.WriteLine($"🔍 First day sample: Day {firstDay.Day}, {firstDay.TotalProduction:F2} kWh, {firstDay.WeatherStats.Condition}");
    }
    
    Console.WriteLine("🎉 All tests passed! The Dictionary<int, List<BarChartData>> structure is working correctly.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}
