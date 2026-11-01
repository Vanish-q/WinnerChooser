using WinnerChooser.Core;

namespace WinnerChooser.Core.Tests;

/// <summary>Часы, которые двигаются только вручную.</summary>
internal sealed class TestClock : IClock
{
    public TimeSpan Now { get; private set; }

    public void Advance(TimeSpan delta) => Now += delta;

    public void AdvanceMilliseconds(double milliseconds) => Advance(TimeSpan.FromMilliseconds(milliseconds));
}

/// <summary>Генератор, который отдаёт заранее заданные значения по кругу.</summary>
internal sealed class ScriptedRandom : IRandomSource
{
    private readonly int[] _values;
    private int _index;

    public ScriptedRandom(params int[] values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Нужно хотя бы одно значение", nameof(values));
        }

        _values = values;
    }

    public int Next(int maxExclusive)
    {
        var value = _values[_index % _values.Length];
        _index++;
        return value % maxExclusive;
    }
}
