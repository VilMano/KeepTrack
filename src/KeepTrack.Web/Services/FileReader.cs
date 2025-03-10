using System.Globalization;
using CsvHelper;
using KeepTrack.Models;

namespace KeepTrack.Services;

public static class FileReader
{
    public static List<MovementCsv> ReadMovementsFile(string filePath)
    {
        using(var stream = new StreamReader(filePath))
        using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<MovementCsv>();
            return records.ToList();
        }

        return null;
    }
    
    public static List<Sheet> ReadSheetsFile(string filePath)
    {
        using(var stream = new StreamReader(filePath))
        using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<Sheet>();
            return records.ToList();
        }

        return null;
    }

    public static List<string> GetAllFileNames(string filePath)
    {
        List<string> filePaths = new List<string>();
        if (Directory.Exists(filePath))
        {
            filePaths = Directory.EnumerateFiles(filePath).ToList();
        }
        
        return filePaths;
    }
}