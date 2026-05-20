using FluentAssertions;

namespace Tests.Common;

public abstract class TestBase
{
    protected static Guid NewId() => Guid.NewGuid();

    protected static DateTime FutureDate()
        => DateTime.UtcNow.AddDays(7);

    protected static void AssertDatesClose(
        DateTime actual,
        DateTime expected,
        int secondsTolerance = 5)
    {
        actual.Should().BeCloseTo(
            expected,
            TimeSpan.FromSeconds(secondsTolerance));
    }
}