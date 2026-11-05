"""Разовая настройка YouGile: ключ API для бота и ID досок. Запускать на своём компьютере: python tools/yougile/setup_key.py"""  # назначение
import getpass                                # пароль вводится без отображения на экране
import json                                   # тела запросов и ответов
import sys                                    # выход с кодом ошибки
import urllib.error                           # HTTPError — чтобы показать ответ сервера
import urllib.request                         # HTTP-клиент стандартной библиотеки

HOSTS = ["https://en.yougile.com", "https://ru.yougile.com", "https://yougile.com"]  # у YouGile несколько адресов; аккаунт живёт на одном из них
BASE = HOSTS[0] + "/api-v2"                   # текущий адрес API; меняется, если вход прошёл на втором адресе


def call(method, path, body=None, key=None, quiet=False):  # общий вызов API; quiet — вернуть None вместо выхода
    data = None if body is None else json.dumps(body).encode("utf-8")  # тело запроса
    req = urllib.request.Request(BASE + path, data=data, method=method)  # запрос
    req.add_header("Content-Type", "application/json")  # формат тела
    if key:                                   # методы, кроме auth, требуют ключ
        req.add_header("Authorization", f"Bearer {key}")  # авторизация ключом
    try:                                      # ошибку сервера показываем текстом, а не трассировкой
        with urllib.request.urlopen(req, timeout=30) as resp:  # выполнить
            return json.loads(resp.read() or b"{}")  # ответ как словарь
    except urllib.error.HTTPError as e:       # сервер ответил 4xx/5xx
        if quiet:                             # вызывающий сам решит, что делать
            print(f"   ответ {e.code}: {e.read().decode('utf-8', 'replace')[:200]}")  # код и текст ответа для диагностики
            return None                       # например, попробует следующий адрес
    except urllib.error.URLError as e:        # адрес недоступен: DNS, прокси, сеть
        if quiet:                             # при переборе адресов не останавливаемся
            print(f"   нет соединения: {e.reason}")  # причина для диагностики
            return None                       # пробуем следующий адрес
        raise                                 # в обычном вызове — стандартная ошибка
        print(f"\nYouGile ответил {e.code} на {method} {path}: {e.read().decode('utf-8', 'replace')}")  # причина из ответа сервера
        sys.exit(1)                           # дальше идти бессмысленно


def ask_password():                           # ввод пароля
    mode = input("Показывать пароль при вводе? (д/н, по умолчанию н): ").strip().lower()  # видимый ввод проще вставить
    if mode.startswith(("д", "y")):           # видимый ввод: вставка правой кнопкой или Ctrl+V работает как обычно
        return input("Пароль: ").strip()      # пробелы по краям убираем
    print("Скрытый ввод: вставляйте ПРАВОЙ кнопкой мыши — Ctrl+V здесь вставляет служебный символ.")  # частая причина ошибки 400
    return getpass.getpass("Пароль: ").strip()  # скрытый ввод


def main():                                   # точка входа
    login = input("Логин (почта) сотрудника-бота YouGile: ").strip()  # отдельный сотрудник, не ваш личный аккаунт
    password = ask_password()                 # пароль бота в YouGile
    if any(ord(ch) < 32 for ch in password):  # в пароль попал управляющий символ (обычно от Ctrl+V)
        print("В пароле есть служебный символ — вероятно, вставка через Ctrl+V. Запустите снова и вставьте правой кнопкой мыши.")  # подсказка
        sys.exit(1)                           # не отправляем заведомо неверный пароль
    print(f"Длина введённого пароля: {len(password)} символов")  # сверка без показа самого пароля
    global BASE                               # адрес API меняем для всех следующих вызовов
    answer = None                             # ответ со списком компаний
    for host in HOSTS:                        # пробуем оба адреса YouGile по очереди
        BASE = host + "/api-v2"               # адрес API этого сервера
        print(f"{host}: пробую вход…")         # строка перед ответом сервера
        answer = call("POST", "/auth/companies", {"login": login, "password": password}, quiet=True)  # вход
        print(f"{host}: {'вход выполнен' if answer else 'вход не выполнен'}")  # итог по адресу
        if answer:                            # нашли сервер, где живёт аккаунт
            break                             # дальше работаем с ним
    if not answer:                            # ни один адрес не принял
        print("Логин и пароль не подошли ни на одном адресе YouGile. Проверьте адрес в строке браузера, где бот входит.")  # подсказка
        sys.exit(1)                           # без входа ключ не получить
    companies = answer.get("content", [])     # компании бота
    if not companies:                         # бот не состоит ни в одной компании
        print("У этого аккаунта нет компаний: примите приглашение в компанию из письма YouGile и повторите.")  # подсказка
        sys.exit(1)                           # без компании ключ не выдать
    for i, c in enumerate(companies):         # список компаний
        print(f"{i}: {c['name']}  ({c['id']})")  # номер для выбора
    company = companies[int(input("Номер компании: "))]["id"]  # выбранная компания
    key = call("POST", "/auth/keys", {"login": login, "password": password, "companyId": company})["key"]  # новый ключ
    print(f"\nКлюч API (в GitHub → Settings → Secrets → YOUGILE_API_KEY):\n{key}\n")  # показать один раз
    for b in call("GET", "/boards", key=key)["content"]:  # доски компании
        print(f"Доска «{b['title']}»: {b['id']}")  # ID → в переменную YG_BOARD_ID
    print(f"\nАдрес API: {BASE}")             # это значение — в переменную репозитория YG_API_BASE


if __name__ == "__main__":                    # запуск как скрипта
    main()                                    # настройка
