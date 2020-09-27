using System;
using System.Threading.Tasks;
using System.Linq;

namespace nhs_proto_test
{
    class Program
    {
        private const int DAYS_LOOKBACK = 13;
        static async Task Main(string[] args)
        {
            var dates = Enumerable.Range(1, DAYS_LOOKBACK).Select(x => DateTime.Today.AddDays(x * -1));

            foreach (var date in dates)
                Console.WriteLine($"{date.ToShortDateString()}: {await ExposureKeyStatsGenerator.GetNumberOfExposuresForDate(date)}");   
        }
    }
}
