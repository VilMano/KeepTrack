using KeepTrack.Contracts.Services;
using KeepTrack.Models;

namespace KeepTrack.Services;

public class MovementService(IConfiguration config) : IMovementService
{
    private string[] Months =
    {
        "", "January", "February", "March", "April", "May",
        "June", "July", "August", "September", "October", "November", "December"
    };

    public Total GetTotal(int year, int month)
    {
        Total total = new Total();
        string monthNorm = month < 10 ? $"0{month}" : month.ToString();
        string yearMonth = $"{year}-{monthNorm}";
        
        total.Year = year;
        total.Month = Months[month];

        string filesPath = config.GetValue<string>("Locations:Files");
        string filePath = $"{filesPath}/{yearMonth}.csv";
        if (!File.Exists(filePath))
        {
            return total;
        }
        
        List<MovementCsv> movementsCsv = FileReader.ReadMovementsFile(filePath);

        if (movementsCsv.Count > 0)
        {
            double totalCost = movementsCsv.Sum(m => m.Value);

            total.Value = Math.Round(totalCost, 2);
        }


        return total;
    }

    public List<Movement> GetMovements(int month, int year)
    {
        List<Movement> movements = new List<Movement>();

        string monthNorm = month < 10 ? $"0{month}" : month.ToString();
        string yearMonth = $"{year}-{monthNorm}";
        string filesPath = config.GetValue<string>("Locations:Files");
        string filePath = $"{filesPath}/{yearMonth}.csv";

        if (!File.Exists(filePath))
        {
            return movements;
        }

        List<MovementCsv> movementsCsv = FileReader.ReadMovementsFile(filePath);

        string user1Colour = RandomColour();
        string user2Colour = RandomColour();

        var users = movementsCsv.GroupBy(m => m.Spender).Select(m => m.Key).ToList();

        foreach (MovementCsv movement in movementsCsv)
        {
            Movement newMovement = new Movement();
            newMovement.Spender = movement.Spender;
            newMovement.Description = movement.Description;
            newMovement.Value = Math.Round(movement.Value, 2);

            newMovement.Date = DateTime.Parse($"{yearMonth}-{movement.Day}");
            newMovement.Colour = users.IndexOf(movement.Spender) == 0 ? user1Colour : user2Colour;
            movements.Add(newMovement);
        }

        return movements;
    }
    
    public List<Movement> GetAllMovements()
    {
        List<Movement> movements = new List<Movement>();

        int year = DateTime.Now.Year;
        List<string> filePaths = FileReader.GetAllFileNames(config.GetValue<string>("Locations:Files"));

        filePaths = filePaths.Where(p => 
                p.Contains(year.ToString()) || 
                p.Contains((year-1).ToString()))
            .ToList();
        
        foreach (string filePath in filePaths)
        {
            year = int.Parse(filePath.Substring(filePath.LastIndexOf("/")+1, 4));
            string month = filePath.Substring(filePath.LastIndexOf("-")+1, 2);
            
            string monthNorm = int.Parse(month) < 10 ? $"{month}" : month.ToString();
            
            string filesPath = config.GetValue<string>("Locations:Files");

            if (!File.Exists(filePath))
            {
                return movements;
            }

            List<MovementCsv> movementsCsv = FileReader.ReadMovementsFile(filePath);

            string user1Colour = RandomColour();
            string user2Colour = RandomColour();

            var users = movementsCsv.GroupBy(m => m.Spender).Select(m => m.Key).ToList();

            foreach (MovementCsv movement in movementsCsv)
            {
                Movement newMovement = new Movement();
                newMovement.Spender = movement.Spender;
                newMovement.Description = movement.Description;
                newMovement.Value = Math.Round(movement.Value, 2);

                newMovement.Date = DateTime.Parse($"{year}-{month}-{movement.Day}");
                newMovement.Colour = users.IndexOf(movement.Spender) == 0 ? user1Colour : user2Colour;
                movements.Add(newMovement);
            }
        }

        movements = movements.OrderBy(m => m.Date).ToList();
        
        return movements;
    }

    public Spendings GetUsersSpendings(int year, int month)
    {
        Spendings spendings = new Spendings();
        
        string monthNorm = month < 10 ? $"0{month}" : month.ToString();
        string yearMonth = $"{year}-{monthNorm}";
        
        string filesPath = config.GetValue<string>("Locations:Files");
        string filePath = $"{filesPath}/{yearMonth}.csv";

        if (!File.Exists(filePath))
        {
            return spendings;
        }
        
        List<MovementCsv> movementsCsv = FileReader.ReadMovementsFile(filePath);

        spendings.Debt = 0;
        List<UserSpendings> usersSpendings = new List<UserSpendings>();

        // group by spender 
        var movements = movementsCsv
            .GroupBy(m => m.Spender)
            .Select(m => m)
            .ToList();

        foreach (var m in movements)
        {
            double total = m.Sum(m => m.Value);
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

    public string RandomColour()
    {
        Random rand = new Random();
        string[] colours = { "lightblue", "lightgreen", "lightyellow" };

        var r = rand.Next(0, 3);

        return colours[r];
    }

    public static string[] GetUserColours()
    {
        Random rand = new Random();
        string[] colours = { "lightblue", "lightgreen", "lightyellow" };

        var r1 = rand.Next(0, 3);
        var r2 = rand.Next(0, 3);

        if (r1 == r2)
            r2 = rand.Next(0, 3);

        return [colours[r1], colours[r2]];
    }
    
}