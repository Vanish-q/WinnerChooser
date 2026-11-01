using Microsoft.Maui.Storage;

namespace WinnerChooser.App.Services;

/// <summary>
/// Пользовательские настройки. Хранятся в Preferences и переживают перезапуск.
/// </summary>
public static class AppSettings
{
    public const double DefaultCountdownSeconds = 3;
    public const double DefaultCircleRadius = 64;

    public const double MinCountdownSeconds = 1;
    public const double MaxCountdownSeconds = 10;
    public const double MinCircleRadius = 32;
    public const double MaxCircleRadius = 96;

    private const string CountdownKey = "countdown_seconds";
    private const string RadiusKey = "circle_radius";

    /// <summary>Пауза после последнего касания, секунды.</summary>
    public static double CountdownSeconds
    {
        get => Math.Clamp(
            Preferences.Default.Get(CountdownKey, DefaultCountdownSeconds),
            MinCountdownSeconds,
            MaxCountdownSeconds);
        set => Preferences.Default.Set(
            CountdownKey,
            Math.Clamp(value, MinCountdownSeconds, MaxCountdownSeconds));
    }

    /// <summary>Радиус круга участника в DIP.</summary>
    public static double CircleRadius
    {
        get => Math.Clamp(
            Preferences.Default.Get(RadiusKey, DefaultCircleRadius),
            MinCircleRadius,
            MaxCircleRadius);
        set => Preferences.Default.Set(
            RadiusKey,
            Math.Clamp(value, MinCircleRadius, MaxCircleRadius));
    }
}
