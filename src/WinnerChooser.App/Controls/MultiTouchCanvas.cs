namespace WinnerChooser.App.Controls;

/// <summary>Фаза касания, которую отдаёт платформенный обработчик.</summary>
public enum TouchPhase
{
    Down,
    Moved,
    Up,
    Cancelled,
}

/// <summary>Одно касание: идентификатор указателя, позиция в DIP и фаза.</summary>
public sealed class TouchPointEventArgs : EventArgs
{
    public TouchPointEventArgs(long id, Point position, TouchPhase phase)
    {
        Id = id;
        Position = position;
        Phase = phase;
    }

    /// <summary>Идентификатор указателя, стабильный, пока палец на экране.</summary>
    public long Id { get; }

    /// <summary>Позиция в независимых от плотности пикселях (DIP).</summary>
    public Point Position { get; }

    public TouchPhase Phase { get; }
}

/// <summary>
/// Представление, отдающее все одновременные касания. MAUI из коробки умеет
/// только один указатель, поэтому для Android написан собственный ViewHandler.
/// </summary>
public class MultiTouchCanvas : View
{
    /// <summary>Возникает на каждое изменение любого пальца.</summary>
    public event EventHandler<TouchPointEventArgs>? Touch;

    internal void RaiseTouch(long id, Point position, TouchPhase phase)
        => Touch?.Invoke(this, new TouchPointEventArgs(id, position, phase));
}
