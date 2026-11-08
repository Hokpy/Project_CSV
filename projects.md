# Задачи производственной практики ПП.02.01

## Основная задача
Разработать ASP.NET MVC-модуль импорта товаров электронного магазина из CSV-файлов с валидацией данных и хранением в памяти приложения.

## Вариант задания
**Вариант 1. Электроника**

## Список задач

### 1. Создание структуры проекта
- [x] Создать ASP.NET Core MVC проект `ECommerceCsvImport`
- [x] Создать xUnit тестовый проект `ECommerceCsvImport.Tests`
- [x] Настроить Git репозиторий
- [x] Создать структуру папок (Models, Services, Controllers, Views)

### 2. Реализация моделей данных
- [x] Создать модель `Product` с полями для электроники:
  - sku, name, description, price, currency, category_name
  - stock, image_url, is_active
  - brand, warranty_months, model_number (специфичные для электроники)
- [x] Создать `ImportResult` для результатов импорта
- [x] Создать `ImportError` для описания ошибок
- [x] Создать view-модели: `ImportViewModel`, `ProductsIndexViewModel`

### 3. Реализация репозитория в памяти
- [x] Создать интерфейс `IProductRepository`
- [x] Реализовать `InMemoryProductRepository` с методами:
  - Add(Product product)
  - GetAll()
  - GetBySku(string sku)
  - Count
  - Clear()
- [x] Обеспечить thread-safety (singleton в ASP.NET Core)
- [x] Написать unit-тесты для репозитория

### 4. Реализация парсера CSV
- [x] Создать `CsvLineParser` с поддержкой:
  - Разделения полей по запятым
  - Пустых полей
  - Кавычек (запятые внутри кавычек)
  - Экранирования двойных кавычек
  - Trim значений
- [x] Написать unit-тесты для парсера

### 5. Реализация сервиса импорта
- [x] Создать интерфейс `IProductImportService`
- [x] Реализовать `ProductImportService` с методами:
  - ImportFromCsv(string filePath)
  - ImportFromLines(IEnumerable<string> lines)
- [x] Реализовать валидацию полей:
  - sku: обязательное, не пустое
  - name: обязательное, не пустое
  - price: число > 0
  - stock: целое число >= 0
  - is_active: true/false/1/0
  - brand: обязательное для электроники
  - warranty_months: целое 0-60
  - currency: по умолчанию "RUB"
  - category_name: по умолчанию "Без категории"
- [x] Обработать дубликаты SKU
- [x] Обработать ошибки чтения файла
- [x] Написать 23+ unit-тестов для сервиса импорта

### 6. Реализация контроллера и представлений
- [x] Создать `ProductsController` с действиями:
  - Index() - список товаров
  - Import() GET - форма импорта
  - Import(string filePath) POST - запуск импорта
- [x] Реализовать `ResolveFilePath` для поддержки относительных и абсолютных путей
- [x] Создать `Views/Products/Index.cshtml` - таблица товаров
- [x] Создать `Views/Products/Import.cshtml` - форма импорта и результаты

### 7. Подготовка тестовых данных
- [x] Создать `wwwroot/csv/sample_products.csv` (11 товаров)
- [x] Создать тестовые CSV файлы:
  - valid_electronics.csv
  - invalid_price.csv
  - duplicate_sku.csv
  - missing_brand.csv
  - warranty_out_of_range.csv
  - bad_active.csv
  - empty_file.csv

### 8. Настройка Dependency Injection
- [x] Зарегистрировать сервисы как singleton в Program.cs:
  - IProductRepository → InMemoryProductRepository
  - IProductImportService → ProductImportService

### 9. Тестирование
- [x] Выполнить все unit-тесты (репозиторий, парсер, сервис)
- [x] Провести ручное тестирование UI:
  - [x] Успешный импорт (`sample_products.csv`, товары отображаются в списке)
  - [ ] Неверная цена
  - [ ] Дубликаты SKU
  - [ ] Пустой файл
  - [ ] Неверное значение is_active

### 10. Оформление отчета
- [x] Подготовить отчет по практике с разделами:
  - Титульный лист
  - Цель и задачи
  - Описание задания
  - Архитектура решения
  - Формат CSV-файла
  - Реализация функциональности
  - Результаты тестирования
  - Выводы
  - Приложения (CSV, скриншоты, фрагменты кода, Git история)

## Технологический стек
- ASP.NET Core MVC
- C# (.NET 6+)
- xUnit для тестирования
- Git для контроля версий

## Критерии приемки
- ✅ Приложение успешно импортирует корректные CSV файлы
- ✅ Валидация всех обязательных полей работает
- ✅ Обработка ошибок не приводит к падению приложения
- ✅ Дубликаты SKU обрабатываются корректно
- ✅ Все unit-тесты проходят
- ✅ Код размещен в Git с осмысленными коммитами
- ✅ Отчет по практике оформлен
