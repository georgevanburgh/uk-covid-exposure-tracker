using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;

// PARAMS
const int DAYS_LOOKBACK = 14;
string EXPOSURE_FILE = @"exposure-stats.csv";

// Look back N days
var dates = Enumerable.Range(0, DAYS_LOOKBACK).Select(x => DateTime.Today.AddDays(x * -1));

// Fetch number of compromised keys for each day
var tasks = dates.Select(x => ExposureKeyStatsGenerator.GetNumberOfExposuresForDate(x));
var stats = (await Task.WhenAll(tasks)).ToList();

// Dedupe with existing stats
var existing = ReadExistingCsv(EXPOSURE_FILE);
var toSave = existing != null ? MergeStats(existing, stats) : stats;

foreach (var stat in toSave)
    Console.WriteLine($"{stat.Date.ToShortDateString()}: {stat.Count}");

WriteRecords(toSave.OrderByDescending(x => x.Date), EXPOSURE_FILE);

static List<ExposureStat> MergeStats(IEnumerable<ExposureStat> first, IEnumerable<ExposureStat> second) => first
        .Union(second)
        .GroupBy(x => x.Date)
        .Select(g => new ExposureStat { Date = g.Key, Count = g.Max(m => m.Count) })
        .OrderByDescending(o => o.Date)
        .ToList();

static List<ExposureStat> ReadExistingCsv(string fileName)
{
    if (!File.Exists(fileName)) return null;

    using (var reader = new StreamReader(fileName))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        csv.Context.RegisterClassMap<ExposureCsvMap>();
        return csv.GetRecords<ExposureStat>().ToList();
    }
}

static void WriteRecords(IEnumerable<ExposureStat> stats, string fileName)
{
    using (var writer = new StreamWriter(fileName))
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        csv.Context.RegisterClassMap<ExposureCsvMap>();
        csv.WriteRecords(stats);
    }
}

class ExposureCsvMap : ClassMap<ExposureStat>
{
    public ExposureCsvMap()
    {
        Map(x => x.Date).TypeConverterOption.Format("yyyyMMdd");
        Map(x => x.Count);
    }
}