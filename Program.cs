using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;

namespace nhs_proto_test
{
    class Program
    {
        private const int DAYS_LOOKBACK = 14;
        private const string EXPOSURE_FILE = "exposure-stats.csv";

        static async Task Main(string[] args)
        {
            // Look back N days
            var dates = Enumerable.Range(0, DAYS_LOOKBACK).Select(x => DateTime.Today.AddDays(x * -1));

            // Fetch number of compromised keys for each day
            var tasks = dates.Select(x => ExposureKeyStatsGenerator.GetNumberOfExposuresForDate(x));
            await Task.WhenAll(tasks);
            var stats = tasks.Select(x => x.Result);

            // Dedupe with existing stats
            var existing = ReadExistingCsv();
            var toSave = existing != null ? MergeStats(existing, stats) : stats;

            foreach (var stat in toSave)
                Console.WriteLine($"{stat.Date.ToShortDateString()}: {stat.Count}");

            WriteRecords(toSave.OrderByDescending(x => x.Date));
        }

        private static List<ExposureStat> MergeStats(IEnumerable<ExposureStat> first, IEnumerable<ExposureStat> second) => first
                .Union(second)
                .GroupBy(x => x.Date)
                .Select(g => new ExposureStat { Date = g.Key, Count = g.Max(m => m.Count) })
                .OrderByDescending(o => o.Date)
                .ToList();

        private static List<ExposureStat> ReadExistingCsv()
        {
            if (!File.Exists(EXPOSURE_FILE)) return null;

            using (var reader = new StreamReader(EXPOSURE_FILE))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.RegisterClassMap<ExposureCsvMap>();
                return csv.GetRecords<ExposureStat>().ToList();
            }
        }

        private static void WriteRecords(IEnumerable<ExposureStat> stats)
        {
            using (var writer = new StreamWriter(EXPOSURE_FILE))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Configuration.RegisterClassMap<ExposureCsvMap>();
                csv.WriteRecords(stats);
            }
        }

        private class ExposureCsvMap : ClassMap<ExposureStat>
        {
            public ExposureCsvMap()
            {
                Map(x => x.Date).TypeConverterOption.Format("yyyyMMdd");
                Map(x => x.Count);
            }
        }
    }
}
