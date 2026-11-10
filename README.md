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
├── ECommerceCsvImport.sln             # Solution для Visual Studio
├── ECommerceCsvImport.slnx            # Solution в формате .slnx
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
├── final_documents/                 # Подготовленные материалы практики
│   ├── Отчет_по_практике_ПП0201_черновик.docx
│   ├── Дневник_практики_ПП0201_черновик.docx
│   ├── Аттестационный_лист_ПП0201_черновик.docx
│   ├── Презентация_ПП0201.pptx
│   └── Доклад_и_вопросы_к_защите.md
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
- `currency` — любое непустое значение; по умолчанию `RUB`
- `stock` — целое число ≥ 0
- `is_active` — `true`, `false`, `1` или `0`
- `brand` — обязателен
- `warranty_months` — целое число от 0 до 60
- Строки с любой ошибкой пропускаются целиком

## Запуск

### Требования

- .NET 10 SDK
- Visual Studio с поддержкой .NET 10 для запуска на Windows

### Visual Studio (Windows)

1. Откройте `ECommerceCsvImport.sln` в Visual Studio 2022
2. Нажмите F5 или выберите профиль запуска (IIS Express / https / http)
3. Браузер откроется на странице `/Products`

### Командная строка

```bash
# Сборка
dotnet build ECommerceCsvImport.sln

# Запуск веб-приложения
dotnet run --project src/ECommerceCsvImport

# Запуск тестов
dotnet test ECommerceCsvImport.sln
```

По умолчанию приложение запустится на `https://localhost:7003` (или `http://localhost:5245`).

## Использование

1. Откройте `http://localhost:5245/Products/Import` в браузере
2. Укажите путь к CSV-файлу:
   - Веб-путь: `/csv/sample_products.csv` (ищется в `wwwroot/csv/`)
   - Только имя файла: `sample_products.csv` (также ищется в `wwwroot/csv/`)
   - Полный путь ОС: `C:\data\products.csv` или `/home/user/products.csv`
3. Нажмите «Импортировать»
4. Просмотрите результаты на странице результатов
5. Перейдите в «Товары» для просмотра импортированных записей

### Быстрый тест

В папке `wwwroot/csv/` уже есть `sample_products.csv` с 11 корректными товарами-электроникой.
Введите путь `/csv/sample_products.csv` в форму импорта.

## Тесты

Проект покрыт **32 unit-тестами**:

| Файл | Тестов | Что проверяется |
|------|--------|-----------------|
| `RepositoryTests.cs` | 4 | Добавление, хранение, поиск по SKU, дубликаты |
| `CsvLineParserTests.cs` | 4 | Парсинг строк, кавычки, пустые поля |
| `ProductImportServiceTests.cs` | 24 | Все сценарии валидации по спецификации + регрессия |

```bash
dotnet test ECommerceCsvImport.sln --verbosity normal
```
