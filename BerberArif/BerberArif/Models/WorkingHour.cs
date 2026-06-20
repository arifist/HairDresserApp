namespace BerberArif.Models;

public class WorkingHour
{
    public int Id { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public bool IsOpen { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }
}
