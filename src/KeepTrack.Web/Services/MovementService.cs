using KeepTrack.Contracts.Services;
using KeepTrack.Models;

namespace KeepTrack.Services;

public class MovementService(IConfiguration config) : IMovementService
{
    private string[] Months =
    [
        "", "January", "February", "March", "April", "May",
        "June", "July", "August", "September", "October", "November", "December"
    ];

    public Total GetTotal(int year, int month)
    {
        var total = new Total();
        var monthNorm = month < 10 ? $"0{month}" : month.ToString();
        var yearMonth = $"{year}-{monthNorm}";
        
        total.Year = year;
        total.Month = Months[month];

        var filesPath = config.GetValue<string>("Locations:Files");
        var filePath = $"{filesPath}/{yearMonth}.csv";
        if (!File.Exists(filePath))
        {
            return total;
        }
        
        var movementsCsv = FileReader.ReadMovementsFile(filePath);

        if (movementsCsv.Count <= 0) return total;
        
        var totalCost = movementsCsv.Sum(m => m.Value);

        total.Value = Math.Round(totalCost, 2);


        return total;
    }

    public List<Movement> GetMovements(int month, int year)
    {
        var movements = new List<Movement>();

        var monthNorm = month < 10 ? $"0{month}" : month.ToString();
        var yearMonth = $"{year}-{monthNorm}";
        var filesPath = config.GetValue<string>("Locations:Files");
        var filePath = $"{filesPath}/{yearMonth}.csv";

        if (!File.Exists(filePath))
        {
            return movements;
        }

        var movementsCsv = FileReader.ReadMovementsFile(filePath);

        var user1Colour = RandomColour();
        var user2Colour = RandomColour();

        var users = movementsCsv.GroupBy(m => m.Spender).Select(m => m.Key).ToList();

        movements.AddRange(movementsCsv.Select(movement => new Movement
        {
            Spender = movement.Spender,
            Description = movement.Description,
            Value = Math.Round(movement.Value, 2),
            Date = DateTime.Parse($"{yearMonth}-{movement.Day}"),
            Colour = users.IndexOf(movement.Spender) == 0 ? user1Colour : user2Colour
        }));

        return movements;
    }

    public List<Movement> GetMovementsByUser(int year, int month, string user)
    {
        throw new NotImplementedException();
    }

    public double GetYearAverage(int year)
    {
        var average = 0.0;
        
        var filesPath = config.GetValue<string>("Locations:Files");
        var files = Directory.GetFiles(filesPath).Where(f => f.Contains(year.ToString()));

        foreach (var file in files)
        {
            var movementsCsv = FileReader.ReadMovementsFile(file);

            average += movementsCsv.Sum(m => m.Value);
        }
        
        return Math.Round(average/files.Count(), 2);
    }

    public List<Movement> GetAllMovements()
    {
        List<Movement> movements = [];

        var year = DateTime.Now.Year;
        var filePaths = FileReader.GetAllFileNames(config.GetValue<string>("Locations:Files"));

        filePaths = filePaths.Where(p => 
                p.Contains(year.ToString()) || 
                p.Contains((year-1).ToString()))
            .ToList();
        
        foreach (var filePath in filePaths)
        {
            year = int.Parse(filePath.Substring(filePath.LastIndexOf("/")+1, 4));
            var month = filePath.Substring(filePath.LastIndexOf("-")+1, 2);

            if (!File.Exists(filePath))
            {
                return movements;
            }

            var movementsCsv = FileReader.ReadMovementsFile(filePath);

            var user1Colour = RandomColour();
            var user2Colour = RandomColour();

            var users = movementsCsv.GroupBy(m => m.Spender).Select(m => m.Key).ToList();

            movements.AddRange(movementsCsv.Select(movement => new Movement
            {
                Spender = movement.Spender,
                Description = movement.Description,
                Value = Math.Round(movement.Value, 2),
                Date = DateTime.Parse($"{year}-{month}-{movement.Day}"),
                Colour = users.IndexOf(movement.Spender) == 0 ? user1Colour : user2Colour
            }));
        }

        movements = movements.OrderBy(m => m.Date).ToList();
        
        return movements;
    }

    public Spendings GetUsersSpendings(int year, int month)
    {
        var spendings = new Spendings();
        
        var monthNorm = month < 10 ? $"0{month}" : month.ToString();
        var yearMonth = $"{year}-{monthNorm}";
        
        var filesPath = config.GetValue<string>("Locations:Files");
        var filePath = $"{filesPath}/{yearMonth}.csv";

        if (!File.Exists(filePath))
        {
            return spendings;
        }
        
        var movementsCsv = FileReader.ReadMovementsFile(filePath);

        spendings.Debt = 0;
        var usersSpendings = new List<UserSpendings>();

        // group by spender 
        var movements = movementsCsv
            .GroupBy(m => m.Spender)
            .Select(m => m)
            .ToList();

        foreach (var m in movements)
        {
            var total = m.Sum(m => m.Value);
            spendings.Debt = Math.Round(Math.Abs((total /2) - spendings.Debt), 2);

            usersSpendings.Add(new UserSpendings
            {
                UserName = m.Key,
                Spendings = total
            });
        }

        spendings.Payer = usersSpendings[usersSpendings[0].Spendings > usersSpendings[1].Spendings ? 1 : 0].UserName;
        spendings.UserSpendings = usersSpendings;

        return spendings;
    }

    private static string RandomColour()
    {
        var rand = new Random();
        string[] colours = ["lightblue", "lightgreen"];

        var r = rand.Next(0, 2);

        return colours[r];
    }

    public static string[] GetUserColours()
    {
        var rand = new Random();
        string[] colours = ["lightblue", "lightgreen"];

        var r1 = rand.Next(0, 2);
        var r2 = rand.Next(0, 2);

        if (r1 == r2)
            r2 = rand.Next(0, 2);

        return [colours[r1], colours[r2]];
    }
    
}