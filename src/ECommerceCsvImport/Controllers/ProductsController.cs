using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCsvImport.Controllers;

public class ProductsController : Controller
{
    private readonly IProductRepository _repository;
    private readonly IProductImportService _importService;
    private readonly IWebHostEnvironment _environment;

    public ProductsController(
        IProductRepository repository,
        IProductImportService importService,
        IWebHostEnvironment environment)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
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
    public ActionResult Import(string filePath)
    {
        var resolvedPath = ResolveFilePath(filePath);
        if (string.IsNullOrWhiteSpace(resolvedPath))
        {
            return View("Import", new ImportViewModel
            {
                FilePath = filePath ?? string.Empty,
                Result = new ImportResult
                {
                    Errors =
                    {
                        new ImportError
                        {
                            RowNumber = 0,
                            Message = "Укажите путь к CSV-файлу"
                        }
                    }
                }
            });
        }

        var result = _importService.ImportFromCsv(resolvedPath);
        return View("Import", new ImportViewModel
        {
            FilePath = filePath ?? string.Empty,
            Result = result
        });
    }

    private string? ResolveFilePath(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var trimmed = input.Trim();

        // Handle web-style paths like /csv/file.csv or csv/file.csv
        // that refer to files inside wwwroot/csv/
        var normalized = trimmed.Replace('\\', '/');

        if (normalized.StartsWith("/csv/", StringComparison.OrdinalIgnoreCase))
        {
            var fileName = normalized["/csv/".Length..];
            return Path.Combine(_environment.WebRootPath, "csv", fileName);
        }

        if (normalized.StartsWith("csv/", StringComparison.OrdinalIgnoreCase))
        {
            var fileName = normalized["csv/".Length..];
            return Path.Combine(_environment.WebRootPath, "csv", fileName);
        }

        // Absolute OS path (e.g. C:\files\data.csv or /home/user/data.csv)
        if (Path.IsPathRooted(trimmed))
            return trimmed;

        // Relative path — resolve against wwwroot/csv/
        return Path.Combine(_environment.WebRootPath, "csv", trimmed);
    }
}
