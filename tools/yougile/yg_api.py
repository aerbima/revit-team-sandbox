"""Мини-клиент YouGile REST API v2: поиск карточки, перенос по колонкам, сообщение и файл в чат задачи."""  # назначение модуля
import json                                   # тело запросов и ответов — JSON
import os                                     # ключ API и ID доски берём из переменных окружения
import time                                   # пауза между запросами: у API лимит частоты
import urllib.parse                           # экранирование параметров строки запроса
import urllib.request                         # HTTP-клиент стандартной библиотеки, без pip install
import uuid                                   # граница частей multipart при загрузке файла

BASE = os.environ.get("YG_API_BASE") or "https://yougile.com/api-v2"  # адрес API: переменная репозитория YG_API_BASE, например https://en.yougile.com/api-v2
KEY = os.environ["YOUGILE_API_KEY"]           # ключ хранится в GitHub Secrets, в коде его нет
PAUSE = 1.3                                   # лимит — не более 50 запросов в минуту на компанию


def call(method, path, body=None):            # общий вызов: метод, путь ресурса, тело
    data = None if body is None else json.dumps(body).encode("utf-8")  # тело запроса в UTF-8
    req = urllib.request.Request(BASE + path, data=data, method=method)  # собираем запрос
    req.add_header("Authorization", f"Bearer {KEY}")  # авторизация ключом API
    req.add_header("Content-Type", "application/json")  # формат тела, которого ждёт API
    with urllib.request.urlopen(req, timeout=30) as resp:  # выполняем; ошибка HTTP бросит исключение
        raw = resp.read()                     # читаем ответ целиком
    time.sleep(PAUSE)                         # соблюдаем лимит частоты
    return json.loads(raw) if raw else {}     # пустой ответ превращаем в пустой словарь


def find_task(code):                          # карточка по коду задачи, например TASK-TS-01
    query = urllib.parse.urlencode({"title": code, "limit": 20})  # поиск по заголовку
    for task in call("GET", f"/task-list?{query}").get("content", []):  # перебираем найденное
        if task.get("title", "").startswith(code):  # заголовок карточки начинается с кода
            return task                        # нашли — возвращаем карточку
    return None                                # такой карточки на доске нет


def column_id(board_id, title):               # ID колонки по названию в пределах доски
    query = urllib.parse.urlencode({"boardId": board_id, "title": title})  # фильтр доска + имя
    found = call("GET", f"/columns?{query}").get("content", [])  # список подходящих колонок
    if not found:                              # колонки с таким названием нет
        raise RuntimeError(f"На доске нет колонки «{title}»")  # понятная ошибка в логе Actions
    return found[0]["id"]                      # первая подходящая колонка


def move(task_id, board_id, column_title):    # перенести карточку в колонку
    call("PUT", f"/tasks/{task_id}", {"columnId": column_id(board_id, column_title)})  # меняем только колонку


def post(task_id, text):                      # сообщение в чат задачи: ID чата = ID задачи
    call("POST", f"/chats/{task_id}/messages", {"text": text, "textHtml": "", "label": ""})  # три обязательных поля


def upload(path):                             # загрузить файл и вернуть его адрес в YouGile
    boundary = uuid.uuid4().hex               # уникальная граница частей multipart
    name = os.path.basename(path)             # имя файла, которое увидит получатель
    with open(path, "rb") as f:               # читаем файл в двоичном режиме
        payload = f.read()                    # содержимое файла
    head = (f"--{boundary}\r\n"               # начало части
            f'Content-Disposition: form-data; name="file"; filename="{name}"\r\n'  # поле file по спецификации
            "Content-Type: application/octet-stream\r\n\r\n").encode("utf-8")  # тип содержимого части
    tail = f"\r\n--{boundary}--\r\n".encode("utf-8")  # закрывающая граница
    req = urllib.request.Request(BASE + "/upload-file", data=head + payload + tail, method="POST")  # запрос загрузки
    req.add_header("Authorization", f"Bearer {KEY}")  # тот же ключ API
    req.add_header("Content-Type", f"multipart/form-data; boundary={boundary}")  # тип тела с границей
    with urllib.request.urlopen(req, timeout=60) as resp:  # отправляем файл
        info = json.loads(resp.read())        # ответ: result, url, fullUrl
    time.sleep(PAUSE)                         # лимит частоты
    return info["url"]                        # относительный адрес вида /user-data/…/файл


def post_file(task_id, path, caption):        # подпись и файл одним сообщением
    url = upload(path)                        # сначала загружаем файл
    post(task_id, f"{caption}\n/root/#file:{url}")  # формат вложения — как в открытом MCP-сервере YouGile
