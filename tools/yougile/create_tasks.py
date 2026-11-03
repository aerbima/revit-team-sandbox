"""Создаёт карточки YouGile из файлов tasks/TASK-*.md. Повторный запуск не плодит дубликаты."""  # назначение
import datetime as dt                         # даты плана → метки времени для стикера «Дедлайн»
import glob                                   # перебор файлов тасков
import json                                   # план назначений в plan.json
import os                                     # переменные окружения
import re                                     # разбор markdown-файла таска
import urllib.parse                           # экранирование e-mail в строке запроса
import yg_api as yg                           # клиент YouGile из соседнего файла

BOARD = os.environ["YG_BOARD_ID"]             # доска этапа
REPO = os.environ.get("REPO_URL", "https://github.com/aerbima/revit-team-sandbox") + "/blob/dev/tasks/"  # ссылка на файл таска; адрес репозитория передаёт workflow
PLAN = json.load(open("tools/yougile/plan.json", encoding="utf-8"))  # кто, когда, сколько часов


def ms(day):                                  # '2026-11-09' → миллисекунды UTC, как ждёт API
    return int(dt.datetime.fromisoformat(day).replace(tzinfo=dt.timezone.utc).timestamp() * 1000)  # перевод


def user_id(email):                           # ID сотрудника YouGile по почте
    query = urllib.parse.urlencode({"email": email})  # фильтр по e-mail
    found = yg.call("GET", f"/users?{query}").get("content", [])  # поиск сотрудника
    return [found[0]["id"]] if found else []  # API ждёт массив ID исполнителей


def parse(path):                              # вытащить из таска то, что нужно карточке
    text = open(path, encoding="utf-8").read()  # весь markdown таска
    title = re.search(r"^## (TASK-.+)$", text, re.M).group(1).strip()  # «TASK-TS-01 · Генератор марок»
    deps = re.search(r"\*\*Зависит от:\*\*\s*(.+?)\s*\*\*Требования", text)  # строка зависимостей
    reqs = re.search(r"\*\*Требования ТЗ:\*\*\s*(.+)", text)  # строка требований ТЗ
    done = re.search(r"### (?:Готово, когда|Ведомость)[^\n]*\s*(.+?)(?:\n### |\n---|\Z)", text, re.S)  # критерии или ведомость приёмки
    items = re.findall(r"- \[ \] (.+)", done.group(1)) if done else []  # пункты чек-листа
    return title, (deps.group(1) if deps else "—"), (reqs.group(1) if reqs else "—"), items  # кортеж для карточки


def main():                                   # точка входа
    backlog = yg.column_id(BOARD, "Бэклог")   # новые карточки — в бэклог
    for path in sorted(glob.glob("tasks/TASK-*.md")):  # все таски по порядку номеров
        title, deps, reqs, items = parse(path)  # данные из файла
        code = title.split(" ")[0]            # TASK-TS-01
        if yg.find_task(code):                # карточка уже есть
            print(f"{code}: уже есть, пропуск")  # не создаём дубликат
            continue                          # к следующему файлу
        name = os.path.basename(path)         # имя файла для ссылки
        body = {                              # тело запроса POST /tasks
            "title": title,                   # заголовок начинается с кода — по нему ищет мост
            "columnId": backlog,              # колонка «Бэклог»
            "description": (f'<p>Таск: <a href="{REPO}{urllib.parse.quote(name)}">{name}</a></p>'  # ссылка на таск
                            f"<p>Зависит от: {deps}</p><p>Требования ТЗ: {reqs}</p>"),  # зависимости и ТР
            "checklists": [{"title": "Готово, когда",  # критерии закрытия из таска
                            "items": [{"title": i, "isCompleted": False} for i in items]}],  # пункты не отмечены
        }
        plan = PLAN.get(code)                 # назначение из plan.json, если есть
        if plan:                              # задача уже распланирована
            body["assigned"] = user_id(plan["assignee"])  # исполнитель
            body["deadline"] = {"startDate": ms(plan["start"]), "deadline": ms(plan["deadline"]),  # даты на Ганте
                                "withTime": False, "blockedPoints": [], "links": []}  # обязательные поля стикера
            body["timeTracking"] = {"plan": plan["hours"], "work": 0}  # план часов, факт пока 0
        yg.call("POST", "/tasks", body)        # создаём карточку
        print(f"{code}: создана")              # след в логе


if __name__ == "__main__":                    # запуск как скрипта
    main()                                    # создаём карточки
