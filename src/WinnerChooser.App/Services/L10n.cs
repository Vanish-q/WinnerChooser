using System.Globalization;
using Microsoft.Maui.Storage;

namespace WinnerChooser.App.Services;

/// <summary>Выбранный язык интерфейса. System — брать из системных настроек.</summary>
public enum AppLanguage
{
    System = 0,
    Russian = 1,
    English = 2,
}

/// <summary>
/// Простая локализация без .resx: словарь строк на два языка и выбор языка
/// с сохранением в настройках. Русский — при русской системной локали,
/// во всех остальных случаях английский.
/// </summary>
public static class L10n
{
    private const string LanguageKey = "app_language";

    private static readonly Dictionary<string, (string Ru, string En)> Text = new()
    {
        ["AppTitle"] = ("Выбор победителя", "Winner Chooser"),
        ["Modes"] = ("Режимы", "Modes"),
        ["ModeDraw"] = ("Жребий", "Lucky Draw"),
        ["ModeDrawDescription"] = (
            "Режим «Жребий»: случайный выбор среди касаний",
            "Lucky Draw mode: random pick among touches"),
        ["Settings"] = ("Настройки", "Settings"),
        ["Back"] = ("Назад", "Back"),
        ["CountdownTitle"] = ("Время до выбора", "Time before pick"),
        ["CountdownHint"] = (
            "Через сколько секунд после последнего касания выбирается победитель",
            "Seconds after the last touch before the winner is picked"),
        ["CircleSizeTitle"] = ("Размер кругов", "Circle size"),
        ["CircleSizeHint"] = (
            "Радиус круга участника",
            "Radius of a participant circle"),
        ["LanguageTitle"] = ("Язык", "Language"),
        ["LanguageSystem"] = ("Как в системе", "System language"),
        ["LanguageRussian"] = ("Русский", "Русский"),
        ["LanguageEnglish"] = ("English", "English"),
        ["SecondsShort"] = ("с", "s"),
        ["HintPlaceFingers"] = (
            "Положите пальцы на экран",
            "Place your fingers on the screen"),
        ["HintNeedMore"] = (
            "Нужен ещё один палец",
            "One more finger needed"),
        ["CloseMode"] = ("Закрыть режим", "Close mode"),
    };

    public static AppLanguage Setting { get; private set; } = AppLanguage.System;

    public static bool IsRussian => Resolve(Setting) == "ru";

    /// <summary>Прочитать сохранённый язык. Вызывается один раз при старте.</summary>
    public static void Initialize()
    {
        var saved = Preferences.Default.Get(LanguageKey, "system");
        Setting = saved switch
        {
            "ru" => AppLanguage.Russian,
            "en" => AppLanguage.English,
            _ => AppLanguage.System,
        };
    }

    /// <summary>Сменить и сохранить язык.</summary>
    public static void SetLanguage(AppLanguage language)
    {
        Setting = language;
        Preferences.Default.Set(LanguageKey, language switch
        {
            AppLanguage.Russian => "ru",
            AppLanguage.English => "en",
            _ => "system",
        });
    }

    /// <summary>Строка по ключу на текущем языке.</summary>
    public static string T(string key)
        => Text.TryGetValue(key, out var value)
            ? (IsRussian ? value.Ru : value.En)
            : key;

    private static string Resolve(AppLanguage setting) => setting switch
    {
        AppLanguage.Russian => "ru",
        AppLanguage.English => "en",
        _ => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "ru" : "en",
    };
}
