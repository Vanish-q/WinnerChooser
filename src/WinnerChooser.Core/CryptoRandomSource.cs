using System.Security.Cryptography;

namespace WinnerChooser.Core;

/// <summary>
/// Криптостойкий источник случайности: каждое число берётся из генератора ОС,
/// без seed и без предсказуемой последовательности между запусками.
/// </summary>
public sealed class CryptoRandomSource : IRandomSource
{
    /// <summary>Равномерно возвращает число из диапазона [0, maxExclusive).</summary>
    public int Next(int maxExclusive)
    {
        if (maxExclusive <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxExclusive),
                maxExclusive,
                "Диапазон должен быть положительным.");
        }

        return RandomNumberGenerator.GetInt32(maxExclusive);
    }
}
