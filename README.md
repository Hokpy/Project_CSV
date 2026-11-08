# ECommerceCsvImport

Учебный проект производственной практики **ПП.02.01** — ASP.NET Core MVC-приложение для импорта товаров интернет-магазина из CSV-файлов.

## Вариант 1 — Электроника

Дополнительные поля для варианта «Электроника»:

| Поле | Описание | Ограничения |
|------|----------|-------------|
| `brand` | Бренд производителя | Обязательно |
| `warranty_months` | Срок гарантии (мес.) | 0–60 |
| `model_number` | Номер модели | Может быть пустым |

## Структура проекта

```
work_ber/
├── src/
│   └── ECommerceCsvImport/          # ASP.NET Core MVC приложение
│       ├── Controllers/
│       │   └── ProductsController.cs
│       ├── Models/
│       │   ├── Product.cs
│       │   ├── ImportResult.cs
│       │   ├── ImportError.cs
│       │   ├── ImportViewModel.cs
│       │   └── ProductsIndexViewModel.cs
│       ├── Services/
│       │   ├── IProductRepository.cs
│       │   ├── InMemoryProductRepository.cs
│       │   ├── CsvLineParser.cs
│       │   ├── IProductImportService.cs
│       │   └── ProductImportService.cs
│       ├── Views/
│       │   └── Products/
│       │       ├── Index.cshtml
│       │       └── Import.cshtml
│       └── wwwroot/csv/
│           └── sample_products.csv  # Пример CSV с 11 товарами
├── tests/
│   └── ECommerceCsvImport.Tests/    # xUnit тесты
│       ├── testdata/                # Тестовые CSV-файлы
│       ├── RepositoryTests.cs
│       ├── CsvLineParserTests.cs
│       └── ProductImportServiceTests.cs
├── docs/
│   └── practice_report_draft.md    # Черновик отчёта по практике
└── README.md
```

## Формат CSV

Обязательный заголовок:

```
sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number
```

Пример строки:

```
SKU001,Samsung Galaxy S24,Флагманский смартфон,89990,RUB,Смартфоны,50,/images/s24.jpg,true,Samsung,24,SM-S921B
```

## Правила валидации

- `sku` — обязателен, уникален в рамках импорта
- `name` — обязателен
- `price` — число > 0
- `currency` — `RUB`, `USD` или `EUR`
- `stock` — целое число ≥ 0
- `is_active` — `true` или `false`
- `brand` — обязателен
- `warranty_months` — целое число от 0 до 60
- Строки с любой ошибкой пропускаются целиком

## Запуск

### Требования

- .NET 10 SDK

### Команды

```bash
# Сборка
dotnet build

# Запуск веб-приложения
dotnet run --project src/ECommerceCsvImport

# Запуск тестов
dotnet test
```

По умолчанию приложение запустится на `https://localhost:5001` (или `http://localhost:5000`).

## Использование

1. Откройте `http://localhost:5000/Products/Import` в браузере
2. Укажите путь к CSV-файлу (абсолютный или относительный к папке `wwwroot/csv/`)
3. Нажмите «Импортировать»
4. Просмотрите результаты на странице результатов
5. Перейдите в «Товары» для просмотра импортированных записей

### Быстрый тест

В папке `wwwroot/csv/` уже есть `sample_products.csv` с 11 корректными товарами-электроникой. Введите путь `sample_products.csv` в форму импорта.

## Тесты

Проект покрыт 31 unit-тестом:

| Файл | Тестов | Что проверяется |
|------|--------|-----------------|
| `RepositoryTests.cs` | 4 | Добавление, хранение, поиск по SKU, дубликаты |
| `CsvLineParserTests.cs` | 4 | Парсинг строк, кавычки, пустые поля |
| `ProductImportServiceTests.cs` | 23 | Все сценарии валидации по спецификации |

```bash
dotnet test --verbosity normal
```
