using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using ProtoBuf;

internal class ExposureKeyStatsGenerator
{
    private static readonly HttpClient client = new HttpClient
    {
        // Cloudfront supports HTTP2
        DefaultRequestVersion = new Version(2, 0)
    };

    private const string ENDPOINT = @"https://distribution-te-prod.prod.svc-test-trace.nhs.uk/distribution/daily/{0}00.zip";
    public static async Task<ExposureStat> GetNumberOfExposuresForDate(DateOnly date)
    {
        var url = string.Format(ENDPOINT, date.ToString("yyyyMMdd"));

        try
        {
            var count = await GetKeyCountForFile(url);
            return new ExposureStat { Date = date, Count = count };
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error whilst retrieving file for {date.ToShortDateString()}: {e}");
            return new ExposureStat{ Date = date, Count = 0 };
        }
    }

    private static async Task<int> GetKeyCountForFile(string url)
    {
        using (var response = await client.GetAsync(url))
        using (var stream = await response.Content.ReadAsStreamAsync())
        using (var unzipStream = new ZipArchive(stream))
        {
            var exportFile = unzipStream.GetEntry("export.bin");
            using (var exportStream = exportFile.Open())
            using (var decodeStream = new MemoryStream())
            {
                await exportStream.CopyToAsync(decodeStream);
                decodeStream.Seek(16, SeekOrigin.Begin); // Skip header
                var exportData = Serializer.Deserialize<TemporaryExposureKeyExport>(decodeStream);
                return exportData.Keys.Count;
            }
        }
    }
}