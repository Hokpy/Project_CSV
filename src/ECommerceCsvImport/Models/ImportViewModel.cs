namespace ECommerceCsvImport.Models;

public class ImportViewModel
{
    public string FileName { get; set; } = string.Empty;
    public ImportResult? Result { get; set; }
}
