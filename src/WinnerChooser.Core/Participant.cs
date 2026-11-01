namespace WinnerChooser.Core;

/// <summary>Один палец, лежащий на экране.</summary>
public sealed class Participant
{
    internal Participant(long id, int number, PointF position)
    {
        Id = id;
        Number = number;
        Position = position;
    }

    /// <summary>Идентификатор указателя ОС, стабильный, пока палец на экране.</summary>
    public long Id { get; }

    /// <summary>Порядковый номер участника в раунде, с 1. Он же подпись на круге.</summary>
    public int Number { get; }

    /// <summary>Индекс цвета в палитре (номер участника по модулю 10).</summary>
    public int ColorIndex => (Number - 1) % TouchPalette.Colors.Count;

    /// <summary>Текущая позиция в DIP.</summary>
    public PointF Position { get; internal set; }

    /// <summary>Цвет круга в формате 0xAARRGGBB.</summary>
    public uint Color => TouchPalette.ColorForNumber(Number);
}
