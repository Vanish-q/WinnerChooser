using WinnerChooser.App.Controls;
using WinnerChooser.App.Drawing;
using WinnerChooser.App.Platforms;
using WinnerChooser.App.Services;
using WinnerChooser.Core;

namespace WinnerChooser.App.Pages;

public partial class WinnerPickerPage : ContentPage
{
    private readonly StopwatchClock _clock = new();
    private readonly RoundController _controller;
    private readonly BoardDrawable _board = new();
    private readonly IDispatcherTimer _timer;

    private TimeSpan _revealedAt;

    public WinnerPickerPage()
    {
        InitializeComponent();

        _controller = new RoundController(_clock, new SystemRandomSource());
        _controller.Changed += OnControllerChanged;
        _controller.WinnerSelected += OnWinnerSelected;

        Board.Drawable = _board;
        Canvas.Touch += OnCanvasTouch;

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += OnFrame;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        FullscreenService.EnterImmersive();

        if (Window is not null)
        {
            Window.Deactivated += OnWindowDeactivated;
            Window.Stopped += OnWindowDeactivated;
        }

        Refresh();
    }

    protected override void OnDisappearing()
    {
        if (Window is not null)
        {
            Window.Deactivated -= OnWindowDeactivated;
            Window.Stopped -= OnWindowDeactivated;
        }

        _timer.Stop();
        _controller.Reset();
        FullscreenService.ExitImmersive();

        base.OnDisappearing();
    }

    private void OnCanvasTouch(object? sender, TouchPointEventArgs e)
    {
        var position = new WinnerChooser.Core.PointF((float)e.Position.X, (float)e.Position.Y);

        switch (e.Phase)
        {
            case TouchPhase.Down:
                _controller.PointerDown(e.Id, position);
                break;

            case TouchPhase.Moved:
                _controller.PointerMoved(e.Id, position);
                break;

            case TouchPhase.Up:
            case TouchPhase.Cancelled:
                // Отменённый системой жест считаем снятием пальца.
                _controller.PointerUp(e.Id);
                break;
        }
    }

    private void OnControllerChanged(object? sender, EventArgs e) => Refresh();

    private void OnWinnerSelected(object? sender, Participant winner)
    {
        _revealedAt = _clock.Now;
        Refresh();
        HapticService.Winner();
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        _controller.Tick();
        Refresh();
    }

    private void Refresh()
    {
        var state = _controller.State;
        var pulse = 0d;

        if (state == RoundState.WinnerRevealed)
        {
            var elapsed = _clock.Now - _revealedAt;
            var duration = _controller.Options.WinnerPulseDuration;
            pulse = duration <= TimeSpan.Zero ? 1d : Math.Clamp(elapsed / duration, 0d, 1d);
        }

        _board.Update(
            _controller.Participants,
            state,
            _controller.Winner?.Id ?? -1,
            _controller.CountdownProgress,
            pulse,
            _controller.Options.ParticipantRadius);

        Board.Invalidate();

        switch (state)
        {
            case RoundState.Waiting:
                HintLabel.IsVisible = true;
                HintLabel.Text = _controller.Participants.Count == 0
                    ? "Положите пальцы на экран"
                    : "Нужен ещё один палец";
                CountdownLabel.IsVisible = false;
                _timer.Stop();
                break;

            case RoundState.Countdown:
                HintLabel.IsVisible = false;
                CountdownLabel.IsVisible = true;
                CountdownLabel.Text = Math.Max(1, (int)Math.Ceiling(_controller.Remaining.TotalSeconds)).ToString();
                if (!_timer.IsRunning)
                {
                    _timer.Start();
                }

                break;

            case RoundState.WinnerRevealed:
                HintLabel.IsVisible = false;
                CountdownLabel.IsVisible = false;

                if (pulse >= 1d)
                {
                    _timer.Stop();
                }
                else if (!_timer.IsRunning)
                {
                    _timer.Start();
                }

                break;
        }
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        _controller.Reset();
        Refresh();
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        _ = Navigation.PopAsync();
    }
}
