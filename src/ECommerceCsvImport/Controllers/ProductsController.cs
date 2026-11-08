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
        if (Path.IsPathRooted(trimmed))
            return trimmed;

        if (trimmed.StartsWith("/"))
            trimmed = trimmed.TrimStart('/');

        return Path.Combine(_environment.WebRootPath, "csv", trimmed);
    }
}
