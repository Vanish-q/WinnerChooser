using Android.Content;
using Android.Views;
using Microsoft.Maui.Handlers;
using WinnerChooser.App.Controls;

namespace WinnerChooser.App.Platforms.Android;

/// <summary>
/// Доставляет в <see cref="Controls.MultiTouchCanvas"/> все одновременные касания.
/// Штатные средства MAUI отдают только один указатель, поэтому используется
/// собственный Android-вид с OnTouchEvent.
/// </summary>
public class MultiTouchCanvasHandler : ViewHandler<Controls.MultiTouchCanvas, global::Android.Views.View>
{
    public static readonly IPropertyMapper<Controls.MultiTouchCanvas, MultiTouchCanvasHandler> Mapper =
        new PropertyMapper<Controls.MultiTouchCanvas, MultiTouchCanvasHandler>(ViewMapper);

    public MultiTouchCanvasHandler()
        : base(Mapper)
    {
    }

    protected override global::Android.Views.View CreatePlatformView()
        => new TouchSurface(Context, this);

    protected override void DisconnectHandler(global::Android.Views.View platformView)
    {
        platformView.Dispose();
        base.DisconnectHandler(platformView);
    }

    /// <summary>Плоский Android-вид: сам обрабатывает MotionEvent и возвращает true.</summary>
    private sealed class TouchSurface : global::Android.Views.View
    {
        private readonly MultiTouchCanvasHandler _handler;

        public TouchSurface(Context context, MultiTouchCanvasHandler handler)
            : base(context)
        {
            _handler = handler;
            Clickable = true;
            Focusable = true;
        }

        public override bool OnTouchEvent(MotionEvent? e)
        {
            if (e is null)
            {
                return base.OnTouchEvent(e);
            }

            var canvas = _handler.VirtualView;
            var density = Context?.Resources?.DisplayMetrics?.Density ?? 1f;
            if (density <= 0f)
            {
                density = 1f;
            }

            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    // Забираем жест у прокручиваемых предков, иначе Move/Up не дойдут.
                    Parent?.RequestDisallowInterceptTouchEvent(true);
                    Emit(canvas, e, e.ActionIndex, TouchPhase.Down, density);
                    break;

                case MotionEventActions.Move:
                    for (var i = 0; i < e.PointerCount; i++)
                    {
                        Emit(canvas, e, i, TouchPhase.Moved, density);
                    }

                    break;

                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    Emit(canvas, e, e.ActionIndex, TouchPhase.Up, density);
                    break;

                case MotionEventActions.Cancel:
                    // Система отменила жест целиком (звонок, уведомление, жест навигации).
                    for (var i = 0; i < e.PointerCount; i++)
                    {
                        Emit(canvas, e, i, TouchPhase.Cancelled, density);
                    }

                    break;
            }

            return true;
        }

        private static void Emit(
            Controls.MultiTouchCanvas? canvas,
            MotionEvent e,
            int pointerIndex,
            TouchPhase phase,
            float density)
        {
            if (canvas is null || pointerIndex < 0 || pointerIndex >= e.PointerCount)
            {
                return;
            }

            var id = e.GetPointerId(pointerIndex);
            var position = new Point(e.GetX(pointerIndex) / density, e.GetY(pointerIndex) / density);

#if DEBUG
            global::Android.Util.Log.Info(
                "WinnerChooser.Touch",
                $"{phase} id={id} x={position.X:F1} y={position.Y:F1} count={e.PointerCount}");
#endif

            canvas.RaiseTouch(id, position, phase);
        }
    }
}
