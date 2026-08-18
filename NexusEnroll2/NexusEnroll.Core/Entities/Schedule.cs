namespace NexusEnroll.Core.Entities;

public class Schedule
{
    public DayOfWeek Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Room { get; set; } = string.Empty;
    public string TimeSlot { get; set; } = string.Empty;

    public Schedule()
    {
    }

    public Schedule(DayOfWeek day, TimeOnly startTime, TimeOnly endTime, string room)
    {
        Day = day;
        StartTime = startTime;
        EndTime = endTime;
        Room = room;
    }
}