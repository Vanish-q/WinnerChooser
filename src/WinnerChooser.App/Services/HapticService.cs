namespace WinnerChooser.App.Services;

/// <summary>
/// Тактильный отклик на победу. Намеренно без разрешения VIBRATE:
/// используется системный HapticFeedback, у которого разрешений нет.
/// </summary>
public static partial class HapticService
{
    /// <summary>Короткий отклик в момент выбора победителя.</summary>
    public static void Winner() => PerformWinnerHaptic();

    static partial void PerformWinnerHaptic();
}
