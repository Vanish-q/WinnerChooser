using Android.Views;

namespace WinnerChooser.App.Services;

public static partial class HapticService
{
    static partial void PerformWinnerHaptic()
    {
        // View.PerformHapticFeedback не требует android.permission.VIBRATE
        // и уважает системную настройку тактильного отклика.
        var decorView = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView;
        decorView?.PerformHapticFeedback(FeedbackConstants.LongPress);
    }
}
