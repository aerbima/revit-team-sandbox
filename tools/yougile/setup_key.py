"""Разовая настройка YouGile: ключ API для бота и ID досок. Запускать на своём компьютере: python tools/yougile/setup_key.py"""  # назначение
import getpass                                # пароль вводится без отображения на экране
import json                                   # тела запросов и ответов
import urllib.request                         # HTTP-клиент стандартной библиотеки

BASE = "https://yougile.com/api-v2"           # адрес API YouGile


def call(method, path, body=None, key=None):  # общий вызов API
    data = None if body is None else json.dumps(body).encode("utf-8")  # тело запроса
    req = urllib.request.Request(BASE + path, data=data, method=method)  # запрос
    req.add_header("Content-Type", "application/json")  # формат тела
    if key:                                   # методы, кроме auth, требуют ключ
        req.add_header("Authorization", f"Bearer {key}")  # авторизация ключом
    with urllib.request.urlopen(req, timeout=30) as resp:  # выполнить
        return json.loads(resp.read() or b"{}")  # ответ как словарь


def main():                                   # точка входа
    login = input("Логин (почта) сотрудника-бота YouGile: ").strip()  # отдельный сотрудник, не ваш личный аккаунт
    password = getpass.getpass("Пароль: ")    # не сохраняется и не печатается
    companies = call("POST", "/auth/companies", {"login": login, "password": password})["content"]  # компании бота
    for i, c in enumerate(companies):         # список компаний
        print(f"{i}: {c['name']}  ({c['id']})")  # номер для выбора
    company = companies[int(input("Номер компании: "))]["id"]  # выбранная компания
    key = call("POST", "/auth/keys", {"login": login, "password": password, "companyId": company})["key"]  # новый ключ
    print(f"\nКлюч API (в GitHub → Settings → Secrets → YOUGILE_API_KEY):\n{key}\n")  # показать один раз
    for b in call("GET", "/boards", key=key)["content"]:  # доски компании
        print(f"Доска «{b['title']}»: {b['id']}")  # ID → в переменную YG_BOARD_ID


if __name__ == "__main__":                    # запуск как скрипта
    main()                                    # настройка
