namespace WinnerChooser.Core;

/// <summary>
/// Настройки раунда — единственное место, где меняются тайминги и размеры.
/// </summary>
public sealed class RoundOptions
{
    /// <summary>Сколько ждать после последнего касания перед выбором.</summary>
    public TimeSpan CountdownDuration { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>Сколько после победы игнорировать касания, чтобы палец не начал новый раунд сразу.</summary>
    public TimeSpan RevealLockDuration { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>Минимум пальцев для запуска отсчёта.</summary>
    public int MinParticipants { get; init; } = 2;

    /// <summary>Радиус круга участника в DIP.</summary>
    public double ParticipantRadius { get; init; } = 64;

    /// <summary>Длительность пульсации круга победителя.</summary>
    public TimeSpan WinnerPulseDuration { get; init; } = TimeSpan.FromMilliseconds(600);
}
