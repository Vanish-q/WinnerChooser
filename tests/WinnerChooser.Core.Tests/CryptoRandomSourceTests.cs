using WinnerChooser.Core;

namespace WinnerChooser.Core.Tests;

public sealed class CryptoRandomSourceTests
{
    [Fact]
    public void Next_ReturnsValueInsideRange()
    {
        var random = new CryptoRandomSource();

        for (var i = 0; i < 10_000; i++)
        {
            Assert.InRange(random.Next(3), 0, 2);
        }
    }

    [Fact]
    public void Next_CoversAllValues()
    {
        var random = new CryptoRandomSource();
        var seen = new HashSet<int>();

        for (var i = 0; i < 300 && seen.Count < 3; i++)
        {
            seen.Add(random.Next(3));
        }

        Assert.Equal(3, seen.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Next_ThrowsForNonPositiveRange(int maxExclusive)
    {
        var random = new CryptoRandomSource();

        Assert.Throws<ArgumentOutOfRangeException>(() => random.Next(maxExclusive));
    }

    [Fact]
    public void FirstWinner_IsNotTheSameAcrossFreshSources()
    {
        // Регрессия: при холодном старте первый выбор не должен повторяться
        // из-за одинакового сида, поэтому каждый источник создаётся заново.
        var winners = new HashSet<int>();

        for (var i = 0; i < 30; i++)
        {
            var clock = new TestClock();
            var controller = new RoundController(clock, new CryptoRandomSource());
            controller.PointerDown(1, new PointF(0, 0));
            controller.PointerDown(2, new PointF(0, 0));
            controller.PointerDown(3, new PointF(0, 0));

            clock.Advance(TimeSpan.FromSeconds(3));
            controller.Tick();

            winners.Add(controller.Winner!.Number);
        }

        Assert.True(
            winners.Count > 1,
            "Первый выбор одинаков для всех свежих источников — генератор не случаен.");
    }
}
