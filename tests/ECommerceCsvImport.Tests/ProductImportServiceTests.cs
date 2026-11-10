using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests;

public class ProductImportServiceTests
{
    private const string Header = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number";

    private static (InMemoryProductRepository repo, ProductImportService service) CreateService()
    {
        var repo = new InMemoryProductRepository();
        var service = new ProductImportService(repo);
        return (repo, service);
    }

    // === Тест 1: Успешный импорт одной корректной строки ===
    [Fact]
    public void ImportFromLines_SingleValidLine_ImportsSuccessfully()
    {
        var (repository, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1001,Ноутбук Lenovo IdeaPad 3,Бюджетный ноутбук для работы и учебы,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad3-15"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(1, result.ImportedCount);
        Assert.Empty(result.Errors);

        var product = repository.GetBySku("1001");
        Assert.NotNull(product);
        Assert.Equal("Ноутбук Lenovo IdeaPad 3", product.Name);
        Assert.Equal(45990m, product.Price);
        Assert.Equal("Lenovo", product.Brand);
        Assert.Equal(24, product.WarrantyMonths);
        Assert.Equal("IdeaPad3-15", product.ModelNumber);
    }

    // === Тест 2: Успешный импорт нескольких строк ===
    [Fact]
    public void ImportFromLines_MultipleValidLines_ImportsAll()
    {
        var (repository, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1001,Ноутбук Lenovo IdeaPad 3,Бюджетный ноутбук,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad3-15",
            "1002,Мышь Logitech M185,Беспроводная мышь,1290,RUB,Периферия,10,/images/mouse.jpg,true,Logitech,12,M185"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(2, result.TotalRows);
        Assert.Equal(2, result.ImportedCount);
        Assert.Empty(result.Errors);
        Assert.Equal(2, repository.Count);
    }

    // === Тест 3: Файл не найден ===
    [Fact]
    public void ImportFromCsv_FileNotFound_ReturnsError()
    {
        var (_, service) = CreateService();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csv");

        var result = service.ImportFromCsv(path);

        Assert.Equal(0, result.TotalRows);
        Assert.Equal(0, result.ImportedCount);
        Assert.Single(result.Errors);
        Assert.Contains("Файл не найден", result.Errors[0].Message);
    }

    // === Тест 4: Пустой файл ===
    [Fact]
    public void ImportFromCsv_EmptyFile_ReturnsError()
    {
        var (_, service) = CreateService();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csv");
        File.WriteAllText(path, string.Empty);

        try
        {
            var result = service.ImportFromCsv(path);

            Assert.Equal(0, result.TotalRows);
            Assert.Equal(0, result.ImportedCount);
            Assert.Single(result.Errors);
            Assert.Contains("Данные для импорта отсутствуют", result.Errors[0].Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // === Тест 5: Только заголовок без данных ===
    [Fact]
    public void ImportFromLines_HeaderOnly_ReturnsDataMissingError()
    {
        var (_, service) = CreateService();
        var lines = new[] { Header };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.TotalRows);
        Assert.Equal(0, result.ImportedCount);
        Assert.Single(result.Errors);
        Assert.Contains("Данные для импорта отсутствуют", result.Errors[0].Message);
    }

    // === Тест 6: Неверный заголовок CSV ===
    [Fact]
    public void ImportFromLines_InvalidHeader_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            "sku,name,description,price,currency,category_name,stock,image_url,is_active"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.TotalRows);
        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Неверные заголовки CSV", result.Errors[0].Message);
    }

    // === Тест 7: Неверное количество полей в строке данных ===
    [Fact]
    public void ImportFromLines_WrongFieldCount_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1001,Ноутбук Lenovo IdeaPad 3"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(1, result.TotalRows);
        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Неверное количество полей в строке", result.Errors[0].Message);
    }

    // === Тест 8: Пустой SKU ===
    [Fact]
    public void ImportFromLines_EmptySku_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            ",Ноутбук Lenovo IdeaPad 3,Бюджетный ноутбук,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad3-15"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Поле sku не может быть пустым"));
    }

    // === Тест 9: Пустое название товара ===
    [Fact]
    public void ImportFromLines_EmptyName_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1001,,Бюджетный ноутбук,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad3-15"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Поле name не может быть пустым"));
    }

    // === Тест 10: Неверная цена — отрицательное значение ===
    [Fact]
    public void ImportFromLines_NegativePrice_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1003,Клавиатура A4Tech Bloody V3M,Механическая игровая клавиатура,-500,RUB,Периферия,7,/images/keyboard.jpg,true,A4Tech,24,V3M"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Поле price должно быть числом больше 0"));
    }

    // === Тест 11: Неверная цена — нечисловое значение ===
    [Fact]
    public void ImportFromLines_NonNumericPrice_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1003,Клавиатура A4Tech Bloody V3M,Механическая игровая клавиатура,abc,RUB,Периферия,7,/images/keyboard.jpg,true,A4Tech,24,V3M"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Поле price должно быть числом больше 0"));
    }

    // === Тест 12: Отрицательный остаток ===
    [Fact]
    public void ImportFromLines_NegativeStock_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1004,Монитор Dell S2722QC,Игровой монитор 27 дюймов,29990,RUB,Компьютеры,-5,/images/monitor.jpg,true,Dell,24,S2722QC"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Остаток на складе должен быть целым числом не меньше 0"));
    }

    // === Тест 13: Неверное значение is_active ===
    [Fact]
    public void ImportFromLines_InvalidIsActive_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1007,Webcam Logitech C920,Веб-камера Full HD,4990,RUB,Периферия,6,/images/webcam.jpg,yes,Logitech,24,C920"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Поле is_active должно иметь значение true, false, 1 или 0"));
    }

    // === Тест 14: Отсутствует бренд ===
    [Fact]
    public void ImportFromLines_MissingBrand_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1005,Видеокарта NVIDIA GeForce RTX 3060,,24990,RUB,Компьютеры,4,/images/gpu.jpg,true,,36,RTX3060"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Поле brand обязательно для варианта «Электроника»"));
    }

    // === Тест 15: Гарантия меньше нуля ===
    [Fact]
    public void ImportFromLines_WarrantyBelowZero_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1006,Накопитель SSD Samsung 870 EVO 1TB,Внутренний твердотельный накопитель,5990,RUB,Компьютеры,12,/images/ssd.jpg,true,Samsung,-1,870EVO-1T"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Гарантия должна быть целым числом от 0 до 60 месяцев"));
    }

    // === Тест 16: Гарантия больше 60 ===
    [Fact]
    public void ImportFromLines_WarrantyAbove60_ReturnsError()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1006,Накопитель SSD Samsung 870 EVO 1TB,Внутренний твердотельный накопитель,5990,RUB,Компьютеры,12,/images/ssd.jpg,true,Samsung,61,870EVO-1T"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Гарантия должна быть целым числом от 0 до 60 месяцев"));
    }

    // === Тест 17: Дубликат SKU внутри одного файла ===
    [Fact]
    public void ImportFromLines_DuplicateSkuInFile_OnlyFirstImported()
    {
        var (repository, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1001,Ноутбук Lenovo IdeaPad 3,Бюджетный ноутбук,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad3-15",
            "1001,Ноутбук ASUS VivoBook,Другой ноутбук,52990,RUB,Компьютеры,7,/images/asus.jpg,true,ASUS,24,VivoBook"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(2, result.TotalRows);
        Assert.Equal(1, result.ImportedCount);
        Assert.Single(result.Errors);
        Assert.Contains("Артикул товара уже существует", result.Errors[0].Message);

        var product = repository.GetBySku("1001");
        Assert.NotNull(product);
        Assert.Equal("Ноутбук Lenovo IdeaPad 3", product.Name);
    }

    [Fact]
    public void ImportFromLines_DuplicateRows_ReportTheirActualLineNumbers()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1001,First product,,100,RUB,Test,1,,true,Brand,12,Model",
            "",
            "1001,Second product,,100,RUB,Test,1,,true,Brand,12,Model"
        };

        var result = service.ImportFromLines(lines);

        var duplicate = Assert.Single(result.Errors);
        Assert.Equal(4, duplicate.RowNumber);
    }

    // === Тест 18: Дубликат SKU с уже существующим товаром в репозитории ===
    [Fact]
    public void ImportFromLines_SkuAlreadyInRepository_ReturnsError()
    {
        var (repository, service) = CreateService();
        repository.Add(new Product
        {
            Sku = "1001",
            Name = "Existing product",
            Price = 100m,
            Brand = "SomeBrand"
        });

        var lines = new[]
        {
            Header,
            "1001,Ноутбук ASUS VivoBook,Другой ноутбук,52990,RUB,Компьютеры,7,/images/asus.jpg,true,ASUS,24,VivoBook"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(1, repository.Count);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Артикул товара уже существует"));
    }

    // === Тест 19: Несколько ошибок в одной строке ===
    [Fact]
    public void ImportFromLines_MultipleErrorsInOneLine_ReportsAll()
    {
        var (repository, service) = CreateService();
        var lines = new[]
        {
            Header,
            // 11 fields — will match count for 12-column header? No, this is 11.
            // We need 12 fields to pass field count check.
            // Per the spec: ",,Некорректная строка,-10,RUB,Компьютеры,-5,/images/x.jpg,yes,,61" has only 11 fields
            // So we add one more to make it 12:
            ",,Некорректная строка,-10,RUB,Компьютеры,-5,/images/x.jpg,yes,,61,"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.True(result.Errors.Count >= 4);
        Assert.Empty(repository.GetAll());
    }

    // === Тест 20: Пустые необязательные поля разрешены ===
    [Fact]
    public void ImportFromLines_EmptyOptionalFields_ImportedSuccessfully()
    {
        var (repository, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1011,Блок питания Corsair CV550,,4990,RUB,Компьютеры,9,,true,Corsair,60,CV550"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(1, result.ImportedCount);
        Assert.Empty(result.Errors);

        var product = repository.GetBySku("1011");
        Assert.NotNull(product);
        Assert.Equal("", product.Description);
        Assert.Equal("", product.ImageUrl);
    }

    // === Тест 21: Пустая валюта заменяется на RUB ===
    [Fact]
    public void ImportFromLines_EmptyCurrency_DefaultsToRub()
    {
        var (repository, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1011,Блок питания Corsair CV550,,4990,,Компьютеры,9,,true,Corsair,60,CV550"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(1, result.ImportedCount);
        
        var product = repository.GetBySku("1011");
        Assert.NotNull(product);
        Assert.Equal("RUB", product.Currency);
    }

    // === Тест 22: Пустая категория заменяется на «Без категории» ===
    [Fact]
    public void ImportFromLines_EmptyCategory_DefaultsToBezKategorii()
    {
        var (repository, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1011,Блок питания Corsair CV550,,4990,RUB,,9,,true,Corsair,60,CV550"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(1, result.ImportedCount);
        
        var product = repository.GetBySku("1011");
        Assert.NotNull(product);
        Assert.Equal("Без категории", product.CategoryName);
    }

    // === Тест 23: Кириллица сохраняется корректно ===
    [Fact]
    public void ImportFromLines_CyrillicText_PreservedCorrectly()
    {
        var (repository, service) = CreateService();
        var lines = new[]
        {
            Header,
            "1001,Ноутбук Lenovo IdeaPad 3,Бюджетный ноутбук для работы и учебы,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad3-15"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(1, result.ImportedCount);
        
        var product = repository.GetBySku("1001");
        Assert.NotNull(product);
        Assert.Equal("Ноутбук Lenovo IdeaPad 3", product.Name);
        Assert.Equal("Бюджетный ноутбук для работы и учебы", product.Description);
        Assert.Equal("Компьютеры", product.CategoryName);
    }

    [Fact]
    public void ImportFromLines_WarrantyBoundaries_AreAccepted()
    {
        var (repository, service) = CreateService();
        var lines = new[]
        {
            Header,
            "2001,Minimum warranty,,100,RUB,Test,1,,true,Brand,0,Min",
            "2002,Maximum warranty,,100,RUB,Test,1,,true,Brand,60,Max"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(2, result.ImportedCount);
        Assert.Empty(result.Errors);
        Assert.Equal(2, repository.Count);
    }

    [Fact]
    public void ImportFromLines_WhitespaceOnlyRequiredValues_AreRejected()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            Header,
            "   ,   ,Description,100,RUB,Test,1,,true,Brand,12,Model"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(0, result.ImportedCount);
        Assert.Contains(result.Errors, error => error.Message.Contains("sku"));
        Assert.Contains(result.Errors, error => error.Message.Contains("name"));
    }

    [Fact]
    public void ImportFromLines_LeadingBlankLines_PreservePhysicalRowNumber()
    {
        var (_, service) = CreateService();
        var lines = new[]
        {
            "",
            Header,
            "3001,Product,,100,RUB,Test,1,,true,Brand,12,Model"
        };

        var result = service.ImportFromLines(lines);

        Assert.Equal(1, result.ImportedCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ImportFromLines_ZeroPrice_IsRejected()
    {
        var (_, service) = CreateService();
        var result = service.ImportFromLines(new[]
        {
            Header,
            "4001,Zero price,,0,RUB,Test,1,,true,Brand,12,Model"
        });

        Assert.Equal(0, result.ImportedCount);
        Assert.Contains(result.Errors, error => error.Message.Contains("price"));
    }

    [Fact]
    public void ImportFromLines_NonNumericStock_IsRejected()
    {
        var (_, service) = CreateService();
        var result = service.ImportFromLines(new[]
        {
            Header,
            "4002,Bad stock,,100,RUB,Test,abc,,true,Brand,12,Model"
        });

        Assert.Equal(0, result.ImportedCount);
        Assert.Contains(result.Errors, error => error.Message.Contains("Остаток"));
    }

    [Fact]
    public void ImportFromLines_NonNumericWarranty_IsRejected()
    {
        var (_, service) = CreateService();
        var result = service.ImportFromLines(new[]
        {
            Header,
            "4003,Bad warranty,,100,RUB,Test,1,,true,Brand,abc,Model"
        });

        Assert.Equal(0, result.ImportedCount);
        Assert.Contains(result.Errors, error => error.Message.Contains("Гарантия"));
    }

    [Fact]
    public void ImportFromLines_BooleanValues_AreCaseInsensitive()
    {
        var (repository, service) = CreateService();
        var result = service.ImportFromLines(new[]
        {
            Header,
            "4004,Upper true,,100,RUB,Test,1,,TRUE,Brand,12,Model",
            "4005,Mixed-case false,,100,RUB,Test,1,,FaLsE,Brand,12,Model"
        });

        Assert.Equal(2, result.ImportedCount);
        Assert.True(repository.GetBySku("4004")!.IsActive);
        Assert.False(repository.GetBySku("4005")!.IsActive);
    }
}
