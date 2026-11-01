namespace WinnerChooser.App.Platforms;

/// <summary>
/// Полноэкранный режим на время раунда: системные бары прячутся, чтобы пальцы
/// не задевали жесты, при выходе возвращаются. Реализация — по платформам.
/// </summary>
public static partial class FullscreenService
{
    /// <summary>Скрыть статус-бар и навигацию, оставив свайп для их показа.</summary>
    public static void EnterImmersive() => ApplyImmersive();

    /// <summary>Вернуть системные бары.</summary>
    public static void ExitImmersive() => RestoreSystemBars();

    static partial void ApplyImmersive();

    static partial void RestoreSystemBars();
}
