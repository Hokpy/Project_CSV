namespace ECommerceCsvImport.Services;

using ECommerceCsvImport.Models;
using System.Globalization;

public class ProductImportService : IProductImportService
{
    private readonly IProductRepository _repository;
    
    private const string DefaultCurrency = "RUB";
    private const string DefaultCategory = "Без категории";
    private const int MinWarrantyMonths = 0;
    private const int MaxWarrantyMonths = 60;

    public ProductImportService(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ImportResult ImportFromCsv(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new ImportResult
            {
                Errors = { new ImportError { RowNumber = 0, Message = "Файл не найден" } }
            };
        }

        if (!File.Exists(filePath))
        {
            return new ImportResult
            {
                Errors = { new ImportError { RowNumber = 0, Message = "Файл не найден" } }
            };
        }

        try
        {
            var lines = File.ReadAllLines(filePath);
            return ImportFromLines(lines);
        }
        catch (IOException)
        {
            return new ImportResult
            {
                Errors = { new ImportError { RowNumber = 0, Message = "Файл не найден" } }
            };
        }
        catch (UnauthorizedAccessException)
        {
            return new ImportResult
            {
                Errors = { new ImportError { RowNumber = 0, Message = "Файл не найден" } }
            };
        }
    }

    public ImportResult ImportFromLines(IEnumerable<string> lines)
    {
        var result = new ImportResult();
        var linesList = lines.ToList();

        var nonEmptyLines = linesList.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        
        if (nonEmptyLines.Count == 0)
        {
            result.Errors.Add(new ImportError
            {
                RowNumber = 0,
                Message = "Данные для импорта отсутствуют"
            });
            return result;
        }

        var headerLine = nonEmptyLines[0];
        var headerFields = CsvLineParser.Parse(headerLine);
        
        var requiredColumns = new[] 
        { 
            "sku", "name", "description", "price", "currency", 
            "category_name", "stock", "image_url", "is_active", 
            "brand", "warranty_months", "model_number" 
        };

        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        
        for (int i = 0; i < headerFields.Count; i++)
        {
            var colName = headerFields[i].Trim();
            if (!columnMap.ContainsKey(colName))
            {
                columnMap[colName] = i;
            }
        }

        foreach (var required in requiredColumns)
        {
            if (!columnMap.ContainsKey(required))
            {
                var headerRowNumber = linesList.IndexOf(headerLine) + 1;
                result.Errors.Add(new ImportError
                {
                    RowNumber = headerRowNumber,
                    Message = "Неверные заголовки CSV"
                });
                return result;
            }
        }

        var expectedFieldCount = headerFields.Count;
        var importedSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 1; i < nonEmptyLines.Count; i++)
        {
            var line = nonEmptyLines[i];
            var physicalRowNumber = linesList.IndexOf(line) + 1;
            
            result.TotalRows++;
            
            var values = CsvLineParser.Parse(line);
            
            if (values.Count != expectedFieldCount)
            {
                result.Errors.Add(new ImportError
                {
                    RowNumber = physicalRowNumber,
                    Message = "Неверное количество полей в строке"
                });
                continue;
            }

            string GetValue(string columnName)
            {
                if (columnMap.TryGetValue(columnName, out int index) && index < values.Count)
                    return values[index];
                return string.Empty;
            }

            var rowErrors = new List<ImportError>();

            var sku = GetValue("sku").Trim();
            if (string.IsNullOrWhiteSpace(sku))
            {
                rowErrors.Add(new ImportError
                {
                    RowNumber = physicalRowNumber,
                    Message = "Поле sku не может быть пустым"
                });
            }

            var name = GetValue("name").Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                rowErrors.Add(new ImportError
                {
                    RowNumber = physicalRowNumber,
                    Message = "Поле name не может быть пустым"
                });
            }

            decimal price = 0;
            bool priceValid = TryParseDecimal(GetValue("price"), out price) && price > 0;
            if (!priceValid)
            {
                rowErrors.Add(new ImportError
                {
                    RowNumber = physicalRowNumber,
                    Message = "Поле price должно быть числом больше 0"
                });
            }

            int stock = 0;
            bool stockValid = TryParseInt(GetValue("stock"), out stock) && stock >= 0;
            if (!stockValid)
            {
                rowErrors.Add(new ImportError
                {
                    RowNumber = physicalRowNumber,
                    Message = "Остаток на складе должен быть целым числом не меньше 0"
                });
            }

            bool isActive = false;
            bool activeValid = TryParseBool(GetValue("is_active"), out isActive);
            if (!activeValid)
            {
                rowErrors.Add(new ImportError
                {
                    RowNumber = physicalRowNumber,
                    Message = "Поле is_active должно иметь значение true, false, 1 или 0"
                });
            }

            var brand = GetValue("brand").Trim();
            if (string.IsNullOrWhiteSpace(brand))
            {
                rowErrors.Add(new ImportError
                {
                    RowNumber = physicalRowNumber,
                    Message = "Поле brand обязательно для варианта «Электроника»"
                });
            }

            int warrantyMonths = 0;
            bool warrantyValid = TryParseInt(GetValue("warranty_months"), out warrantyMonths)
                                 && warrantyMonths >= MinWarrantyMonths
                                 && warrantyMonths <= MaxWarrantyMonths;
            if (!warrantyValid)
            {
                rowErrors.Add(new ImportError
                {
                    RowNumber = physicalRowNumber,
                    Message = "Гарантия должна быть целым числом от 0 до 60 месяцев"
                });
            }

            if (rowErrors.Count > 0)
            {
                result.Errors.AddRange(rowErrors);
                continue;
            }

            bool isDuplicate = _repository.GetBySku(sku) != null || importedSkus.Contains(sku);
            if (isDuplicate)
            {
                result.Errors.Add(new ImportError
                {
                    RowNumber = physicalRowNumber,
                    Message = "Артикул товара уже существует"
                });
                continue;
            }

            var product = new Product
            {
                Sku = sku,
                Name = name,
                Description = GetValue("description").Trim(),
                Price = price,
                Currency = NormalizeCurrency(GetValue("currency")),
                CategoryName = NormalizeCategory(GetValue("category_name")),
                StockQuantity = stock,
                ImageUrl = GetValue("image_url").Trim(),
                IsActive = isActive,
                Brand = brand,
                WarrantyMonths = warrantyMonths,
                ModelNumber = GetValue("model_number").Trim()
            };

            _repository.Add(product);
            importedSkus.Add(sku);
            result.ImportedCount++;
        }

        if (result.TotalRows == 0 && result.Errors.Count == 0)
        {
            result.Errors.Add(new ImportError
            {
                RowNumber = 0,
                Message = "Данные для импорта отсутствуют"
            });
        }

        return result;
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        return decimal.TryParse(value?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseInt(string value, out int result)
    {
        return int.TryParse(value?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseBool(string value, out bool result)
    {
        var trimmed = value?.Trim().ToLowerInvariant();
        
        if (trimmed == "true" || trimmed == "1")
        {
            result = true;
            return true;
        }
        
        if (trimmed == "false" || trimmed == "0")
        {
            result = false;
            return true;
        }
        
        result = false;
        return false;
    }

    private static string NormalizeCurrency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DefaultCurrency;
        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizeCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DefaultCategory;
        return value.Trim();
    }
}
