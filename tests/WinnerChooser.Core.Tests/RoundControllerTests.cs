using WinnerChooser.Core;

namespace WinnerChooser.Core.Tests;

public sealed class RoundControllerTests
{
    private static readonly TimeSpan Countdown = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan RevealLock = TimeSpan.FromSeconds(1);

    private readonly TestClock _clock = new();
    private readonly ScriptedRandom _random = new(0);

    private RoundController CreateController(IRandomSource? random = null)
        => new(
            _clock,
            random ?? _random,
            new RoundOptions
            {
                CountdownDuration = Countdown,
                RevealLockDuration = RevealLock,
                MinParticipants = 2,
            });

    private static PointF P(float x, float y) => new(x, y);

    [Fact]
    public void OneFinger_DoesNotStartCountdown()
    {
        var controller = CreateController();

        controller.PointerDown(1, P(10, 10));

        Assert.Equal(RoundState.Waiting, controller.State);
        Assert.Equal(TimeSpan.Zero, controller.Remaining);

        _clock.Advance(TimeSpan.FromSeconds(10));
        controller.Tick();

        Assert.Equal(RoundState.Waiting, controller.State);
        Assert.Null(controller.Winner);
    }

    [Fact]
    public void TwoFingers_StartCountdown()
    {
        var controller = CreateController();

        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));

        Assert.Equal(RoundState.Countdown, controller.State);
        Assert.Equal(Countdown, controller.Remaining);
    }

    [Fact]
    public void TickBeforeDeadline_DoesNothing()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));

        _clock.AdvanceMilliseconds(2999);
        controller.Tick();

        Assert.Equal(RoundState.Countdown, controller.State);
        Assert.Null(controller.Winner);
    }

    [Fact]
    public void TickExactlyAtDeadline_RevealsWinner()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));

        _clock.Advance(Countdown);
        controller.Tick();

        Assert.Equal(RoundState.WinnerRevealed, controller.State);
        Assert.NotNull(controller.Winner);
    }

    [Fact]
    public void NewTouch_RestartsCountdown()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));

        _clock.Advance(TimeSpan.FromSeconds(2));
        controller.PointerDown(3, P(200, 200));

        Assert.Equal(Countdown, controller.Remaining);

        _clock.Advance(TimeSpan.FromSeconds(2));
        controller.Tick();
        Assert.Equal(RoundState.Countdown, controller.State);

        _clock.Advance(TimeSpan.FromSeconds(1));
        controller.Tick();
        Assert.Equal(RoundState.WinnerRevealed, controller.State);
    }

    [Fact]
    public void RemovingFingerWhileTwoRemain_RestartsCountdown()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));
        controller.PointerDown(3, P(200, 200));

        _clock.Advance(TimeSpan.FromSeconds(2));
        controller.PointerUp(1);

        Assert.Equal(RoundState.Countdown, controller.State);
        Assert.Equal(Countdown, controller.Remaining);
        Assert.Equal(2, controller.Participants.Count);
    }

    [Fact]
    public void MovingFinger_DoesNotRestartCountdown()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));

        _clock.Advance(TimeSpan.FromSeconds(2));
        controller.PointerMoved(1, P(50, 50));

        Assert.Equal(TimeSpan.FromSeconds(1), controller.Remaining);

        _clock.Advance(TimeSpan.FromSeconds(1));
        controller.Tick();
        Assert.Equal(RoundState.WinnerRevealed, controller.State);
    }

    [Fact]
    public void DroppingBelowTwo_ReturnsToWaitingAndClearsDeadline()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));

        _clock.Advance(TimeSpan.FromSeconds(2));
        controller.PointerUp(2);

        Assert.Equal(RoundState.Waiting, controller.State);
        Assert.Equal(TimeSpan.Zero, controller.Remaining);

        _clock.Advance(TimeSpan.FromSeconds(10));
        controller.Tick();

        Assert.Equal(RoundState.Waiting, controller.State);
        Assert.Null(controller.Winner);
    }

    [Fact]
    public void RemovingAllFingers_LeavesEmptyRoundWithoutWinner()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));

        controller.PointerUp(1);
        controller.PointerUp(2);

        Assert.Equal(RoundState.Waiting, controller.State);
        Assert.Empty(controller.Participants);
        Assert.Null(controller.Winner);
    }

    [Fact]
    public void Winner_IsChosenOnlyFromFingersStillDown()
    {
        var controller = CreateController(new ScriptedRandom(0));
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));
        controller.PointerDown(3, P(200, 200));

        controller.PointerUp(1);
        _clock.Advance(Countdown);
        controller.Tick();

        Assert.NotNull(controller.Winner);
        Assert.NotEqual(1, controller.Winner!.Id);
    }

    [Fact]
    public void AfterReveal_PointerMovesAndUpsAreIgnored()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));
        _clock.Advance(Countdown);
        controller.Tick();

        var winnerPosition = controller.Winner!.Position;
        controller.PointerMoved(controller.Winner.Id, P(500, 500));
        controller.PointerUp(controller.Winner.Id);

        Assert.Equal(2, controller.Participants.Count);
        Assert.Equal(winnerPosition, controller.Winner.Position);
    }

    [Fact]
    public void PointerDownDuringRevealLock_IsIgnored()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));
        _clock.Advance(Countdown);
        controller.Tick();

        _clock.AdvanceMilliseconds(500);
        controller.PointerDown(9, P(300, 300));

        Assert.Equal(RoundState.WinnerRevealed, controller.State);
        Assert.Equal(2, controller.Participants.Count);
        Assert.NotNull(controller.Winner);
    }

    [Fact]
    public void PointerDownAfterRevealLock_StartsNewRound()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));
        _clock.Advance(Countdown);
        controller.Tick();

        _clock.Advance(RevealLock);
        controller.PointerDown(9, P(300, 300));

        Assert.Equal(RoundState.Waiting, controller.State);
        Assert.Null(controller.Winner);
        var participant = Assert.Single(controller.Participants);
        Assert.Equal(9, participant.Id);
        Assert.Equal(1, participant.Number);
    }

    [Fact]
    public void Reset_ClearsEverything()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));

        controller.Reset();

        Assert.Equal(RoundState.Waiting, controller.State);
        Assert.Empty(controller.Participants);
        Assert.Null(controller.Winner);
        Assert.Equal(TimeSpan.Zero, controller.Remaining);
    }

    [Fact]
    public void Reset_AfterReveal_AllowsNewRound()
    {
        var controller = CreateController();
        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));
        _clock.Advance(Countdown);
        controller.Tick();

        controller.Reset();
        controller.PointerDown(5, P(50, 50));
        controller.PointerDown(6, P(60, 60));

        Assert.Equal(RoundState.Countdown, controller.State);
        Assert.Equal(2, controller.Participants.Count);
    }

    [Fact]
    public void ElevenFingers_ReusePaletteColorsCyclically()
    {
        var controller = CreateController();

        for (var i = 1; i <= 11; i++)
        {
            controller.PointerDown(i, P(i * 10, i * 10));
        }

        Assert.Equal(11, controller.Participants.Count);
        Assert.Equal(1, controller.Participants[0].Number);
        Assert.Equal(11, controller.Participants[10].Number);
        Assert.Equal(controller.Participants[0].Color, controller.Participants[10].Color);
        Assert.Equal(1, controller.Participants[10].ColorIndex + 1);
    }

    [Fact]
    public void Palette_HasTenDistinctColors()
    {
        Assert.Equal(10, TouchPalette.Colors.Count);
        Assert.Equal(TouchPalette.Colors.Count, TouchPalette.Colors.Distinct().Count());
    }

    [Fact]
    public void Distribution_IsRoughlyUniform()
    {
        const int runs = 10_000;
        var results = new Dictionary<int, int> { [1] = 0, [2] = 0, [3] = 0 };
        var controller = new RoundController(
            _clock,
            new SystemRandomSource(new Random(20261101)),
            new RoundOptions { CountdownDuration = Countdown });

        for (var i = 0; i < runs; i++)
        {
            controller.PointerDown(1, P(10, 10));
            controller.PointerDown(2, P(100, 100));
            controller.PointerDown(3, P(200, 200));

            _clock.Advance(Countdown);
            controller.Tick();

            results[controller.Winner!.Number]++;
            controller.Reset();
        }

        var expected = runs / 3.0;
        var tolerance = expected * 0.05;
        foreach (var count in results.Values)
        {
            Assert.InRange(count, expected - tolerance, expected + tolerance);
        }
    }

    [Fact]
    public void Changed_IsRaisedOnStateTransitions()
    {
        var controller = CreateController();
        var events = 0;
        controller.Changed += (_, _) => events++;

        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));
        _clock.Advance(Countdown);
        controller.Tick();

        Assert.True(events >= 3);
    }

    [Fact]
    public void WinnerSelected_IsRaisedOnce()
    {
        var controller = CreateController();
        var events = 0;
        controller.WinnerSelected += (_, _) => events++;

        controller.PointerDown(1, P(10, 10));
        controller.PointerDown(2, P(100, 100));
        _clock.Advance(Countdown);
        controller.Tick();
        controller.Tick();

        Assert.Equal(1, events);
    }
}
