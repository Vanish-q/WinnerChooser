namespace WinnerChooser.Core;

/// <summary>Источник случайности — заменяется в тестах на детерминированный.</summary>
public interface IRandomSource
{
    /// <summary>Возвращает число из диапазона [0, maxExclusive).</summary>
    int Next(int maxExclusive);
}

/// <summary>
/// Обычный генератор псевдослучайных чисел. Используется в тестах и для
/// воспроизводимых сценариев: в приложении победителя выбирает
/// <see cref="CryptoRandomSource"/>.
/// </summary>
public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random _random;

    public SystemRandomSource()
        : this(Random.Shared)
    {
    }

    public SystemRandomSource(Random random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public int Next(int maxExclusive) => _random.Next(maxExclusive);
}
