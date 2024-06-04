using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;
using System.Xml;
using System.Xml.Linq;
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
Console.OutputEncoding = Encoding.UTF8;
static Dictionary<string, string> GetDict(string path)
{
    Dictionary<string, string> dict = new();
    if (Path.GetExtension(path) == ".xaml")
    {
        //xaml to dict
        var doc = XElement.Load(path);
        foreach (var item in doc.Descendants())
        {
            if (item.FirstAttribute is not null)
            {
                dict[item.FirstAttribute.Value] = item.Value;
            }

        }
    }
    else if (Path.GetExtension(path) == ".xlsx")
    {
        //xlsx to dict
        using ExcelPackage excel = new ExcelPackage(path);

        using ExcelWorksheet workSheet = excel.Workbook.Worksheets.First();

        for (int i = 2; i < workSheet.Dimension.End.Row; i++)
        {
            string? key = workSheet.Cells[i, 1].Value.ToString();
            string? value = workSheet.Cells[i, 2].Value.ToString();
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                dict[key] = value;
            }
        }
    }



    return dict;
}

switch (args[0])
{
    case "-p":
    case "--path":
        //-p path
        //显示词条
        Display(args);
        break;

    case "-e":
    case "--export":
        //-e filePath newXlsxFilePath 
        //导出词条
        Dictionary<string, string> dict = GetDict(args[1]);
        WriteToXlsx(args[2], dict);
        break;

    case "-g":
    case "--generate":
        //-g xlsxFilePath newFilePath 
        //生成xaml文件
        Generate(args);

        break;

    case "-h":
    case "--help":
    default:
        break;
}


static void WriteToXlsx(string path, Dictionary<string, string> dict)
{
    // Creating an instance 
    // of ExcelPackage 
    using ExcelPackage excel = new ExcelPackage();

    // name of the sheet 
    using ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet1");


    // Setting the properties 
    // of the first row 
    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    workSheet.Row(1).Style.Font.Bold = true;

    // Header of the Excel sheet 
    workSheet.Cells[1, 1].Value = "Key";
    workSheet.Cells[1, 2].Value = "Value";

    // Inserting the article data into excel 
    // sheet by using the for each loop 
    // As we have values to the first row  
    // we will start with second row 
    int recordIndex = 2;

    foreach (var pair in dict)
    {
        workSheet.Cells[recordIndex, 1].Value = pair.Key;
        workSheet.Cells[recordIndex, 2].Value = pair.Value;
        recordIndex++;
    }

    excel.SaveAs(new FileInfo(path));
}


static void Display(string[] args)
{
    string path = args[1];
    Dictionary<string, string> dict = GetDict(path);

    foreach (var item in dict)
    {
        Console.WriteLine(item.Key);
        Console.WriteLine(item.Value);
    }
}

static void Generate(string[] args)
{
    XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
    XNamespace sys = "clr-namespace:System;assembly=mscorlib";
    var doc = XElement.Parse($"""
                <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                            xmlns:x="{x}"
                            xmlns:sys="{sys}"></ResourceDictionary>
        """);
    //node <sys:String x:Key="RemoveSentenceRepeat">句子重复处理</sys:String>
    Dictionary<string, string> dict = GetDict(args[1]);

    foreach (var item in dict)
    {
        var node = new XElement(sys + "String", item.Value);
        node.SetAttributeValue(x + "Key", item.Key);
        doc.Add(node);
    }
    Console.WriteLine(doc);
    using var writer = XmlWriter.Create(args[2], new XmlWriterSettings
    {
        OmitXmlDeclaration = true,
        Indent = true,
    });
    doc.Save(writer);
}