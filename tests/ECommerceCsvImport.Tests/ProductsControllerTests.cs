using System.Text;
using ECommerceCsvImport.Controllers;
using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCsvImport.Tests;

public class ProductsControllerTests
{
    private const string Header = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number";

    [Fact]
    public async Task Import_ValidCsvFile_ImportsProducts()
    {
        var repository = new InMemoryProductRepository();
        var controller = new ProductsController(repository, new ProductImportService(repository));
        var file = CreateFile("products.csv", $"{Header}\n1001,Phone,,100,RUB,Phones,1,,true,Brand,12,Model");

        var action = await controller.Import(file);

        var view = Assert.IsType<ViewResult>(action);
        var model = Assert.IsType<ImportViewModel>(view.Model);
        Assert.Equal(1, model.Result!.ImportedCount);
        Assert.Equal("products.csv", model.FileName);
    }

    [Fact]
    public async Task Import_NonCsvFile_ReturnsValidationError()
    {
        var repository = new InMemoryProductRepository();
        var controller = new ProductsController(repository, new ProductImportService(repository));
        var file = CreateFile("products.txt", "not csv");

        var action = await controller.Import(file);

        var model = Assert.IsType<ImportViewModel>(Assert.IsType<ViewResult>(action).Model);
        Assert.Contains(".csv", model.Result!.Errors[0].Message);
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public async Task Import_FileLargerThanLimit_ReturnsValidationError()
    {
        var repository = new InMemoryProductRepository();
        var controller = new ProductsController(repository, new ProductImportService(repository));
        var content = new byte[5 * 1024 * 1024 + 1];
        var file = new FormFile(new MemoryStream(content), 0, content.Length, "csvFile", "large.csv");

        var action = await controller.Import(file);

        var model = Assert.IsType<ImportViewModel>(Assert.IsType<ViewResult>(action).Model);
        Assert.Contains("5 МБ", model.Result!.Errors[0].Message);
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public async Task Import_EmptyFile_ReturnsValidationError()
    {
        var repository = new InMemoryProductRepository();
        var controller = new ProductsController(repository, new ProductImportService(repository));
        var file = new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, "csvFile", "empty.csv");

        var action = await controller.Import(file);

        var model = Assert.IsType<ImportViewModel>(Assert.IsType<ViewResult>(action).Model);
        Assert.Contains("Выберите", model.Result!.Errors[0].Message);
    }

    [Fact]
    public async Task Import_UppercaseCsvExtension_IsAccepted()
    {
        var repository = new InMemoryProductRepository();
        var controller = new ProductsController(repository, new ProductImportService(repository));
        var file = CreateFile("products.CSV", $"{Header}\n1002,Phone,,100,RUB,Phones,1,,true,Brand,12,Model");

        var action = await controller.Import(file);

        var model = Assert.IsType<ImportViewModel>(Assert.IsType<ViewResult>(action).Model);
        Assert.Equal(1, model.Result!.ImportedCount);
    }

    private static FormFile CreateFile(string fileName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "csvFile", fileName);
    }
}
