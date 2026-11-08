namespace ECommerceCsvImport.Services;

public static class CsvLineParser
{
    public static IReadOnlyList<string> Parse(string line)
    {
        if (string.IsNullOrEmpty(line))
            return Array.Empty<string>();

        var fields = new List<string>();
        var currentField = new System.Text.StringBuilder();
        bool inQuotes = false;
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString().Trim());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }
        
        fields.Add(currentField.ToString().Trim());
        return fields;
    }
}
