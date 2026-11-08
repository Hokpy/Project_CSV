namespace ECommerceCsvImport.Services;

using ECommerceCsvImport.Models;

public interface IProductImportService
{
    ImportResult ImportFromCsv(string filePath);
    ImportResult ImportFromLines(IEnumerable<string> lines);
}
