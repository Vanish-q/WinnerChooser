using WinnerChooser.App.Pages;
using WinnerChooser.App.Services;

namespace WinnerChooser.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        L10n.Initialize();
        UserAppTheme = AppTheme.Dark;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(new NavigationPage(new MainPage()));
}
