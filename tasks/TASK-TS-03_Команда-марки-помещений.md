## TASK-TS-03 · Команда «Марки помещений» (Revit + App + UI)

**Зависит от:** TS-01, TS-02 (контракт)  **Требования ТЗ:** ТР-У.08…ТР-У.11
**Файлы ТЗ:** [`Мини-ТЗ.md`](../docs/Мини-ТЗ.md), раздел 3
**Исполнитель:** dev2 · **План:** 6 ч · **Сценарии протокола:** 11, 14
**Результат:** кнопка на ленте, которая размечает помещения активного вида.

### Что создаётся

```
src/Sandbox.Core/Marks/LevelCodes.cs                 — порядок уровня → «01» (ТР-У.10), покрыт тестами
src/Sandbox.Revit/Rooms/RoomMarker.cs                — сбор помещений и запись марок в транзакции
src/Sandbox.App/Commands/RoomMarksCommand.cs         — команда кнопки
src/Sandbox.App/SandboxApplication.cs                — вторая кнопка на панели «Учебная»
tests/Sandbox.Core.Tests/Marks/LevelCodesTests.cs
CHANGELOG.md
```

### Схема вызовов

```
Кнопка «Марки помещений»
   └─ RoomMarksCommand.Execute               (App)
        ├─ RoomMarker.Collect(doc, view)     (Revit)  помещения вида, сгруппированные по уровням
        ├─ DuplicateMarkFinder.Find(...)     (Core)   дубли среди старых «Комментариев»
        ├─ RoomMarker.Apply(doc, marks)      (Revit)  Transaction "Trn_SetRoomMarks"
        └─ new InfoWindow(...).ShowDialog()  (UI)     окно-итог ТР-У.11
```

### Контракт

```csharp
public static class LevelCodes
{
    public static string FromOrder(int order);   // 0 → «01», 1 → «02»; order < 0 или > 98 — ArgumentOutOfRangeException
}
```

### Пример записи в транзакции

```csharp
using (var t = new Transaction(doc, "Trn_SetRoomMarks"))           // одна транзакция на всю команду (ТР-У.09)
{
    t.Start();                                                      // начало изменений документа
    foreach (KeyValuePair<Room, string> pair in marks)              // помещение → новая марка
    {
        Parameter p = pair.Key.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS); // параметр «Комментарии»
        p.Set(pair.Value);                                          // записать марку
    }
    t.Commit();                                                     // зафиксировать; при исключении using откатит транзакцию
}
```

### Промт

```
#Мини-ТЗ.md #copilot-instructions.md #SandboxApplication.cs #AboutCommand.cs #MarkGenerator.cs #DuplicateMarkFinder.cs

Реализуй команду «Марки помещений» по ТР-У.08…ТР-У.11. Слои: логика порядка уровней — в Core
(LevelCodes), работа с документом — в Sandbox.Revit, окно — существующий InfoWindow.
Формат — MarkFormat.Default. Код должен собираться для Revit 2021 и 2025: числовой ElementId
только через GetValue().
```

### Готово, когда

- [ ] Кнопка «Марки помещений» на панели «Учебная» (ТР-У.08)
- [ ] Марки записаны в «Комментарии» одной транзакцией `Trn_SetRoomMarks`, помещения без уровня пропущены (ТР-У.09)
- [ ] `LevelCodes.FromOrder` покрыт тестами (ТР-У.10)
- [ ] Окно-итог показывает размеченные, пропущенные и наличие дублей (ТР-У.11)
- [ ] Проверено в Revit 2021 и 2025 на модели с двумя уровнями и неразмещённым помещением; Ctrl+Z отменяет всё одним шагом
- [ ] Строка в `CHANGELOG.md` добавлена

### Подводные камни

- Неразмещённое помещение (`Room.Area == 0`, `Location == null`) есть в модели, но не на виде — оно не должно ронять команду.
- Помещения без номера сортируйте в конец, а не по пустой строке в начало.
- На виде без помещений команда показывает итог «размечено: 0», а не ошибку.
