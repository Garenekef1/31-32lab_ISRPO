# Лабораторная работа №31-32

Введение в SQLite и Entity Framework Core.

**ФИО:** Назаренко Алексей  
**Группа:** ИСП-233  
**Дата:** 08.05.2026

## Краткое описание

В работе создан Web API проект `TaskDb`. Данные задач хранятся не в списке в памяти, а в SQLite базе `taskdb.db`. Для работы с базой используется Entity Framework Core, миграции, LINQ-запросы и асинхронные методы.

## Полезные команды EF Core

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations list
dotnet ef migrations script
dotnet ef migrations remove
```

## Структура проекта

```text
Lab31-32_EFCore
├── img
├── TaskDb
│   ├── Controllers
│   ├── Data
│   ├── Migrations
│   ├── Models
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── taskdb.db
└── README.md
```

## Маршруты

| Метод | URL | Описание |
| --- | --- | --- |
| GET | `/api/tasks` | Все задачи |
| GET | `/api/tasks?completed=true` | Фильтр по статусу |
| GET | `/api/tasks/{id}` | Задача по id |
| POST | `/api/tasks` | Создать задачу |
| PUT | `/api/tasks/{id}` | Обновить задачу |
| PATCH | `/api/tasks/{id}/complete` | Переключить статус |
| DELETE | `/api/tasks/{id}` | Удалить задачу |
| GET | `/api/tasks/search` | Поиск и фильтрация |
| GET | `/api/tasks/stats` | Статистика |
| GET | `/api/tasks/paged` | Пагинация |
| GET | `/api/tasks/overdue` | Просроченные задачи |
| PATCH | `/api/tasks/complete-all` | Завершить все невыполненные задачи |
| DELETE | `/api/tasks/completed` | Удалить выполненные задачи |

## Миграции

| Миграция | Что делает |
| --- | --- |
| `InitialCreate` | Создаёт таблицу `Tasks` и добавляет начальные данные |
| `AddDueDateToTask` | Добавляет поле `DueDate` для срока выполнения |

## LINQ vs SQL

| LINQ | SQL |
| --- | --- |
| `.Where(t => !t.IsCompleted)` | `WHERE IsCompleted = 0` |
| `.OrderByDescending(t => t.CreatedAt)` | `ORDER BY CreatedAt DESC` |
| `.Skip(10).Take(5)` | `LIMIT 5 OFFSET 10` |
| `.CountAsync()` | `SELECT COUNT(*)` |
| `.GroupBy(t => t.Priority)` | `GROUP BY Priority` |

## Сравнение хранения данных

| Концепция | Хранение в памяти | EF Core + SQLite |
| --- | --- | --- |
| Хранение данных | `static List<T>` в RAM | Файл `.db` на диске |
| После перезапуска | Данные пропадают | Данные сохраняются |
| Поиск по условию | LINQ to Objects | LINQ to Entities → SQL |
| Создание структуры | Не нужно | Миграции `dotnet ef` |
| Начальные данные | Хардкод в коде | `HasData()` в миграции |
| Получение данных | `list.FirstOrDefault(...)` | `await db.Table.FindAsync(id)` |
| Добавление | `list.Add(item)` | `db.Table.Add(item)` + `SaveChangesAsync()` |
| Удаление | `list.Remove(item)` | `db.Table.Remove(item)` + `SaveChangesAsync()` |
| Масштабируемость | Ограничена RAM | Гигабайты данных |
| Транзакции | Нет | Встроены в EF Core |

## Главные выводы

1. EF Core переводит LINQ-запросы из C# в SQL-запросы для базы данных.
2. SQLite удобен для учебных проектов, потому что база хранится в одном `.db` файле.
3. Миграции позволяют хранить историю изменений структуры базы данных.
4. `SaveChangesAsync()` фиксирует изменения, которые до этого были только в памяти контекста.
5. `async/await` нужен для работы с базой, чтобы сервер не блокировал поток во время ожидания.
