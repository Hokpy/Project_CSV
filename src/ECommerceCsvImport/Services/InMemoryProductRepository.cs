namespace ECommerceCsvImport.Services;

using ECommerceCsvImport.Models;

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private readonly object _lock = new();

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _products.Count;
            }
        }
    }

    public void Add(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        lock (_lock)
        {
            _products.Add(product);
        }
    }

    public bool TryAdd(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        lock (_lock)
        {
            if (_products.Any(existing =>
                    string.Equals(existing.Sku, product.Sku.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            _products.Add(product);
            return true;
        }
    }

    public IReadOnlyList<Product> GetAll()
    {
        lock (_lock)
        {
            return _products.ToList();
        }
    }

    public Product? GetBySku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        lock (_lock)
        {
            return _products.FirstOrDefault(p =>
                string.Equals(p.Sku, sku.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _products.Clear();
        }
    }
}
