using System.Net;

namespace KeepTrack.Services;

public class SheetsService(IConfiguration config)
{
    private readonly IConfiguration _config = config;
    
    public void AddSheet(string url, string date)
    {
        string filePath = _config.GetValue<string>("Locations:Sheet");    
        Console.Write("File: ", filePath);
        if (File.Exists(filePath))
        {
            File.AppendAllText(filePath,
                $"{Environment.NewLine}{url},{date}");
        }
    }

    public async Task DownloadSheetsFromGoogle()
    {
        string sheetPath = _config.GetValue<string>("Locations:Sheet");
        string filesPath = _config.GetValue<string>("Locations:Files");
        var sheets = FileReader.ReadSheetsFile(sheetPath);

        using (var client = new HttpClient())
        {
            foreach (var sheet in sheets)
            {
                var finalPath = filesPath + $"/{sheet.Date}.csv";
                if (File.Exists(finalPath))
                {
                    File.Delete(finalPath);
                }

                string[] sheetArr = sheet.Url.Split("edit?");
                string exportUrl = sheetArr[0] + "export?exportFormat=csv&" + sheetArr[1];
                var respose = await client.GetAsync(exportUrl);
                var file = await respose.Content.ReadAsByteArrayAsync();

                using (var stream = new FileStream(finalPath, FileMode.Create))
                {
                    stream.Write(file);
                }
            }
        }
    }
}