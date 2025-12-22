using System.Diagnostics;
using StudyRoomSystem.Core.Helpers;
using Xunit.Abstractions;

namespace StudyRoomSystem.Core.Test;

public class TimeExtensionTest(ITestOutputHelper output)
{
    [Fact]
    public void ToOpenTimesTest()
    {
        var start = new DateTime(2025, 12, 13, 8, 0, 0);
        var end = new DateTime(2025, 12, 13, 20, 0, 0);
        var closeTimes = new[]
        {
            new KeyValuePair<DateTime, DateTime>(
                new DateTime(2025, 12, 13, 10, 0, 0),
                new DateTime(2025, 12, 13, 12, 0, 0)
            ),
            new KeyValuePair<DateTime, DateTime>(
                new DateTime(2025, 12, 13, 14, 0, 0),
                new DateTime(2025, 12, 13, 16, 0, 0)
            ),
            new KeyValuePair<DateTime, DateTime>(
                new DateTime(2025, 12, 13, 18, 0, 0),
                new DateTime(2025, 12, 13, 20, 0, 0)
            )
        };

        var openTimes = closeTimes.ToOpenTimes(start, end).ToArray();

        output.WriteLine(string.Join(',', openTimes));
        Assert.Equal(
            [
                new(new DateTime(2025, 12, 13, 8, 0, 0), new DateTime(2025, 12, 13, 10, 0, 0)),
                new(new DateTime(2025, 12, 13, 12, 0, 0), new DateTime(2025, 12, 13, 14, 0, 0)),
                new(new DateTime(2025, 12, 13, 16, 0, 0), new DateTime(2025, 12, 13, 18, 0, 0)),
            ],
            openTimes
        );
    }
}