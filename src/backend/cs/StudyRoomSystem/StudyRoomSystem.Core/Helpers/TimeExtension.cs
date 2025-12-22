using System.Collections;

namespace StudyRoomSystem.Core.Helpers;

public static class TimeExtension
{
    public static TimeOnly ToTimeOnly(this TimeSpan source)
    {
        return new TimeOnly(source.Ticks);
    }

    public static IEnumerable<KeyValuePair<DateTime, DateTime>> ToOpenTimes
    (
        this IEnumerable<KeyValuePair<DateTime, DateTime>> closeTimes,
        DateTime start,
        DateTime end)     
    {
        foreach (var (s, e) in closeTimes)
        {
            if (start < s)
            {
                yield return new(start, s);
            }
            start = e;
        }
        
        if (start < end)
        {
            yield return new(start, end);
        }
    }
}