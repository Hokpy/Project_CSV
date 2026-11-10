using System.Text;
using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCsvImport.Controllers;

public class ProductsController : Controller
{
    private const long MaxCsvFileSize = 5 * 1024 * 1024;

    private readonly IProductRepository _repository;
    private readonly IProductImportService _importService;

    public ProductsController(
        IProductRepository repository,
        IProductImportService importService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
    }

    [HttpGet]
    public ActionResult Index()
    {
        var viewModel = new ProductsIndexViewModel
        {
            Products = _repository.GetAll()
        };
        return View(viewModel);
    }

    [HttpGet]
    public ActionResult Import()
    {
        return View(new ImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Import(IFormFile? csvFile)
    {
        if (csvFile is null || csvFile.Length == 0)
        {
            return ImportViewWithError("Выберите CSV-файл");
        }

        var fileName = Path.GetFileName(csvFile.FileName);
        if (!Path.GetExtension(fileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return ImportViewWithError("Разрешены только файлы с расширением .csv", fileName);
        }

        if (csvFile.Length > MaxCsvFileSize)
        {
            return ImportViewWithError("Размер CSV-файла не должен превышать 5 МБ", fileName);
        }

        ImportResult result;
        try
        {
            using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true);

            var lines = new List<string>();
            while (await reader.ReadLineAsync() is { } line)
            {
                lines.Add(line);
            }

            result = _importService.ImportFromLines(lines);
        }
        catch (IOException)
        {
            result = CreateFileError("Не удалось прочитать CSV-файл");
        }

        return View("Import", new ImportViewModel
        {
            FileName = fileName,
            Result = result
        });
    }

    private ViewResult ImportViewWithError(string message, string fileName = "")
    {
        return View("Import", new ImportViewModel
        {
            FileName = fileName,
            Result = CreateFileError(message)
        });
    }

    private static ImportResult CreateFileError(string message)
    {
        return new ImportResult
        {
            Errors =
            {
                new ImportError { RowNumber = 0, Message = message }
            }
        };
    }
}
