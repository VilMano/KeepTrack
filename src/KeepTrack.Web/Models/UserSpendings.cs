namespace KeepTrack.Models;

public class UserSpendings
{
    public double Spendings { get; set; }
    public string UserName { get; set; }
}

public class Spendings
{
    public List<UserSpendings> UserSpendings { get; set; } = new List<UserSpendings>();
    public double Debt { get; set; }
    public string Payer { get; set; }
}