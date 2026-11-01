using WinnerChooser.App.Services;

namespace WinnerChooser.App.Pages;

public partial class SettingsPage : ContentPage
{
    // true до конца инициализации: не сохраняем значения, которые XAML
    // выставляет при создании контролов раньше, чем загрузятся настройки.
    private bool _updating = true;

    public SettingsPage()
    {
        InitializeComponent();

        CountdownSlider.Value = Math.Round(AppSettings.CountdownSeconds);
        RadiusSlider.Value = Math.Round(AppSettings.CircleRadius);
        _updating = false;

        ApplyLocalization();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        TitleLabel.Text = L10n.T("Settings");
        SemanticProperties.SetDescription(BackButton, L10n.T("Back"));

        CountdownTitleLabel.Text = L10n.T("CountdownTitle");
        CountdownHintLabel.Text = L10n.T("CountdownHint");
        RadiusTitleLabel.Text = L10n.T("CircleSizeTitle");
        RadiusHintLabel.Text = L10n.T("CircleSizeHint");
        LanguageTitleLabel.Text = L10n.T("LanguageTitle");

        SemanticProperties.SetDescription(CountdownSlider, L10n.T("CountdownTitle"));
        SemanticProperties.SetDescription(RadiusSlider, L10n.T("CircleSizeTitle"));

        UpdateCountdownValue();
        UpdateRadiusValue();

        _updating = true;
        LanguagePicker.ItemsSource = new List<string>
        {
            L10n.T("LanguageSystem"),
            L10n.T("LanguageRussian"),
            L10n.T("LanguageEnglish"),
        };
        LanguagePicker.SelectedIndex = (int)L10n.Setting;
        _updating = false;
    }

    private void OnCountdownChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_updating)
        {
            return;
        }

        var value = Math.Round(e.NewValue);
        if (Math.Abs(CountdownSlider.Value - value) > 0.01)
        {
            _updating = true;
            CountdownSlider.Value = value;
            _updating = false;
        }

        AppSettings.CountdownSeconds = value;
        UpdateCountdownValue();
    }

    private void OnRadiusChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_updating)
        {
            return;
        }

        var value = Math.Round(e.NewValue);
        if (Math.Abs(RadiusSlider.Value - value) > 0.01)
        {
            _updating = true;
            RadiusSlider.Value = value;
            _updating = false;
        }

        AppSettings.CircleRadius = value;
        UpdateRadiusValue();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (_updating)
        {
            return;
        }

        var index = LanguagePicker.SelectedIndex;
        if (index < 0)
        {
            return;
        }

        L10n.SetLanguage((AppLanguage)index);

        // Язык применяется сразу, чтобы его можно было проверить на месте.
        ApplyLocalization();
    }

    private void UpdateCountdownValue()
        => CountdownValueLabel.Text =
            $"{Math.Round(AppSettings.CountdownSeconds)} {L10n.T("SecondsShort")}";

    private void UpdateRadiusValue()
        => RadiusValueLabel.Text = $"{Math.Round(AppSettings.CircleRadius)}";

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
