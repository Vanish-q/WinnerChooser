namespace WinnerChooser.App.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnWinnerPickerClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new WinnerPickerPage());
    }
}
