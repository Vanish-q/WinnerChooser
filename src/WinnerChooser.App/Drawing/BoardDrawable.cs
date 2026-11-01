using Microsoft.Maui.Graphics;
using WinnerChooser.Core;

namespace WinnerChooser.App.Drawing;

/// <summary>
/// Рисует поле: круги участников с номерами, кольцо обратного отсчёта,
/// золотой круг победителя с пульсацией.
/// </summary>
public sealed class BoardDrawable : IDrawable
{
    private static readonly Color Background = Color.FromArgb("#121212");
    private static readonly Color Winner = Color.FromUint(TouchPalette.WinnerColor);
    private static readonly Color RingBack = Color.FromRgba(255, 255, 255, 40);
    private static readonly Color RingFront = Color.FromArgb("#FFFFFF");

    private IReadOnlyList<Participant> _participants = Array.Empty<Participant>();
    private RoundState _state = RoundState.Waiting;
    private long _winnerId = -1;
    private double _progress;
    private double _pulse;
    private double _radius = 64d;

    public void Update(
        IReadOnlyList<Participant> participants,
        RoundState state,
        long winnerId,
        double progress,
        double pulse,
        double radius)
    {
        _participants = participants;
        _state = state;
        _winnerId = winnerId;
        _progress = progress;
        _pulse = pulse;
        _radius = radius;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FillColor = Background;
        canvas.FillRectangle(dirtyRect);

        foreach (var participant in _participants)
        {
            var isWinner = _state == RoundState.WinnerRevealed && participant.Id == _winnerId;

            var radius = (float)_radius;
            if (isWinner)
            {
                // Пульсация: круг на мгновение раздувается и возвращается.
                radius *= (float)(1d + (0.12d * Math.Sin(Math.Clamp(_pulse, 0d, 1d) * Math.PI)));
            }

            var centerX = participant.Position.X;
            var centerY = participant.Position.Y;

            if (!isWinner && _state == RoundState.Countdown)
            {
                DrawCountdownRing(canvas, centerX, centerY, radius);
            }

            canvas.FillColor = isWinner ? Winner : Color.FromUint(participant.Color);
            canvas.FillCircle(centerX, centerY, radius);

            if (isWinner)
            {
                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = 6;
                canvas.DrawCircle(centerX, centerY, radius);
            }

            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontSize = 28;
            canvas.FontColor = IsLight(participant.Color) ? Colors.Black : Colors.White;
            canvas.DrawString(
                participant.Number.ToString(),
                centerX - radius,
                centerY - radius,
                radius * 2,
                radius * 2,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }

        canvas.RestoreState();
    }

    private void DrawCountdownRing(ICanvas canvas, float centerX, float centerY, float radius)
    {
        var ringRadius = radius + 12f;

        canvas.StrokeSize = 6;
        canvas.StrokeColor = RingBack;
        canvas.DrawCircle(centerX, centerY, ringRadius);

        canvas.StrokeColor = RingFront;
        var sweep = (float)(360d * Math.Clamp(_progress, 0d, 1d));
        canvas.DrawArc(
            centerX - ringRadius,
            centerY - ringRadius,
            ringRadius * 2,
            ringRadius * 2,
            90,
            90 - sweep,
            clockwise: true,
            closed: false);
    }

    /// <summary>Светлый ли цвет — чтобы подпись была контрастной.</summary>
    private static bool IsLight(uint argb)
    {
        var r = (argb >> 16) & 0xFF;
        var g = (argb >> 8) & 0xFF;
        var b = argb & 0xFF;
        var luminance = (0.299 * r) + (0.587 * g) + (0.114 * b);
        return luminance > 186;
    }
}
