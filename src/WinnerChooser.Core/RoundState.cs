namespace WinnerChooser.Core;

/// <summary>Состояние раунда.</summary>
public enum RoundState
{
    /// <summary>Ждём пальцы: подсказка на экране, таймер не идёт.</summary>
    Waiting,

    /// <summary>На экране не меньше двух пальцев, идёт обратный отсчёт.</summary>
    Countdown,

    /// <summary>Победитель выбран, круг подсвечен.</summary>
    WinnerRevealed,
}
