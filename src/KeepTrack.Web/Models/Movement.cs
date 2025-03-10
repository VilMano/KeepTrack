using KeepTrack.Data;

namespace KeepTrack.Models;

public class Movement
{   
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public double Value  { get; set; }
    public string Spender { get; set; }

    public string Colour { get; set; } = "white";

}