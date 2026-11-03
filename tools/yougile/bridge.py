"""Мост GitHub → YouGile. Вызывается из GitHub Actions: python bridge.py pr | review | ci | message."""  # назначение
import json                                   # событие GitHub приходит JSON-файлом
import os                                     # переменные окружения Actions
import re                                     # код задачи ищем регулярным выражением
import sys                                    # режим работы — первый аргумент командной строки
import yg_api as yg                           # клиент YouGile из соседнего файла

BOARD = os.environ["YG_BOARD_ID"]             # ID доски этапа (переменная репозитория vars.YG_BOARD_ID)
CODE = re.compile(r"TASK-[0-9A-ZА-Я]{2}-\d{2}")  # TASK-TS-01 … TASK-TS-05
VERDICT = {"approved": "одобрено", "changes_requested": "возврат на доработку", "commented": "комментарии"}  # статусы ревью по-русски


def load_event():                             # событие, которое запустило workflow
    with open(os.environ["GITHUB_EVENT_PATH"], encoding="utf-8") as f:  # путь даёт сам GitHub
        return json.load(f)                   # словарь с данными PR, ревью и т. д.


def task_for(title):                          # карточка YouGile по заголовку PR
    m = CODE.search(title or "")              # ищем код задачи в заголовке
    if not m:                                 # заголовок без кода — PR не по задаче
        print(f"В заголовке нет кода задачи: {title!r}")  # пишем в лог и выходим
        return None, None                     # нечего обновлять
    return m.group(0), yg.find_task(m.group(0))  # код и карточка (или None)


def on_pr(ev):                                # открытие, готовность к ревью, слияние PR
    pr = ev["pull_request"]                   # данные PR
    code, task = task_for(pr["title"])        # карточка по коду
    if not task:                              # карточки нет — выходим
        return                                # ничего не делаем
    link = f"PR #{pr['number']}: {pr['html_url']}"  # ссылка для сообщения
    if ev["action"] in ("opened", "reopened") and pr["draft"]:  # черновик открыт
        yg.move(task["id"], BOARD, "В работе")  # задача начата
        yg.post(task["id"], f"Открыт черновик {link}")  # видно, что работа идёт
    elif ev["action"] in ("opened", "reopened", "ready_for_review"):  # PR готов к проверке
        yg.move(task["id"], BOARD, "Ревью")   # карточка в колонку ревью
        yg.post(task["id"], f"Готово к ревью: {link}")  # сигнал тимлиду и ревьюеру
    elif ev["action"] == "closed" and pr["merged"]:  # PR влит
        yg.move(task["id"], BOARD, "Принято")  # задача сдана (слияние в dev)
        yg.post(task["id"], f"{link} влит в {pr['base']['ref']}, коммит {pr['merge_commit_sha'][:7]}")  # след в истории


def on_review(ev):                            # ревью PR отправлено (человеком или Claude)
    pr, review = ev["pull_request"], ev["review"]  # данные PR и ревью
    code, task = task_for(pr["title"])        # карточка по коду задачи
    if not task:                              # карточки нет — выходим
        return                                # ничего не делаем
    state = review["state"].lower()           # approved / changes_requested / commented
    path = f"Ревью_{code}_PR-{pr['number']}_{review['id']}.md"  # имя файла отчёта
    with open(path, "w", encoding="utf-8") as f:  # сохраняем текст ревью в файл
        f.write(f"# Ревью {code}, PR #{pr['number']}\n\n{review.get('body') or ''}\n")  # тело ревью = отчёт
    if state == "changes_requested":          # есть замечания, мешающие слиянию
        yg.move(task["id"], BOARD, "Возврат")  # карточка в колонку возврата
    elif state == "approved":                 # код одобрен
        yg.move(task["id"], BOARD, "Проверка в Revit")  # дальше — функциональная проверка
    caption = f"Ревью {code}: {VERDICT.get(state, state)}. {review['html_url']}"  # подпись к файлу
    yg.post_file(task["id"], path, caption)   # отчёт файлом в чат задачи


def on_ci():                                  # итог CI; вызывается последним заданием ci.yml
    code, task = task_for(os.environ.get("PR_TITLE", ""))  # заголовок PR передаёт ci.yml
    if not task:                              # карточки нет — выходим
        return                                # ничего не делаем
    ok = os.environ["CI_RESULT"] == "success"  # итог задания build-test
    mark = "✅ CI зелёный: 5 версий Revit, тесты" if ok else "❌ CI упал"  # короткий итог
    yg.post(task["id"], f"{mark}. {os.environ['RUN_URL']}")  # ссылка на прогон для деталей


def on_message():                             # ручное сообщение: workflow_dispatch от тимлида или Claude
    code, task = task_for(os.environ["TASK"])  # код задачи из поля формы запуска
    if task:                                  # карточка найдена
        yg.post(task["id"], os.environ["TEXT"])  # текст из поля формы запуска


if __name__ == "__main__":                    # точка входа
    mode = sys.argv[1]                        # режим: pr / review / ci / message
    if mode == "pr":                          # событие pull_request
        on_pr(load_event())                   # обработка PR
    elif mode == "review":                    # событие pull_request_review
        on_review(load_event())               # обработка ревью
    elif mode == "ci":                        # вызов из ci.yml
        on_ci()                               # итог CI
    elif mode == "message":                   # ручной запуск
        on_message()                          # сообщение в чат
