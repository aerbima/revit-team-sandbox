# Revit Team Sandbox

Учебная песочница для проверки процесса командной разработки плагинов Revit: задачи в YouGile, ветки и pull request в GitHub, CI на пять версий Revit, ИИ-ревью, отчёты в чат задачи и на почту.

Код здесь — учебный мини-плагин «Маркировка помещений» с той же структурой, что у рабочих проектов. Реального кода, ТЗ и задач заказчиков в этом репозитории нет и быть не должно: **репозиторий открытый, всё, что в нём, видят все** — код, PR, комментарии, логи Actions.

## Состав

```
Sandbox.sln, Directory.Build.props     решение и конфигурации Debug/Release 2021–2025
src/Sandbox.Core                       логика без Revit (netstandard2.0)
src/Sandbox.Revit                      работа с документом; только здесь #if REVIT20xx
src/Sandbox.UI                         окна WPF
src/Sandbox.App                        лента и команды (IExternalApplication)
tests/Sandbox.Core.Tests               xUnit
installer/addin/Sandbox.addin          манифест для Revit
build/                                 раскладка Debug-сборки в Revit, установка сборки из CI, порог покрытия
tools/                                 мост YouGile, проверка трассировки «требование → тест»
tasks/                                 учебные задачи TASK-TS-01…05
docs/                                  мини-ТЗ, шаблон отчёта ревью, протокол теста
.github/                               CI, ИИ-ревью, письма, CODEOWNERS, шаблон PR
```

## Настройка — один раз, около 1,5 часа

Включайте функции по очереди: каждая следующая подключается переменной репозитория, до этого её workflow просто пропускается.

### 1. Репозиторий

1. На GitHub: **New repository** → имя `revit-team-sandbox`, **Public**, без README.
2. В этой папке:
   ```powershell
   git init -b master                                  # основная ветка — master, как в рабочем репозитории
   git add .                                           # все файлы песочницы
   git commit -m "Каркас песочницы"                    # первый коммит
   git remote add origin https://github.com/aerbima/revit-team-sandbox.git  # адрес нового репозитория
   git push -u origin master                           # отправить
   git switch -c dev                                   # ветка интеграции
   git push -u origin dev                              # отправить
   ```
3. **Settings → General → Default branch** → `dev`. В том же разделе включите **Automatically delete head branches** и оставьте из способов слияния только **Allow squash merging**.
4. **Actions** → дождитесь первого прогона `ci` на `dev`. Он должен быть зелёным: это проверка, что каркас собирается под все пять версий. Если красный — пришлите лог Claude.

### 2. Защита веток (сценарии 2, 8, 15)

**Settings → Branches → Add classic branch protection rule**, шаблон `dev`, затем то же для `master`:

- ☑ Require a pull request before merging → Required approvals: **1** → ☑ Dismiss stale pull request approvals → ☑ Require review from Code Owners
- ☑ Require status checks to pass → ☑ Require branches to be up to date → выберите `format`, `build-test (2021)` … `build-test (2025)` (проверки появляются в списке после первого прогона CI)
- ☐ Do not allow bypassing the above settings — **оставьте выключенным**: владелец кода здесь один (вы), и свои служебные PR (правка тасков, протокола) вы не сможете одобрить сами. Без этой галки у вас в PR будет кнопка слияния в обход правил; программистов правила держат в любом случае
- Force push и удаление ветки оставьте запрещёнными (по умолчанию)

Проверку `trace` в обязательные не добавляйте: она запускается только для PR, и на `push` её нет.

### 3. Тестовые программисты

Два отдельных аккаунта GitHub → **Settings → Collaborators → Add people**. Они работают ветками в этом репозитории, **не через форк**: в PR из форка секреты не передаются, и мост, ревью и письма не сработают.

### 4. YouGile (сценарии 1, 3–5, 12–13, 15, 18)

1. Отдельный тестовый проект и доска с колонками ровно такими: **Бэклог, Готово к работе, В работе, Ревью, Возврат, Проверка в Revit, Принято, Оплачено**.
2. Сотрудник-бот (отдельная почта) с доступом к проекту. Два тестовых программиста — сотрудники компании.
3. На своём компьютере: `python tools/yougile/setup_key.py` → ключ и ID досок.
4. GitHub → **Settings → Secrets and variables → Actions**: секрет `YOUGILE_API_KEY`, переменная `YG_BOARD_ID`.
5. В `tools/yougile/plan.json` замените `dev1@example.com` / `dev2@example.com` на почты тестовых программистов в YouGile; PR в `dev`.
6. **Actions → yougile-tasks → Run workflow**.

### 5. ИИ-ревью (сценарий 11)

1. Установите Claude GitHub App на этот репозиторий: в Claude Code команда `/install-github-app` или страница приложения на GitHub.
2. Токен: `claude setup-token` → секрет `CLAUDE_CODE_OAUTH_TOKEN`.
3. Переменная `CLAUDE_REVIEW` = `on`.

ИИ-ревью и `@claude` запускаются только для пользователей с правом записи — посторонние в открытом репозитории их не вызовут.

### 6. Письма (сценарии 12–13)

1. Gmail с двухэтапной проверкой → пароль приложения.
2. Секреты `MAIL_USERNAME` (адрес) и `MAIL_PASSWORD` (пароль приложения).
3. Переменная `DEV_EMAILS` — словарь «логин GitHub → почта»: `{"dev1-login":"dev1@mail.com","dev2-login":"dev2@mail.com"}`.

## Работа программиста

```powershell
git switch dev; git pull                               # свежая ветка интеграции
git switch -c feature/TS-01-mark-generator             # ветка задачи
# … код и тесты …
dotnet test Sandbox.sln -c Debug2025 -p:Platform=x64   # тесты локально
git push -u origin feature/TS-01-mark-generator        # отправить и открыть черновой PR «TASK-TS-01: …»
```

Сборка в конфигурации `Debug2021`…`Debug2025` сама раскладывает плагин в Revit этой версии: цель PostBuild в `Sandbox.App.csproj` вызывает `build\deploy-dev.ps1`. Сборки — в `%AppData%\Autodesk\Revit\Addins\<версия>\Sandbox\`, манифест `Sandbox.addin` — рядом. Перед сборкой закройте Revit этой версии, иначе сборка остановится с понятной ошибкой. В Release и в CI раскладки нет.

Тесты одной задачи: `dotnet test Sandbox.sln -c Debug2025 -p:Platform=x64 --filter "Task=TS-01"`.

Установка сборки из CI в Revit: скачать артефакт `build-revit2025` со страницы прогона, затем
`powershell -ExecutionPolicy Bypass -File build\install-build.ps1 -RevitVersion 2025 -Zip <путь к zip>`.
Удаление: `build\uninstall.ps1 -RevitVersion 2025`.

## Что дальше

Пройдите `docs/Протокол-теста.md`. Задачи и порядок — `tasks/00_Порядок-и-зависимости.md`.
