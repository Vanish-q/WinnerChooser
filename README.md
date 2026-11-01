# Выбор победителя

[![CI](https://github.com/Vanish-q/WinnerChooser/actions/workflows/ci.yml/badge.svg)](https://github.com/Vanish-q/WinnerChooser/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/Vanish-q/WinnerChooser)](https://github.com/Vanish-q/WinnerChooser/releases/latest)

Android-приложение для честного случайного выбора: все кладут пальцы на экран, через несколько секунд после последнего касания приложение выбирает одного — его круг становится золотым. Никаких имён, регистрации, сети и разрешений.

> A simple Android app that picks a random winner among people touching the screen. Place at least two fingers on the display; after a short pause one finger is chosen uniformly at random and its circle turns gold. No network, no analytics, no permissions. Built with .NET MAUI.

## Как играть

1. Запустите приложение и нажмите «Жребий».
2. Положите на экран два пальца или больше.
3. Не двигайте пальцами: через 3 секунды (по умолчанию) после последнего касания один из кругов станет золотым.
4. Чтобы начать новый раунд, уберите пальцы и коснитесь экрана снова (не раньше чем через секунду после победы).
5. Выйти из режима — кнопка «×» в правом верхнем углу или системная кнопка «Назад».

Правила:

- минимум 2 пальца, максимум не ограничен; после 10-го цвета палитра повторяется, а номера на кругах остаются уникальными;
- каждый новый палец перезапускает отсчёт, движение пальца — нет;
- если убрать палец, он выбывает и отсчёт перезапускается; если осталось меньше двух — режим ждёт пальцы;
- поворот телефона не ломает раунд: ориентация зафиксирована портретной;
- уход в фон, блокировка экрана или разворачивание диалога сбрасывают раунд.

## Установка на телефон

Готовый APK лежит в [Releases](https://github.com/Vanish-q/WinnerChooser/releases/latest).

1. Скачайте `winnerchooser-<версия>.apk` на телефон.
2. Откройте файл и разрешите установку из этого источника (Android покажет предупреждение «неизвестный источник» — это нормально для APK вне Google Play).
3. Сверьте SHA-256 с файлом `.sha256`, если хотите убедиться, что скачали именно релизный файл.

Обновления ставятся поверх старой версии и требуют той же подписи. Если поставить APK с другой подписью, Android попросит сначала удалить приложение.

Windows не запускает APK: подсистема Windows Subsystem for Android закрыта в марте 2025 года. Запускайте на телефоне или эмуляторе.

## Разрешения

Приложение ничего не запрашивает:

- нет `INTERNET` и других сетевых доступов;
- нет `VIBRATE`: тактильный отклик идёт через системный `HapticFeedback`, который не требует разрешения и уважает настройку тактильного отклика в системе;
- нет доступа к файлам, контактам, геолокации и камере.

В APK остаётся только служебное разрешение `DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`, которое автоматически объявляет библиотека AndroidX. Оно не даёт доступа к данным. CI-релиз падает, если появится любое другое разрешение.

## Сборка

Нужны .NET SDK 10 и рабочая нагрузка `maui-android`:

```powershell
winget install --id Microsoft.DotNet.SDK.10 -e
dotnet workload install maui-android
```

Тесты:

```powershell
dotnet test
```

Debug-сборка и запуск на подключённом телефоне:

```powershell
dotnet build src/WinnerChooser.App -f net10.0-android -t:Run
```

Если Android SDK и JDK лежат в нестандартных местах, укажите их явно:

```powershell
dotnet build src/WinnerChooser.App -f net10.0-android -t:Run `
  -p:AndroidSdkDirectory="C:\Users\<вы>\AppData\Local\Android\Sdk" `
  -p:JavaSdkDirectory="C:\Program Files (x86)\Android\openjdk\jdk-17.0.14"
```

Для API 36 может понадобиться доустановка платформы:

```powershell
dotnet build src/WinnerChooser.App -f net10.0-android -t:InstallAndroidDependencies `
  -p:AcceptAndroidSDKLicenses=True
```

## Релиз

1. Один раз создайте ключ подписи (он остаётся вне репозитория, в `%USERPROFILE%\.winnerchooser\`):

   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts/create-keystore.ps1
   ```

2. Перенесите пароль из `keystore-password.txt` в менеджер паролей и удалите файл. **Потеря ключа означает, что установленные приложения больше нельзя обновить — только удалить и поставить заново.**

3. Загрузите секреты в GitHub (`gh auth login` должен быть выполнен):

   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts/set-gh-secrets.ps1
   ```

4. Опишите изменения в `CHANGELOG.md` секцией `## [x.y.z]`, закоммитьте, проставьте тег и отправьте:

   ```powershell
   git tag v1.0.0
   git push origin v1.0.0
   ```

Workflow `release.yml` проверит, что секция для версии существует и не пуста, соберёт подписанный APK, проверит подпись и отсутствие разрешений, посчитает SHA-256 и создаст релиз.

## Настройки

Кнопка с ползунками в правом верхнем углу главного экрана открывает настройки:

| Настройка | Диапазон | По умолчанию |
|---|---|---|
| Время до выбора | 1–10 с | 3 с |
| Размер кругов | 32–96 DIP | 64 DIP |
| Язык | как в системе / русский / английский | как в системе |

Настройки сохраняются на устройстве и применяются к следующему раунду. Язык меняется сразу, без перезапуска. Если системный язык не русский и не английский, интерфейс будет английским.

Значения по умолчанию и границы — `src/WinnerChooser.App/Services/AppSettings.cs`, строки интерфейса — `src/WinnerChooser.App/Services/L10n.cs`.

### Параметры логики

Все тайминги и размеры, включая недоступные из интерфейса, собраны в `src/WinnerChooser.Core/RoundOptions.cs`:

| Параметр | По умолчанию | Что делает |
|---|---|---|
| `CountdownDuration` | 3 с | пауза после последнего касания |
| `RevealLockDuration` | 1 с | сколько после победы касания игнорируются |
| `MinParticipants` | 2 | минимум пальцев для старта |
| `ParticipantRadius` | 64 DIP | радиус круга участника |
| `WinnerPulseDuration` | 600 мс | длительность пульсации победителя |

Палитра — `src/WinnerChooser.Core/TouchPalette.cs` (10 цветов, победный золотой `#FFD54F`).

## Устройство проекта

```
src/WinnerChooser.Core/   логика раунда без MAUI: состояние, тайминги, выбор победителя
src/WinnerChooser.App/    MAUI-приложение: страницы, настройки, локализация, отрисовка, мультитач
tests/                    xUnit-тесты логики
scripts/                  ключ, секреты GitHub, секция CHANGELOG
.github/workflows/        CI и релиз
```

Логика полностью отделена от UI и получает время через `IClock`, а случайность — через `IRandomSource`, поэтому тестируется без устройства.

## Лицензия

MIT, см. [LICENSE](LICENSE).
