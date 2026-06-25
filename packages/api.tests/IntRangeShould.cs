using Xunit;

namespace Api.Tests;

// Convention reference for packages/api.tests — see .claude/skills/app-write-tests.
//
// There is no domain type to unit-test yet: the scheduler boundary (IReviewScheduler,
// SM-2) in docs/ARCHITECTURE.md §3 is not built. This exercises a trivial, self-contained
// value type purely to demonstrate the house test style — naming, Arrange/Act/Assert,
// triangulation, asserting on observable behavior. Replace it with the first real
// behavior test (e.g. Sm2SchedulerShould) once the domain lands; do NOT grow it.
public class IntRangeShould
{
    [Fact]
    public void Contains_ValueInsideRange_ReturnsTrue()
    {
        // Arrange
        var range = new IntRange(1, 10);

        // Act
        var contains = range.Contains(5);

        // Assert
        Assert.True(contains);
    }

    [Fact]
    public void Contains_ValueOutsideRange_ReturnsFalse()
    {
        // Arrange
        var range = new IntRange(1, 10);

        // Act
        var contains = range.Contains(11);

        // Assert
        Assert.False(contains);
    }

    [Fact]
    public void Clamp_ValueAboveMax_ReturnsMax()
    {
        // Arrange
        var range = new IntRange(1, 10);

        // Act
        var clamped = range.Clamp(42);

        // Assert
        Assert.Equal(10, clamped);
    }

    [Fact]
    public void Clamp_ValueBelowMin_ReturnsMin()
    {
        // Arrange
        var range = new IntRange(1, 10);

        // Act
        var clamped = range.Clamp(-5);

        // Assert
        Assert.Equal(1, clamped);
    }
}

// Stand-in subject under test, kept in the test file precisely because it is not domain
// code. When a real type exists, delete this and test that instead.
internal readonly record struct IntRange(int Min, int Max)
{
    public bool Contains(int value) => value >= Min && value <= Max;

    public int Clamp(int value) => value < Min ? Min : value > Max ? Max : value;
}
