using CoreGraphics;
using Microsoft.Maui.Handlers;
using UIKit;

namespace WinnerChooser.App.Platforms.iOS;

/// <summary>
/// iOS-реализация мультитач-поля. Не компилируется, пока в проекте нет
/// таргета net10.0-ios, но контракт события совпадает с Android-версией.
/// </summary>
public class MultiTouchCanvasHandler : ViewHandler<Controls.MultiTouchCanvas, TouchView>
{
    public static readonly IPropertyMapper<Controls.MultiTouchCanvas, MultiTouchCanvasHandler> Mapper =
        new PropertyMapper<Controls.MultiTouchCanvas, MultiTouchCanvasHandler>(ViewMapper);

    public MultiTouchCanvasHandler()
        : base(Mapper)
    {
    }

    protected override TouchView CreatePlatformView() => new(this);
}

/// <summary>UIView с включённым MultipleTouchEnabled.</summary>
public sealed class TouchView : UIView
{
    private readonly MultiTouchCanvasHandler _handler;

    public TouchView(MultiTouchCanvasHandler handler)
        : base(CGRect.Empty)
    {
        _handler = handler;
        MultipleTouchEnabled = true;
        UserInteractionEnabled = true;
    }

    public override void TouchesBegan(NSSet touches, UIEvent? evt)
    {
        base.TouchesBegan(touches, evt);
        Emit(touches, TouchPhase.Down);
    }

    public override void TouchesMoved(NSSet touches, UIEvent? evt)
    {
        base.TouchesMoved(touches, evt);
        Emit(touches, TouchPhase.Moved);
    }

    public override void TouchesEnded(NSSet touches, UIEvent? evt)
    {
        base.TouchesEnded(touches, evt);
        Emit(touches, TouchPhase.Up);
    }

    public override void TouchesCancelled(NSSet touches, UIEvent? evt)
    {
        base.TouchesCancelled(touches, evt);
        Emit(touches, TouchPhase.Cancelled);
    }

    private void Emit(NSSet touches, TouchPhase phase)
    {
        var canvas = _handler.VirtualView;
        if (canvas is null)
        {
            return;
        }

        foreach (var item in touches)
        {
            if (item is not UITouch touch)
            {
                continue;
            }

            // На iOS точка уже в DIP, пересчёт плотности не нужен.
            var location = touch.LocationInView(this);
            canvas.RaiseTouch(touch.Handle.ToInt64(), new Point(location.X, location.Y), phase);
        }
    }
}
