namespace WinnerChooser.Core;

/// <summary>Монотонные часы. Никогда не используем DateTime.Now — он может прыгнуть.</summary>
public interface IClock
{
    TimeSpan Now { get; }
}

/// <summary>Часы на Stopwatch: не зависят от перевода времени и часового пояса.</summary>
public sealed class StopwatchClock : IClock
{
    private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();

    public TimeSpan Now => _stopwatch.Elapsed;
}
