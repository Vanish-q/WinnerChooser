using WinnerChooser.App.Services;

namespace WinnerChooser.App.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        ApplyLocalization();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Язык мог измениться на странице настроек.
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        TitleLabel.Text = L10n.T("AppTitle");
        ModesLabel.Text = L10n.T("Modes");
        ModeDrawButton.Text = L10n.T("ModeDraw");

        SemanticProperties.SetDescription(ModeDrawButton, L10n.T("ModeDrawDescription"));
        SemanticProperties.SetDescription(SettingsButton, L10n.T("Settings"));
    }

    private async void OnWinnerPickerClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new WinnerPickerPage());
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}
