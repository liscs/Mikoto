using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;
using System.Xml;
using System.Xml.Linq;
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
Console.OutputEncoding = Encoding.UTF8;
static Dictionary<string, List<string>> GetDict(string path)
{
    Dictionary<string, List<string>> dict = new();

    if (File.Exists(path))
    {
        if (Path.GetExtension(path) == ".xaml")
        {
            XamlToDict(path, dict);
        }
        else if (Path.GetExtension(path) == ".xlsx")
        {
            XlsxToDict(path, dict);
        }
    }

    if (Directory.Exists(path))
    {
        var files = Directory.GetFiles(path, "*.xaml").Union(Directory.GetFiles(path, "*.xlsx")).ToList();
        files.Sort((x, y) =>
        {
            if (x.Contains("zh-CN")) return -1; // 如果x是"zh-CN"，它排在前面
            if (y.Contains("zh-CN")) return 1;  // 如果y是"zh-CN"，它排在前面
            return string.Compare(x, y);  // 否则按字典顺序排序 
        });
        foreach (var item in files)
        {
            dict = MergeDict(dict, GetDict(item));
        }
    }
    return dict;
}
static Dictionary<string, List<string>> MergeDict(Dictionary<string, List<string>> dict, Dictionary<string, List<string>> dictionary)
{
    int langCount = 0;
    foreach (var item in dictionary)
    {
        if (!dict.TryGetValue(item.Key, out List<string>? value))
        {
            value=new();
            dict[item.Key]=value;
        }

        value.AddRange(item.Value);
        langCount = Math.Max(langCount, value.Count);
    }

    //合并后把数量少的填入空白
    foreach (var item in dict.Values)
    {
        while (item.Count<langCount)
        {
            item.Add(string.Empty);
        }
    }

    return dict;
}

if (args.Length == 0)
{
    args = ["-h"];
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
        Dictionary<string, List<string>> dict = GetDict(args[1]);
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


static void WriteToXlsx(string path, Dictionary<string, List<string>> dict)
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
        for (int i = 0; i < pair.Value.Count; i++)
        {
            workSheet.Cells[recordIndex, i+2].Value = pair.Value[i];
        }
        recordIndex++;
    }

    excel.SaveAs(new FileInfo(path));
}


static void Display(string[] args)
{
    string path = args[1];
    if (File.Exists(path))
    {
        Dictionary<string, List<string>> dict = GetDict(path);

        foreach (var item in dict)
        {
            Console.WriteLine(item.Key);
            item.Value.ForEach(Console.WriteLine);
        }
    }

    if (Directory.Exists(path))
    {
        foreach (var xaml in Directory.GetFiles(path, "*.xaml"))
        {
            Console.WriteLine(xaml);
            Dictionary<string, List<string>> dict = GetDict(xaml);
            foreach (var item in dict)
            {
                Console.WriteLine(item.Key);
                item.Value.ForEach(Console.WriteLine);
            }
            Console.WriteLine();
        }
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
    Dictionary<string, List<string>> dict = GetDict(args[1]);

    foreach (var item in dict)
    {
        var node = new XElement(sys + "String", item.Value[0]);
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

static void XamlToDict(string path, Dictionary<string, List<string>> dict)
{
    string filename = Path.GetFileName(path);
    //xaml to dict
    var doc = XElement.Load(path);
    foreach (var item in doc.Descendants())
    {
        if (item.FirstAttribute is not null)
        {
            if (!dict.TryGetValue(item.FirstAttribute.Value, out List<string>? value))
            {
                value=new();
                dict[item.FirstAttribute.Value]=value;
            }

            value.Add(item.Value);
        }

    }
}

static void XlsxToDict(string path, Dictionary<string, List<string>> dict)
{
    string filename = Path.GetFileName(path);

    //xlsx to dict
    ExcelPackage excel = new ExcelPackage(path);
    ExcelWorksheet workSheet = excel.Workbook.Worksheets.First();
    for (int i = 2; i < workSheet.Dimension.End.Row; i++)
    {
        string? key = workSheet.Cells[i, 1].Value.ToString();
        string? value2 = workSheet.Cells[i, 2].Value.ToString();
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value2))
        {
            if (!dict.TryGetValue(key, out List<string>? value))
            {
                value=new();
                dict[key] = value;
            }

            value.Add(value2);
        }
    }
}
