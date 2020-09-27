using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using ProtoBuf;

internal class ExposureKeyStatsGenerator
{
    private static readonly HttpClient client = new HttpClient();
    private const string ENDPOINT = @"https://distribution-te-prod.prod.svc-test-trace.nhs.uk/distribution/daily/{0}00.zip";
    public static async Task<int> GetNumberOfExposuresForDate(DateTime date)
    {
        // TODO: Error handling
        using (var stream = await client.GetStreamAsync(string.Format(ENDPOINT, date.ToString("yyyyMMdd"))))
        using (var unzipStream = new ZipArchive(stream))
        {
            var export = unzipStream.GetEntry("export.bin");
            using (var exportStream = export.Open())
            using (var tempStream = new MemoryStream()) // TODO: Avoid copying
            {
                exportStream.CopyTo(tempStream);
                tempStream.Seek(16, SeekOrigin.Begin); // Skip header
                var exportData = Serializer.Deserialize<TemporaryExposureKeyExport>(tempStream);
                return exportData.Keys.Count;
            }
        }
    }
}