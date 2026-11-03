"""Сверка «требования таска ↔ тесты»: какие ТР из таска покрыты xUnit-тестами с [Trait("TR", "…")]."""  # назначение
import glob                                   # перебор файлов
import os                                     # переменные окружения GitHub Actions
import re                                     # разбор ТР и атрибутов тестов
import sys                                    # код выхода для CI

title = os.environ.get("PR_TITLE", "")        # заголовок PR: «TASK-TS-01: генератор марок»
body = os.environ.get("PR_BODY", "") or ""    # описание PR (может быть пустым)
m = re.search(r"TASK-([0-9A-ZА-Я]{2}-\d{2})", title)  # код задачи из заголовка
if not m:                                     # PR без кода задачи
    print("В заголовке PR нет кода TASK-…: сверка пропущена")  # сообщение в лог
    sys.exit(0)                               # не роняем CI: это может быть служебный PR
task = m.group(1)                             # «TS-01»

files = glob.glob(f"tasks/TASK-{task}_*.md")  # файл таска
if not files:                                 # таска нет в репозитории
    print(f"Файл таска TASK-{task} не найден")  # сообщение в лог
    sys.exit(1)                               # ошибка: PR ссылается на несуществующий таск
text = open(files[0], encoding="utf-8").read()  # содержимое таска
line = re.search(r"\*\*Требования ТЗ:\*\*\s*(.+)", text).group(1)  # строка «Требования ТЗ»

required = []                                 # список ТР таска в порядке упоминания
token = r"(?:ТР-)?([0-9]+|[А-ЯЁ])\.(\d{2})"   # «ТР-У.01», «У.02», «ТР-5.07»
for a, b, c, d in re.findall(token + r"(?:…" + token + r")?", line):  # одиночные ТР и диапазоны «…»
    if c and a == c:                          # диапазон внутри одного раздела: ТР-У.01…ТР-У.04
        required += [f"{a}.{n:02d}" for n in range(int(b), int(d) + 1)]  # разворачиваем
    else:                                     # одиночное требование
        required.append(f"{a}.{b}")           # «У.05»
required = list(dict.fromkeys(required))      # убираем повторы, порядок сохраняем

tests = "\n".join(open(p, encoding="utf-8").read() for p in glob.glob("tests/**/*.cs", recursive=True))  # все тесты
covered = re.findall(r'\[Trait\(\s*"TR"\s*,\s*"([^"]+)"\s*\)\]', tests)  # значения TR в атрибутах
linked = len(re.findall(r'\[Trait\(\s*"Task"\s*,\s*"' + re.escape(task) + r'"\s*\)\]', tests))  # тестов с Task=<код>

rows = ["| ТР | Тестов | Статус |", "|---|---|---|"]  # таблица для сводки прогона
for tr in required:                           # по каждому требованию таска
    n = covered.count(tr)                     # сколько тестов его проверяют
    rows.append(f"| ТР-{tr} | {n} | {'покрыто тестом' if n else 'проверить вручную или в Revit'} |")  # строка таблицы
report = f"### Трассировка TASK-{task}\n\nТестов с Trait Task={task}: {linked}\n\n" + "\n".join(rows)  # итог
print(report)                                 # в лог прогона
summary = os.environ.get("GITHUB_STEP_SUMMARY")  # файл сводки прогона в Actions
if summary:                                   # запущено в GitHub Actions
    open(summary, "a", encoding="utf-8").write(report + "\n")  # сводка видна на странице прогона

if linked == 0 and "Тесты: не применимо" not in body:  # нет ни одного теста задачи и нет обоснования
    print("Нет тестов с [Trait(\"Task\", …)] для этой задачи. Добавьте тесты или строку "
          "«Тесты: не применимо — причина» в описание PR.")  # что сделать, чтобы пройти проверку
    sys.exit(1)                               # проверка красная — PR не влить
