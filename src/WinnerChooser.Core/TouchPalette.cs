namespace WinnerChooser.Core;

/// <summary>Палитра кругов. Ровно 10 цветов, дальше идёт повтор по кругу.</summary>
public static class TouchPalette
{
    /// <summary>Цвета в формате 0xAARRGGBB.</summary>
    public static readonly IReadOnlyList<uint> Colors = new uint[]
    {
        0xFFEF5350, // красный
        0xFF42A5F5, // синий
        0xFF66BB6A, // зелёный
        0xFFAB47BC, // фиолетовый
        0xFFFFA726, // оранжевый
        0xFF26C6DA, // бирюзовый
        0xFFEC407A, // розовый
        0xFFD4E157, // салатовый
        0xFF8D6E63, // коричневый
        0xFF78909C, // серо-синий
    };

    /// <summary>Цвет победителя.</summary>
    public const uint WinnerColor = 0xFFFFD54F;

    /// <summary>Цвет круга по порядковому номеру участника (с 1).</summary>
    public static uint ColorForNumber(int number)
    {
        var index = (number - 1) % Colors.Count;
        if (index < 0)
        {
            index += Colors.Count;
        }

        return Colors[index];
    }
}
