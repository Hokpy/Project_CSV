using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests;

public class RepositoryTests
{
    [Fact]
    public void Add_AddsProduct_GetAllContainsIt()
    {
        var repository = new InMemoryProductRepository();
        var product = new Product
        {
            Sku = "1001",
            Name = "Ноутбук Lenovo IdeaPad 3",
            Price = 45990m,
            Brand = "Lenovo"
        };

        repository.Add(product);

        var all = repository.GetAll();
        Assert.Single(all);
        Assert.Equal("1001", all[0].Sku);
    }

    [Fact]
    public void GetBySku_FindsExistingProduct()
    {
        var repository = new InMemoryProductRepository();
        var product = new Product
        {
            Sku = "1001",
            Name = "Ноутбук Lenovo IdeaPad 3",
            Price = 45990m,
            Brand = "Lenovo"
        };

        repository.Add(product);

        var found = repository.GetBySku("1001");
        
        Assert.NotNull(found);
        Assert.Equal("1001", found.Sku);
        Assert.Equal("Ноутбук Lenovo IdeaPad 3", found.Name);
    }

    [Fact]
    public void GetBySku_IgnoresCaseAndWhitespace()
    {
        var repository = new InMemoryProductRepository();
        var product = new Product
        {
            Sku = "1001",
            Name = "Ноутбук Lenovo IdeaPad 3",
            Price = 45990m,
            Brand = "Lenovo"
        };

        repository.Add(product);

        var found1 = repository.GetBySku(" 1001 ");
        var found2 = repository.GetBySku("1001");
        
        Assert.NotNull(found1);
        Assert.NotNull(found2);
        Assert.Equal("1001", found1.Sku);
        Assert.Equal("1001", found2.Sku);
    }

    [Fact]
    public void EmptyRepository_HasZeroCount()
    {
        var repository = new InMemoryProductRepository();

        Assert.Equal(0, repository.Count);
        Assert.Empty(repository.GetAll());
        Assert.Null(repository.GetBySku("1001"));
    }
}
