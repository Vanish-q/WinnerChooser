namespace WinnerChooser.Core;

/// <summary>
/// Вся игровая логика раунда. Не знает ничего о MAUI и Android:
/// координаты в DIP, время — из <see cref="IClock"/>, случайность — из <see cref="IRandomSource"/>.
/// Все методы вызываются из одного потока (UI).
/// </summary>
public sealed class RoundController
{
    private readonly List<Participant> _participants = new();
    private readonly IClock _clock;
    private readonly IRandomSource _random;
    private readonly RoundOptions _options;

    private TimeSpan? _deadline;
    private TimeSpan _revealedAt;
    private int _nextNumber = 1;

    public RoundController(IClock clock, IRandomSource random, RoundOptions? options = null)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _options = options ?? new RoundOptions();
    }

    /// <summary>Состояние изменилось — пора перерисовать экран.</summary>
    public event EventHandler? Changed;

    /// <summary>Победитель выбран: самое время для вибрации.</summary>
    public event EventHandler<Participant>? WinnerSelected;

    public RoundState State { get; private set; } = RoundState.Waiting;

    public IReadOnlyList<Participant> Participants => _participants;

    public Participant? Winner { get; private set; }

    public RoundOptions Options => _options;

    /// <summary>Правда, пока касания не должны начинать новый раунд.</summary>
    public bool IsRevealLocked =>
        State == RoundState.WinnerRevealed &&
        _clock.Now - _revealedAt < _options.RevealLockDuration;

    /// <summary>Сколько осталось до выбора. Вне отсчёта — ноль.</summary>
    public TimeSpan Remaining
    {
        get
        {
            if (State != RoundState.Countdown || _deadline is null)
            {
                return TimeSpan.Zero;
            }

            var remaining = _deadline.Value - _clock.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    /// <summary>Прогресс отсчёта от 1 (только что начали) до 0 (время вышло).</summary>
    public double CountdownProgress
    {
        get
        {
            if (_options.CountdownDuration <= TimeSpan.Zero)
            {
                return 0d;
            }

            var progress = Remaining.TotalMilliseconds / _options.CountdownDuration.TotalMilliseconds;
            return progress < 0d ? 0d : progress > 1d ? 1d : progress;
        }
    }

    /// <summary>Палец коснулся экрана.</summary>
    public void PointerDown(long id, PointF position)
    {
        if (State == RoundState.WinnerRevealed)
        {
            // Во время блокировки касания не принимаем, после — начинаем заново.
            if (IsRevealLocked)
            {
                return;
            }

            Reset();
        }

        if (Find(id) is not null)
        {
            return;
        }

        _participants.Add(new Participant(id, _nextNumber++, position));

        if (_participants.Count >= _options.MinParticipants)
        {
            StartCountdown();
        }
        else
        {
            StopCountdown();
        }

        RaiseChanged();
    }

    /// <summary>Палец сдвинулся. Дедлайн не трогаем.</summary>
    public void PointerMoved(long id, PointF position)
    {
        if (State == RoundState.WinnerRevealed)
        {
            // Победитель уже показан: состав и позиции заморожены.
            return;
        }

        var participant = Find(id);
        if (participant is null)
        {
            return;
        }

        participant.Position = position;
        RaiseChanged();
    }

    /// <summary>Палец убран.</summary>
    public void PointerUp(long id)
    {
        if (State == RoundState.WinnerRevealed)
        {
            return;
        }

        var participant = Find(id);
        if (participant is null)
        {
            return;
        }

        _participants.Remove(participant);

        if (_participants.Count >= _options.MinParticipants)
        {
            StartCountdown();
        }
        else
        {
            StopCountdown();
        }

        RaiseChanged();
    }

    /// <summary>
    /// Продвинуть время. Вызывается каждый кадр из UI-таймера.
    /// Победитель выбирается из состава на момент вызова.
    /// </summary>
    public void Tick()
    {
        if (State != RoundState.Countdown || _deadline is null)
        {
            return;
        }

        var now = _clock.Now;
        if (now < _deadline.Value)
        {
            return;
        }

        if (_participants.Count < _options.MinParticipants)
        {
            // Защита от гонки: пока шёл отсчёт, состав успел уменьшиться.
            StopCountdown();
            RaiseChanged();
            return;
        }

        Winner = _participants[_random.Next(_participants.Count)];
        State = RoundState.WinnerRevealed;
        _revealedAt = now;
        _deadline = null;

        WinnerSelected?.Invoke(this, Winner);
        RaiseChanged();
    }

    /// <summary>Сбросить раунд в исходное состояние.</summary>
    public void Reset()
    {
        _participants.Clear();
        _nextNumber = 1;
        _deadline = null;
        Winner = null;
        State = RoundState.Waiting;
        RaiseChanged();
    }

    private Participant? Find(long id)
    {
        foreach (var participant in _participants)
        {
            if (participant.Id == id)
            {
                return participant;
            }
        }

        return null;
    }

    private void StartCountdown()
    {
        State = RoundState.Countdown;
        _deadline = _clock.Now + _options.CountdownDuration;
    }

    private void StopCountdown()
    {
        State = RoundState.Waiting;
        _deadline = null;
    }

    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
