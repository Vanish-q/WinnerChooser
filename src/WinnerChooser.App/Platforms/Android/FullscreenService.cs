using AndroidX.Core.View;

namespace WinnerChooser.App.Platforms;

public static partial class FullscreenService
{
    static partial void ApplyImmersive() => Update(hide: true);

    static partial void RestoreSystemBars() => Update(hide: false);

    private static void Update(bool hide)
    {
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        var window = activity?.Window;
        if (window is null)
        {
            return;
        }

        var controller = WindowCompat.GetInsetsController(window, window.DecorView);
        if (controller is null)
        {
            return;
        }

        if (hide)
        {
            controller.Hide(WindowInsetsCompat.Type.SystemBars());
            controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
        }
        else
        {
            controller.Show(WindowInsetsCompat.Type.SystemBars());
            controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorDefault;
        }
    }
}
