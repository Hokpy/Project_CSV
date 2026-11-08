using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests;

public class CsvLineParserTests
{
    [Fact]
    public void Parse_SimpleLineWithoutQuotes_ReturnsFields()
    {
        var line = "1001,Ноутбук Lenovo IdeaPad 3,Бюджетный ноутбук,45990,RUB,Компьютеры,5,/images/laptop.jpg,true,Lenovo,24,IdeaPad3-15";
        
        var fields = CsvLineParser.Parse(line);
        
        Assert.Equal(12, fields.Count);
        Assert.Equal("1001", fields[0]);
        Assert.Equal("Ноутбук Lenovo IdeaPad 3", fields[1]);
        Assert.Equal("Бюджетный ноутбук", fields[2]);
        Assert.Equal("45990", fields[3]);
    }

    [Fact]
    public void Parse_EmptyFields_HandlesCorrectly()
    {
        var line = "1011,Блок питания Corsair CV550,,4990,RUB,Компьютеры,9,,true,Corsair,60,CV550";
        
        var fields = CsvLineParser.Parse(line);
        
        Assert.Equal(12, fields.Count);
        Assert.Equal("1011", fields[0]);
        Assert.Equal("Блок питания Corsair CV550", fields[1]);
        Assert.Equal("", fields[2]); // Empty description
        Assert.Equal("", fields[7]); // Empty image_url
    }

    [Fact]
    public void Parse_FieldWithCommaInsideQuotes_ParsesCorrectly()
    {
        var line = "1012,\"Ноутбук, игровой\",Тестовая строка,59990,RUB,Компьютеры,1,/images/notebook.jpg,true,ASUS,36,VivoBook";
        
        var fields = CsvLineParser.Parse(line);
        
        Assert.Equal(12, fields.Count);
        Assert.Equal("1012", fields[0]);
        Assert.Equal("Ноутбук, игровой", fields[1]);
        Assert.Equal("Тестовая строка", fields[2]);
    }

    [Fact]
    public void Parse_EscapedQuotes_HandlesCorrectly()
    {
        var line = "1013,\"Название \"\"Special\"\"\",Описание,100,RUB,Категория,5,/img.jpg,true,Brand,12,Model";
        
        var fields = CsvLineParser.Parse(line);
        
        Assert.Equal(12, fields.Count);
        Assert.Equal("1013", fields[0]);
        Assert.Equal("Название \"Special\"", fields[1]);
        Assert.Equal("Описание", fields[2]);
    }
}
